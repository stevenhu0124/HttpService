using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace SignalRService
{
    public partial class HyperService : ServiceBase
    {
        public HyperService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Task.Run(() =>
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", "8888"));
                listener.Start();
                while (listener.IsListening)
                {
                    try
                    {
                        HttpListenerContext context = listener.GetContext();
                        var request = context.Request;
                        using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            var sendData = new ConnectionModel() 
                            { 
                                Action = ConnectionActions.RequestServerAlive,
                                Message = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                                Sender = 8888 
                            };
                            var json = ConvertToJson(sendData);
                            SendResponse(context, json);
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
            });
        }

        protected void SendResponse(HttpListenerContext context, string json)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(json);
            HttpListenerResponse response = context.Response;
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
        }

        protected static string ConvertToJson<T>(T message)
        {
            var parameterString = JsonConvert.SerializeObject(message);
            return parameterString;
        }

        protected override void OnStop()
        {
        }
    }

    internal class ConnectionModel
    {
        public string Message { get; set; }

        public ConnectionActions Action { get; set; }

        public int Sender { get; set; }
    }

    internal enum ConnectionActions
    {
        /// <summary>
        ///  Check primary or secondary have response 
        /// </summary>
        RequestServerAlive,

        /// <summary>
        /// Secondary -> Primary
        /// </summary>
        RequestPrimaryServerConnected,

        /// <summary>
        /// Primary -> Secondary
        /// </summary>
        SendSecondaryNewConnected,
    }
}
