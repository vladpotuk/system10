using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace system9
{
    public partial class MainWindow : Window
    {
        private Mutex mutex = new Mutex();
        private bool firstThreadCompleted = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            FirstThreadOutput.Clear();
            SecondThreadOutput.Clear();
            firstThreadCompleted = false;

            Task.Run(() => DisplayAscending());
            Task.Run(() => DisplayDescending());
        }

        private void DisplayAscending()
        {
            mutex.WaitOne();

            for (int i = 0; i <= 20; i++)
            {
                Dispatcher.Invoke(() => FirstThreadOutput.AppendText($"Перший потік: {i}\n"));
                Thread.Sleep(100);
            }

            firstThreadCompleted = true;
            mutex.ReleaseMutex();
        }

        private void DisplayDescending()
        {
            while (true)
            {
                mutex.WaitOne();

                if (firstThreadCompleted)
                {
                    for (int i = 10; i >= 0; i--)
                    {
                        Dispatcher.Invoke(() => SecondThreadOutput.AppendText($"Другий потік: {i}\n"));
                        Thread.Sleep(100);
                    }

                    mutex.ReleaseMutex();
                    break;
                }

                mutex.ReleaseMutex();
                Thread.Sleep(100);
            }
        }
    }
}