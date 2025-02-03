using System.Windows;
using System.Windows.Controls;
using ChatRoom.Network;

namespace ChatRoom
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Server? _activeServer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void HostServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem button && button.Header.ToString() == "Existing")
            {
                MessageBox.Show($"Current active connections {_activeServer?.ActiveConnections()}");
                return;
            }

            _activeServer = new Server("Test");
            if (!_activeServer.Create())
            {
                MessageBox.Show("Error", "Cannot create the new server", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            _activeServer.Start(ChatBox);
            IpLabel.Content = _activeServer.IP;
            PortLabel.Content = _activeServer.Port;
        }

        private void JoinServerButton_Click(object sender, RoutedEventArgs e)
        {
            _activeServer?.Stop();
            ChatBox.Items.Clear();
            IpLabel.Content = "None";
            PortLabel.Content = "None";
        }
    }
}