using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DBClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Client.ConnectAsync();
        }


        public void WriteToConsole(string message)
        {
            if (!(textBlock.CheckAccess()))
            {
                Dispatcher.Invoke(() =>
                    WriteToConsole(message)
                );
                return;
            }
            textBlock.Text += message + "\r";
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            Client.StartSending();
        }

        private void stopBtn_Click(object sender, RoutedEventArgs e)
        {
            Client.StopSending();
        }

        private void getRecordsBtn__Click(object sender, RoutedEventArgs e)
        {
            Client.GetRecords();
        }
    }
}
