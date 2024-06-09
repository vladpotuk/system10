using System;
using System.Collections.Concurrent;
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
        private static readonly string reportFilePath = "casinoReport.txt";
        private static readonly int maxPlayersAtTable = 5;
        private static readonly Random random = new Random();

        private static SemaphoreSlim semaphore = new SemaphoreSlim(maxPlayersAtTable);
        private static ConcurrentQueue<Player> playersQueue;
        private static List<Player> allPlayers;
        private static Mutex mutex = new Mutex();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();
            Task.Run(() => StartCasinoSimulation());
        }

        private void StartCasinoSimulation()
        {
            int totalPlayers = random.Next(20, 101);
            allPlayers = new List<Player>();
            playersQueue = new ConcurrentQueue<Player>();

            for (int i = 0; i < totalPlayers; i++)
            {
                playersQueue.Enqueue(new Player(i + 1, random.Next(100, 1001)));
            }

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < maxPlayersAtTable; i++)
            {
                tasks.Add(Task.Run(() => PlayerSimulation()));
            }

            Task.WhenAll(tasks).ContinueWith(t => GenerateReport());
        }

        private void PlayerSimulation()
        {
            while (playersQueue.TryDequeue(out Player player))
            {
                semaphore.Wait();
                try
                {
                    PlayGame(player);
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        private void PlayGame(Player player)
        {
            int betAmount = random.Next(1, player.Money + 1);
            int betNumber = random.Next(0, 37);
            int winningNumber = random.Next(0, 37);

            if (betNumber == winningNumber)
            {
                player.Money += betAmount;
            }
            else
            {
                player.Money -= betAmount;
            }

            allPlayers.Add(player);
            Dispatcher.Invoke(() =>
            {
                OutputTextBox.AppendText($"Гравець{player.Id} [початкова сума: {player.InitialMoney}] [кінцева сума: {player.Money}]\n");
            });
        }

        private void GenerateReport()
        {
            mutex.WaitOne();
            try
            {
                var reportLines = allPlayers.Select(p => $"Гравець{p.Id} [початкова сума: {p.InitialMoney}] [кінцева сума: {p.Money}]").ToList();
                File.WriteAllLines(reportFilePath, reportLines);

                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText("День закінчено. Звіт збережено у файл.\n");
                });
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        private class Player
        {
            public int Id { get; }
            public int InitialMoney { get; }
            public int Money { get; set; }

            public Player(int id, int initialMoney)
            {
                Id = id;
                InitialMoney = initialMoney;
                Money = initialMoney;
            }
        }
    }
}
