using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace system10
{
    public partial class MainWindow : Window
    {
        private static readonly string firstFilePath = "firstFile.txt";
        private static readonly string secondFilePath = "secondFile.txt";
        private static readonly string thirdFilePath = "thirdFile.txt";

        private static Mutex mutex = new Mutex();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();
            Task.Run(() => FirstThreadWork());
        }

        private void FirstThreadWork()
        {
            mutex.WaitOne();
            try
            {
                var randomNumbers = GenerateRandomNumbers(100);
                File.WriteAllLines(firstFilePath, randomNumbers.Select(n => n.ToString()));

                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText("Перший потік завершив роботу. Згенеровано випадкові числа та записано у файл.\n");
                });
            }
            finally
            {
                mutex.ReleaseMutex();
                Task.Run(() => SecondThreadWork());
            }
        }

        private void SecondThreadWork()
        {
            mutex.WaitOne();
            try
            {
                var numbers = File.ReadAllLines(firstFilePath).Select(int.Parse).ToList();
                var primeNumbers = numbers.Where(IsPrime).ToList();
                File.WriteAllLines(secondFilePath, primeNumbers.Select(n => n.ToString()));

                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText("Другий потік завершив роботу. Зібрано прості числа та записано у новий файл.\n");
                });
            }
            finally
            {
                mutex.ReleaseMutex();
                Task.Run(() => ThirdThreadWork());
            }
        }

        private void ThirdThreadWork()
        {
            mutex.WaitOne();
            try
            {
                var primeNumbers = File.ReadAllLines(secondFilePath).Select(int.Parse).ToList();
                var filteredNumbers = primeNumbers.Where(n => n % 10 == 7).ToList();
                File.WriteAllLines(thirdFilePath, filteredNumbers.Select(n => n.ToString()));

                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText("Третій потік завершив роботу. Зібрано прості числа, остання цифра яких 7, та записано у новий файл.\n");
                });
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private List<int> GenerateRandomNumbers(int count)
        {
            Random random = new Random();
            List<int> numbers = new List<int>();
            for (int i = 0; i < count; i++)
            {
                numbers.Add(random.Next(1, 1000));
            }
            return numbers;
        }

        private bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            for (int i = 3; i <= Math.Sqrt(number); i += 2)
            {
                if (number % i == 0) return false;
            }
            return true;
        }
    }
}
