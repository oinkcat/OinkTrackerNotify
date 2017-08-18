using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
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
        private Mutex singleInstanceMutex;

        // Создать мьютекс единственного запуска приложения
        private bool TrySetupMutex()
        {
            bool isMy;
            singleInstanceMutex = new Mutex(true, MutexName, out isMy);

            return isMy;
        }

        // Приложение запущено
        protected async override void OnStartup(StartupEventArgs e)
        {
            // Проверить факт единственного запуска
            if(!TrySetupMutex())
            {
                MessageBox.Show("Приложение уже запущено", "Оповещения Трекера",
                                MessageBoxButton.OK, MessageBoxImage.Exclamation);
                this.Shutdown();
                return;
            }

            base.OnStartup(e);

            var listWindow = new ListWindow();
            Application.Current.MainWindow = listWindow;

            await SettingsStore.Instance.Load();

            if(!SettingsStore.Instance.HasStoredSettings)
            {
                var settingWindow = new LoginDataWindow();
                bool setup = settingWindow.ShowDialog() ?? false;

                if(!setup)
                {
                    Application.Current.Shutdown();
                }
            }

            // Главное окно
            listWindow.StartActivity();
        }
    }
}
