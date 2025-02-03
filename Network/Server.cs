using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows.Controls;


namespace ChatRoom.Network
{
    public class Server
    {
        private TcpListener _listener;
        private List<TcpClient> _clients;
        private string _name;
        private object mutex = new object();
        public string IP { get; private set; } = "";
        public string Port { get; private set; } = "";

        public CancellationTokenSource Token { get; }
        
        public Server(string serverName)
        {
            _listener = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
            _clients = new List<TcpClient>();
            _name = serverName;
            Token = new CancellationTokenSource();
        }

        /// <summary>
        /// Create tries to create the Server directory with the Server Name provided in the constructor.
        /// </summary>
        /// <returns>true if it created the directory successfully, otherwise false.</returns>
        public bool Create()
        {
            var svs = Directory.GetDirectories("Servers");

            if (svs.Any(x => x == $"Servers\\{_name}"))
            {
                return false;
            }

            Directory.CreateDirectory($"Servers\\{_name}");

            // Ensure the creation of the chat file
            var f = File.Create($"Servers\\{_name}\\chat.txt");

            // We close the stream so that it can be used.
            f.Close();

            return true;
        }
        public int ActiveConnections()
        {
            return _clients.Count;
        }

        public void Start()
        {
            _listener.Start();

            // We get the data of the server so that we know where to connect to
            IPEndPoint localEndPoint = (IPEndPoint)_listener.LocalEndpoint;
            Port = localEndPoint.Port.ToString();

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Make sure the interface is operational and not a loopback or virtual interface
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    // Get the IP properties for this network interface
                    IPInterfaceProperties ipProperties = ni.GetIPProperties();

                    foreach (UnicastIPAddressInformation ip in ipProperties.UnicastAddresses)
                    {
                        // Look for IPv4 addresses (ignoring loopback address 127.0.0.1 and other conditions)
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !ip.Address.ToString().StartsWith("127"))
                        {
                            IP = ip.Address.ToString();
                            break;
                        }
                    }
                }
            }

            // We start a Task running in the background that will handle incoming requests.
            Task.Factory.StartNew(() => 
            {
                while (true)
                {
                    if (Token.IsCancellationRequested)
                    {
                        break;
                    }
                    TcpClient client;

                    try
                    {
                        client = _listener.AcceptTcpClient();
                        _clients.Add(client);
                    }
                    catch
                    {
                        // In case of any errors we simply break and let it crash
                        return;
                    }

                    // We start a Task for each client to listen to incoming messages
                    Task.Factory.StartNew(() =>
                    {
                        var thisClient = client;
                        var buff = new byte[4096];
                        while (true)
                        {
                            if (Token.IsCancellationRequested)
                            {
                                break;
                            }

                            try
                            {
                                // In some cases the client may close the connection before reading EOF
                                // thus it will throw an IOException, but we don't really care because it will simply crash the task
                                var read = thisClient.GetStream().Read(buff);
                                // If we read 0, it means the connection closed.
                                if (read == 0)
                                    break;

                                var json = Encoding.UTF8.GetString(buff, 0, read);

                                // We write to the file
                                lock (mutex)
                                {
                                    WriteMessage(json);
                                }

                                lock (mutex)
                                {
                                    foreach (var client in _clients)
                                    {
                                        client.GetStream().WriteAsync(buff[..read]);
                                    }
                                }

                                //// We add the visual
                                //chatbox.Dispatcher.Invoke(() =>
                                //{
                                //    chatbox.Items.Add(new MessageView(MessageManager.FromJson(json)));
                                //});
                            }
                            catch
                            {
                                thisClient.Close();
                                // In case of any error, we simply let the task crash, and remove the client from the list.
                                _clients.Remove(thisClient);
                                return;
                            }
                        }
                    }, Token.Token);
                }
            }, Token.Token);
        }

        public void Stop()
        {
            _listener.Stop();
            Token.Cancel();
            _listener.Dispose();
        }

        private bool WriteMessage(string message)
        {
            using (StreamWriter sw = new StreamWriter($"Servers\\{_name}\\chat.txt", append: true))
            {
                sw.WriteLine(message);
            }
            return true;
        }

        public List<MessageView> ReadChat()
        {
            var messages = new List<MessageView>();
            using (StreamReader sr = new StreamReader($"Servers\\{_name}\\chat.txt"))
            {
                while (sr.ReadLine() is string msg)
                {
                    messages.Add(new MessageView(MessageManager.FromJson(msg)));
                }
                return messages;
            }
        }
    }
}
