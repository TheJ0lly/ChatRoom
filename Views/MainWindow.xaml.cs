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


            _activeServer.Start();
            

            // Add the join window here, to create a new user.

            var detChan = Channel.CreateBounded<string>(1);
            var JSW = new JoinServerWindow(false);
            JSW.ServerDetailsChan = detChan;

            var joinSuccess = JSW.ShowDialog();

            if (joinSuccess is null || !joinSuccess.Value)
            {
                return;
            }
            var username = await detChan.Reader.ReadAsync();

            IpLabel.Content = _activeServer.IP;
            PortLabel.Content = _activeServer.Port;
            UserLabel.Content = username;

            _activeClient = new Client(username);
            _activeClient.Connect(_activeServer.IP, int.Parse(_activeServer.Port));
            _activeClient.Run(ChatBox);
        }

        private async void JoinServerButton_Click(object sender, RoutedEventArgs e)
        {
            var detChan = Channel.CreateBounded<string>(3);
            var JSW = new JoinServerWindow(true);
            JSW.ServerDetailsChan = detChan;

            var joinSuccess = JSW.ShowDialog();

            if (joinSuccess is null || !joinSuccess.Value)
            {
                return;
            }

            var username = await detChan.Reader.ReadAsync();
            var ip = await detChan.Reader.ReadAsync();
            var port = await detChan.Reader.ReadAsync();

            _activeClient = new Client(username);
            if (!_activeClient.Connect(ip, int.Parse(port)))
            {
                MessageBox.Show("Failed to join server!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IpLabel.Content = ip;
            PortLabel.Content = port;
            UserLabel.Content = username;

            _activeClient.Run(ChatBox);
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

            try
            {
                _activeClient.Send(msg);
            }
            catch
            {
                MessageBox.Show($"Server cannot be reached", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ChatBox.Items.Clear();
                _activeClient.Disconnect();
                _activeClient = null;
                IpLabel.Content = "None";
                PortLabel.Content = "None";
                UserLabel.Content = "None";
            }

            // After we send the message we clear the bar
            tb.Text = string.Empty;
        }

        private void CloseConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ChatBox.Items.Clear();
            IpLabel.Content = "None";
            PortLabel.Content = "None";
            UserLabel.Content = "None";
            _activeClient?.Disconnect();
            _activeClient = null;
            _activeServer?.Stop();
            _activeServer = null;
        }
    }
}