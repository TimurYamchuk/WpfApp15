using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace WpfApp15
{
    public partial class MainWindow : Window
    {
        private Thread _generatorThread;
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_generatorThread != null && _generatorThread.IsAlive)
            {
                MessageBox.Show("Генерация уже выполняется!");
                return;
            }

            _stopEvent.Reset();
            StatusText.Text = "Генерация чисел...";
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;

            _generatorThread = new Thread(GenerateRandomNumbers);
            _generatorThread.Start();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _stopEvent.Set();
        }

        private void GenerateRandomNumbers()
        {
            Random random = new Random();
            string filePath = "numbers.txt";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int i = 0; i < 100; i++)
                {
                    if (_stopEvent.WaitOne(0)) break;

                    int number = random.Next(1, 1000);
                    writer.WriteLine(number);
                    Thread.Sleep(50);
                }
            }

            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Генерация завершена.";
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            });
        }
    }
}
