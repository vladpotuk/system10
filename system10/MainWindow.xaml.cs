using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace system10
{
    public partial class MainWindow : Window
    {
        private Semaphore semaphore = new Semaphore(3, 3); 

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();

            for (int i = 0; i < 10; i++)
            {
                int threadId = i;
                Task.Run(() => ThreadWork(threadId));
            }
        }

        private void ThreadWork(int threadId)
        {
            semaphore.WaitOne();

            try
            {
                Random random = new Random();

                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Потік {threadId} розпочав роботу.\n");
                });

                for (int i = 0; i < 5; i++)
                {
                    int randomValue = random.Next(100);
                    Dispatcher.Invoke(() =>
                    {
                        OutputTextBox.AppendText($"Потік {threadId} генерує випадкове число: {randomValue}\n");
                    });
                    Thread.Sleep(500); 
                }

                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"Потік {threadId} завершив роботу.\n");
                });
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
