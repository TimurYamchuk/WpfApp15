using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp15
{
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            LoadDrives();
            buttonStop.IsEnabled = false;
        }

        private void LoadDrives()
        {
            comboBoxDrives.Items.Clear();
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                comboBoxDrives.Items.Add(drive.Name);

            if (comboBoxDrives.Items.Count > 0)
                comboBoxDrives.SelectedIndex = 0;
        }

        private async void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            listViewResults.Items.Clear();
            buttonStop.IsEnabled = true;
            buttonStop.Content = "Остановить";

            try
            {
                await Task.Run(() => FindFiles(token), token);
            }
            catch (OperationCanceledException) { }
            finally
            {
                buttonStop.IsEnabled = false;
                buttonStop.Content = "Поиск";
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
        }

        private void FindFiles(CancellationToken token)
        {
            string mask = Dispatcher.Invoke(() => textBoxMask.Text.Trim());
            string disk = Dispatcher.Invoke(() => comboBoxDrives.Text);
            bool searchSubDirs = Dispatcher.Invoke(() => checkBoxSubDirs.IsChecked ?? false);
            string phrase = Dispatcher.Invoke(() => textBoxPhrase.Text.Trim());

            if (string.IsNullOrEmpty(disk) || !Directory.Exists(disk))
            {
                Dispatcher.Invoke(() => MessageBox.Show("Диск не найден или недоступен."));
                return;
            }

            var searchOption = searchSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            try
            {
                foreach (var filePath in Directory.EnumerateFiles(disk, mask, searchOption))
                {
                    if (token.IsCancellationRequested) return;

                    if (!string.IsNullOrEmpty(phrase) && !FileContainsPhrase(filePath, phrase))
                        continue;

                    Dispatcher.Invoke(() => listViewResults.Items.Add(new FileInfo(filePath).Name));
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Ошибка доступа к файлу: " + ex.Message));
            }
        }
        private void textBoxMask_TextChanged(object sender, TextChangedEventArgs e)
        {
  
        }

        private bool FileContainsPhrase(string filePath, string phrase)
        {
            try
            {
                return File.ReadLines(filePath).Any(line => line.Contains(phrase));
            }
            catch
            {
                return false;
            }
        }
    }
}
