using System;
using System.Windows;
using System.Windows.Controls;

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
        }


        public void WriteToConsole(string message)
        {
            if (!(TextBlock.CheckAccess()))
            {
                Dispatcher.Invoke(() =>
                    WriteToConsole(message)
                );
                return;
            }
            TextBlock.Text += message + "\r";
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
            if (SourceIdTb.Text.Length == 0 && SourceTypeTb.Text.Length == 0)
            {
                Client.GetRandomRecords();
            }
            else
            {
                int tempVal;
                int? sourceId = int.TryParse(SourceIdTb.Text, out tempVal) ? tempVal : (int?) null;
                int? sourceType = int.TryParse(SourceTypeTb.Text, out tempVal) ? tempVal : (int?)null;
                Client.GetRecords(Client.FormatQueryString(sourceId, sourceType));
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).Text = "";
        }

        private void InsertBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SourceIdTb.Text.Length == 0 || SourceTypeTb.Text.Length == 0)
            {
                return;
            }
            var sourceId = int.Parse(SourceIdTb.Text);
            var sourceType = int.Parse(SourceTypeTb.Text);
            Client.SendRecord(sourceId, sourceType);
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            Client.ConnectAsync();
            StopBtn.IsEnabled = true;
            GetRecordsBtn.IsEnabled = true;
            ConnectBtn.IsEnabled = true;
            InsertBtn.IsEnabled = true;
            StartBtn.IsEnabled = true;
        }
    }
}
