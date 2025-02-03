using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatRoom.Network
{
    public class Server
    {
        private TcpListener _listener;
        private List<TcpClient> _clients;
        private string _name;
        private Task? _receiver;
        
        public Server(string serverName)
        {
            _listener = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
            _clients = new List<TcpClient>();
            _name = serverName;
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

        public bool WriteMessage(string message)
        {
            using (StreamWriter sw = new StreamWriter($"Servers\\{_name}\\chat.txt", append: true))
            {
                sw.WriteLine(message);
            }

            return true;
        }

        public List<MessageView> ReadMessage()
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
