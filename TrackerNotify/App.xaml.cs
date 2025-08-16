using System;
using System.Threading;
using System.Windows;

namespace TrackerNotify
{
    /// <summary>
    /// Представляет приложение
    /// </summary>
    public partial class App : Application
    {
        // Название мьютекса приложения
        private const string MutexName = "Softcat.TrackerNotify";

        // Мьютекс, определяющий факт запуска приложения
        private static Mutex singleInstanceMutex;

        // Создать мьютекс единственного запуска приложения
        private static bool TrySetupMutex()
        {
            singleInstanceMutex = new Mutex(true, MutexName, out bool isOwned);
            return isOwned;
        }

        // Приложение запущено
        protected async override void OnStartup(StartupEventArgs e)
        {
            // Проверить факт единственного запуска
            if(!TrySetupMutex())
            {
                MessageBox.Show("Приложение уже запущено",
                                "Оповещения Трекера",
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
                Shutdown();
                return;
            }

            base.OnStartup(e);

            await SettingsStore.Instance.Load();
            bool canStart = true;

            // В случае отсутствия настроек - запросить адрес трекера
            if(!SettingsStore.Instance.HasStoredSettings)
            {
                var settingWindow = new LoginDataWindow();
                bool setup = settingWindow.ShowDialog() ?? false;

                if(!setup)
                {
                    canStart = false;
                    Shutdown();
                }
            }

            // Главное окно
            if(canStart)
            {
                var listWindow = new ListWindow();
                MainWindow = listWindow;
                listWindow.StartActivity();
            }
        }
    }
}
