using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

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

            string response = HandleRequest(request);

            using(IOutputStream output = socket.OutputStream)
            {
                await WriteResponseAsync(response, output);
            }
        }

        private async Task WriteResponseAsync(string body, IOutputStream outputStream)
        {
            using (Stream response = outputStream.AsStreamForWrite())
            {
                byte[] headers = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\nCONTENT-Length: {0}\r\nConnection: close\r\n\r\n");
                await response.WriteAsync(headers, 0, headers.Length);
                StreamWriter writer = new StreamWriter(response);
                await writer.WriteAsync(body);
                await response.FlushAsync();
            }
        }

        private string HandleRequest(string request)
        {
            string[] requestParts = request.Split(' ');
            // do stuff with request
            return "";
        }
    }
}
