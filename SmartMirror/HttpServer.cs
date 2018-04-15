using System;
using System.Collections.Generic;
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
                System.Diagnostics.Debug.Write(setting);
                System.Diagnostics.Debug.Write(value);
                return await serializeResult(setting, UserAccount.saveSetting(setting, value).ToString());
            }
            return "";
        }

        private async Task<string> serializeResult(string setting, string value)
        {
            if (setting == "zipcode")
            {
                return await GoogleAPIAdapter.zipcode(value);
            }

            if (setting == "google")
            {
                GoogleAPIAdapter.authorize(value);
            }


            return "";
        }
    }
}
