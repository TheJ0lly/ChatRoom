using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for NewServerWindow.xaml
    /// </summary>
    public partial class NewServerWindow : Window
    {
        public Channel<string>? ServerNameChan { get; set; }

        public NewServerWindow()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ServerNameBox.Text.Contains(' '))
            {
                MessageBox.Show("Server name cannot contain spaces", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (ServerNameBox.Text == string.Empty)
            {
                MessageBox.Show("Server name cannot be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ServerNameChan?.Writer.WriteAsync(ServerNameBox.Text);
            DialogResult = true;
            Close();
        }
    }
}
