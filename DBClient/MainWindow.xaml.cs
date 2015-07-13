using System.Windows;

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
            Client.GetRecords();
        }
    }
}
