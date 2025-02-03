using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for ExistingServerWindow.xaml
    /// </summary>
    public partial class ExistingServerWindow : Window
    {
        public Channel<string>? ServerNameChan { get; set; }
        public ExistingServerWindow()
        {
            InitializeComponent();
            var dirs = Directory.GetDirectories("Servers");

            var correctDirs = dirs.Select(d => d.Remove(0, 8));

            ServersBox.ItemsSource = correctDirs;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (ServersBox.SelectedItem is null)
            {
                MessageBox.Show("Must choose a server name", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ServerNameChan?.Writer.WriteAsync((string)ServersBox.SelectedItem);
            DialogResult = true;
            Close();
        }
    }
}
