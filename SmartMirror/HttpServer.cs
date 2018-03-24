using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace SmartMirror
{
    public class HttpServer : IDisposable
    {
        private const uint bufferSize = 4096;
        private StreamSocketListener listener;
        private const string googleMapsKey = "AIzaSyCf7aTd-ZPp6eEKeLNRIKgKPOj0oBMCd2M";

        public HttpServer(int port)
        {
            listener = new StreamSocketListener();
            listener.ConnectionReceived += (s, e) => ProcessRequestAsync(e.Socket);
            listener.BindServiceNameAsync(port.ToString());
        }

        public void Dispose()
        {
            listener.Dispose();
        }

        private async void ProcessRequestAsync(StreamSocket socket)
        {
            string request;
            using(IInputStream input = socket.InputStream)
            {
                byte[] data = new byte[bufferSize];
                IBuffer buffer = data.AsBuffer();
                await input.ReadAsync(buffer, bufferSize, InputStreamOptions.Partial);
                request = Encoding.UTF8.GetString(data, 0, data.Length);
            }

            string response = await HandleRequest(request);

            using(IOutputStream output = socket.OutputStream)
            {
                await WriteResponseAsync(response, output);
            }
        }

        private async Task WriteResponseAsync(string body, IOutputStream outputStream)
        {
            using (Stream response = outputStream.AsStreamForWrite())
            {
                byte[] resp = Encoding.UTF8.GetBytes(
                    "HTTP/1.1 200 OK\r\n" + 
                    "Content-Length: " + body.Length + "\r\n" + 
                    "Connection: close\r\n" +
                    "Content-Type: text/html\r\n" +
                    "\r\n" +
                    body);
                await response.WriteAsync(resp, 0, resp.Length);
                await response.FlushAsync();
            }
        }

        private async Task<string> HandleRequest(string request)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            System.Diagnostics.Debug.Write(request);
            string[] requestParts = request.Split(' ');
            string method = requestParts[0];
            string url = requestParts[1];
            string setting = url.Substring(url.IndexOf("/settings/") + 10);

            if (method == "GET")
            {
                return await serializeResult(setting, UserAccount.getSetting(setting).ToString());
            }

            if (method == "POST" || method == "PUT" || method == "PATCH")
            {
                string value = setting.Split('=')[1];
                setting = setting.Split('?')[0];
                return await serializeResult(setting, UserAccount.saveSetting(setting, value).ToString());
            }
            return "";
        }

        private async Task<string> serializeResult(string setting, string value)
        {
            if (setting == "zipcode")
            {
                HttpClient httpClient = new HttpClient();
                var headers = httpClient.DefaultRequestHeaders;
                Uri requestUri = new Uri("https://maps.googleapis.com/maps/api/geocode/json?address=" + value + "&key=" + googleMapsKey);
                HttpResponseMessage response = new HttpResponseMessage();
                string body = "";

                try
                {
                    response = await httpClient.GetAsync(requestUri);
                    response.EnsureSuccessStatusCode();
                    body = await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    body = "Error: " + e.HResult.ToString("X") + " Message: " + e.Message;
                }

                JsonObject obj = JsonValue.Parse(body).GetObject();
                JsonObject j = obj.GetNamedArray("results").GetObjectAt(0);
                double lat = j.GetNamedObject("geometry").GetNamedObject("location").GetNamedNumber("lat");
                double lng = j.GetNamedObject("geometry").GetNamedObject("location").GetNamedNumber("lng");
                string locality = "";
                JsonArray address = j.GetNamedArray("address_components");
                foreach (var comp in address)
                {
                    JsonArray types = comp.GetObject().GetNamedArray("types");
                    foreach (var type in types)
                    {
                        if (type.GetString() == "locality")
                        {
                            locality = comp.GetObject().GetNamedString("long_name");
                        }
                    }
                }

                JsonObject zipResult = new JsonObject();
                zipResult.SetNamedValue("locality", JsonValue.CreateStringValue(locality));
                zipResult.SetNamedValue("lat", JsonValue.CreateNumberValue(lat));
                zipResult.SetNamedValue("lng", JsonValue.CreateNumberValue(lng));
                return zipResult.ToString();
            }


            return "";
        }
    }
}
