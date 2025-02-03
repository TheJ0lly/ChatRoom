using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using ChatRoom.Network;
using ChatRoom.Views;

namespace ChatRoom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server? _activeServer;
        private Client? _activeClient;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void HostServerButton_Click(object sender, RoutedEventArgs e)
        {
            var choice = e.OriginalSource as MenuItem;

            if (choice is null)
            {
                return;
            }

            var serverName = "";
            var create = false;
            if (choice.Header.ToString() == "New")
            {
                var nameChan = Channel.CreateBounded<string>(1);
                var NSW = new NewServerWindow();
                NSW.ServerNameChan = nameChan;

                var success = NSW.ShowDialog();

                if (success is null || !success.Value)
                {
                    return;
                }

                // We get the new server name.
                serverName = await nameChan.Reader.ReadAsync();

                create = true;
            }
            else if (choice.Header.ToString() == "Existing")
            {
                var nameChan = Channel.CreateBounded<string>(1);
                var ESW = new ExistingServerWindow();
                ESW.ServerNameChan = nameChan;

                var success = ESW.ShowDialog();

                if (success is null || !success.Value) 
                { 
                    return; 
                }


                // We get the new server name.
                serverName = await nameChan.Reader.ReadAsync();
            }
            else
            {
                MessageBox.Show("Unknown button press", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _activeServer = new Server(serverName);

            // If we need to create the server, we check for both conditions
            if (create)
            {
                if (!_activeServer.Create())
                {
                    MessageBox.Show("Cannot create the new server", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                var msgs = _activeServer.ReadChat();

                foreach (var msg in msgs)
                {
                    ChatBox.Items.Add(msg);
                }
            }


            _activeServer.Start();
            IpLabel.Content = _activeServer.IP;
            PortLabel.Content = _activeServer.Port;

            // Add the join window here, to create a new user.

            _activeClient = new Client("admin");
            _activeClient.Connect(_activeServer.IP, int.Parse(_activeServer.Port));
            _activeClient.Run(ChatBox);
        }

        private void JoinServerButton_Click(object sender, RoutedEventArgs e)
        {
            _activeServer?.Stop();
            ChatBox.Items.Clear();
            IpLabel.Content = "None";
            PortLabel.Content = "None";
        }

        private void ToSendBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is not System.Windows.Input.Key.Enter)
            {
                return;
            }

            var tb = sender as TextBox;


            if (tb is null || _activeClient is null)
            {
                return;
            }

            var msg = new Message();
            msg.Text = tb.Text;
            msg.User = _activeClient.User;
            msg.Time = DateTime.Now.ToString("g");

            _activeClient.Send(msg);

            // After we send the message we clear the bar
            tb.Text = string.Empty;
        }
    }
}