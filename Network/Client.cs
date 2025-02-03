using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ChatRoom.Network
{
    public class Client
    {
        private TcpClient _client;
        public CancellationTokenSource Token { get; }

        public string User { get; set; }

        public Client(string User)
        {
            _client = new TcpClient();
            Token = new CancellationTokenSource();
            this.User = User;
        }

        public void Connect(string IpAddress, int port)
        {
            _client.Connect(IpAddress, port);
        }

        public void Run(ListBox chatbox)
        {
            // The read loop
            Task.Factory.StartNew(() =>
            {
                var buff = new byte[4096];

                while (true)
                {
                    if (Token.IsCancellationRequested)
                    {
                        break;
                    }

                    try
                    {
                        var read = _client.GetStream().Read(buff);
                        if (read == 0)
                        {
                            break;
                        }
                        
                        var json = Encoding.UTF8.GetString(buff, 0, read);

                        var jsonSplit = json.Split('\n');

                        if (jsonSplit.Length > 1)
                        {
                            foreach (var line in jsonSplit)
                            {
                                if (line == string.Empty)
                                {
                                    continue;
                                }
                                // We add the visual
                                chatbox.Dispatcher.Invoke(() =>
                                {
                                    chatbox.Items.Add(new MessageView(MessageManager.FromJson(line)));
                                });
                            }
                        }
                        else
                        {
                            // We add the visual
                            chatbox.Dispatcher.Invoke(() =>
                            {
                                chatbox.Items.Add(new MessageView(MessageManager.FromJson(json)));
                            });
                        }

                    }
                    catch
                    {
                        break;
                    }
                }
            }, Token.Token);
        }

        public void Send(Message message)
        {
            var json = MessageManager.ToJson(message);

            var buff = Encoding.UTF8.GetBytes(json);

            _client.GetStream().WriteAsync(buff, 0, buff.Length);
        }

        public void Disconnect()
        {
            _client.Close();
            _client.Dispose();
        }
    }
}
