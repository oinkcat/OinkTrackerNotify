using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TrackerNotify
{
    /// <summary>
    /// Представляет приложение
    /// </summary>
    public partial class App : Application
    {
        // Приложение запущено
        protected async override void OnStartup(StartupEventArgs e)
        {
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
