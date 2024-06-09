using System;
using System.Threading;
using System.Windows;

namespace system10
{
    public partial class MainWindow : Window
    {
        
        private static Semaphore semaphore = new Semaphore(3, 3, "LimitedInstancesAppSemaphore");

        public MainWindow()
        {
            if (!semaphore.WaitOne(TimeSpan.Zero))
            {
                
                MessageBox.Show("Додаток вже запущено в трьох копіях. Нова копія не може бути запущена.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
                return;
            }

            InitializeComponent();
            this.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
            semaphore.Release();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
           
            this.Close();
        }
    }
}
