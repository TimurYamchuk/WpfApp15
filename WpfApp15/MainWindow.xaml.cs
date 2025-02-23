using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp15
{
    public partial class MainWindow : Window
    {
        private Thread _searchThread;
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);

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

        private void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (_searchThread != null && _searchThread.IsAlive)
            {
                MessageBox.Show("Поиск уже выполняется!");
                return;
            }

            _stopEvent.Reset();
            listViewResults.Items.Clear();
            buttonStop.IsEnabled = true;
            buttonStop.Content = "Остановить";

            _searchThread = new Thread(FindFiles);
            _searchThread.Start();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            _stopEvent.Set();
        }

        private void FindFiles()
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
                    if (_stopEvent.WaitOne(0)) return;

                    if (!string.IsNullOrEmpty(phrase) && !FileContainsPhrase(filePath, phrase))
                        continue;

                    var fileInfo = new FileInfo(filePath);
                    Dispatcher.Invoke(() => listViewResults.Items.Add(new FileModel(
                        fileInfo.Name, fileInfo.DirectoryName, 
                        (fileInfo.Length / 1024).ToString(), 
                        fileInfo.LastWriteTime.ToString())));
                }
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Ошибка доступа к файлу: " + ex.Message));
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    buttonStop.IsEnabled = false;
                    buttonStop.Content = "Поиск";
                });
            }
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
