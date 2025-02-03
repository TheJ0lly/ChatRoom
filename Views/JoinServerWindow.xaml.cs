using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatRoom.Views
{
    /// <summary>
    /// Interaction logic for JoinServerWindow.xaml
    /// </summary>
    public partial class JoinServerWindow : Window
    {
        public Channel<string>? ServerDetailsChan { get; set; }
        private bool _needIpAndPort { get; set; }
        public JoinServerWindow(bool NeedIPAndPort)
        {
            InitializeComponent();
            _needIpAndPort = NeedIPAndPort;
            if (!NeedIPAndPort)
            {
                IpBox.Visibility = Visibility.Collapsed;
                IpLabel.Visibility = Visibility.Collapsed;
                PortBox.Visibility = Visibility.Collapsed;
                PortLabel.Visibility = Visibility.Collapsed;
                Height = 140;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ServerDetailsChan is null)
            {
                return;
            }
            if (UserBox.Text == string.Empty)
            {
                MessageBox.Show("Must pass a username", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (UserBox.Text.Contains(' '))
            {
                MessageBox.Show("Username must not contain spaces", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_needIpAndPort)
            {
                if (IpBox.Text == string.Empty)
                {
                    MessageBox.Show("Must pass an IP address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!IPAddress.TryParse(IpBox.Text, out var o))
                {
                    MessageBox.Show("Must pass a valid IP address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (PortBox.Text == string.Empty)
                {
                    MessageBox.Show("Must pass a port", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(PortBox.Text, out var n))
                {
                    MessageBox.Show("Port must be a number", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (n < 1 || n > Math.Pow(2, n) - 1)
                {
                    MessageBox.Show("Port must be over 0 and below 65535", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            }

            ServerDetailsChan.Writer.WriteAsync(UserBox.Text);

            if (_needIpAndPort)
            {
                ServerDetailsChan.Writer.WriteAsync(IpBox.Text);
                ServerDetailsChan.Writer.WriteAsync(PortBox.Text);
            }

            DialogResult = true;
            Close();
        }
    }
}
