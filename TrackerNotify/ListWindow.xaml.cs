using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using System.Diagnostics;
using System.Drawing;
using TrackerNotify.Model;

using AppResources = TrackerNotify.Properties.Resources;

// Из System.Windows.Forms
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using MouseButtons = System.Windows.Forms.MouseButtons;
using WFContextMenu = System.Windows.Forms.ContextMenuStrip;
using WFMenuItem = System.Windows.Forms.ToolStripMenuItem;
using ToolTipIcon = System.Windows.Forms.ToolTipIcon;
using WFFontStyle = System.Drawing.FontStyle;

namespace TrackerNotify
{
    /// <summary>
    /// Отображает последние действия в виде списка
    /// <remarks>Главное окно</remarks>
    /// </summary>
    public partial class ListWindow : Window
    {
        // Интервал проверки новых действий
        private const int CheckIntervalSeconds = 30;

        // Таймер для проверки недавних событий
        private DispatcherTimer checkTimer;

        // Модель представления данных окна
        private ActionsViewModel viewModel;

        // Всплывающее окно уведомления
        private PopupWindow popupNotifyWindow;

        // Иконка в трее
        private NotifyIcon trayIcon;

        // Часть общей функциональности окна
        private WindowOverTrayHelper uiHelper;

        /// <summary>
        /// Запустить функциональность проверки недавних действий
        /// </summary>
        public void StartActivity()
        {
            this.viewModel = new ActionsViewModel();
            this.uiHelper = new WindowOverTrayHelper(this, viewModel);
            this.DataContext = viewModel;
            this.popupNotifyWindow = new PopupWindow(viewModel);
            SetupNotifyIcon();

            if(SettingsStore.Instance.IsCleanRun)
            {
                ShowNotifyBaloon();
            }

            // Таймер проверки новых действий
            checkTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            checkTimer.Interval = TimeSpan.FromSeconds(CheckIntervalSeconds);
            checkTimer.Tick += checkTimer_Tick;
            checkTimer.Start();

            // Проверить сразу же
            checkTimer_Tick(this, EventArgs.Empty);
        }

        // Показать всплывающее сообщение при запуске
        private void ShowNotifyBaloon()
        {
            const string BaloonTitle = "Оповещения Трекера";
            const string BaloonText = "Приложение успешно настроено";
            const ToolTipIcon Icon = ToolTipIcon.Info;

            trayIcon.ShowBalloonTip(2000, BaloonTitle, BaloonText, Icon);
        }

        // Показать иконку в трее
        private void SetupNotifyIcon()
        {
            trayIcon = new NotifyIcon();

            // Значок
            trayIcon.MouseDown += trayIcon_MouseDown;
            trayIcon.Text = "Оповещения Трекера";
            trayIcon.Icon = AppResources.notify;
            trayIcon.Visible = true;

            // Менюшка
            var ctxMenu = new WFContextMenu();
            var openUrlItem = new WFMenuItem("О&ткрыть сайт Трекера");
            openUrlItem.Font = new Font(openUrlItem.Font, WFFontStyle.Bold);
            openUrlItem.Image = AppResources.open_png;
            openUrlItem.Click += openUrlItem_Click;
            ctxMenu.Items.Add(openUrlItem);
            ctxMenu.Items.Add("-");
            var settingsItem = new WFMenuItem("Наст&ройки...");
            settingsItem.Image = AppResources.settings_png;
            settingsItem.Click += settingsItem_Click;
            ctxMenu.Items.Add(settingsItem);
            ctxMenu.Items.Add("-");
            var exitItem = new WFMenuItem("В&ыход");
            exitItem.Click += new EventHandler(exitItem_Click);
            ctxMenu.Items.Add(exitItem);
            trayIcon.ContextMenuStrip = ctxMenu;
        }

        #region Пункты меню
        // Выбран пункт показа окна настроек
        private void settingsItem_Click(object sender, EventArgs e)
        {
            new LoginDataWindow().ShowDialog();
        }

        // Выбрано открытие сайта Трекера
        private void openUrlItem_Click(object sender, EventArgs e)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = SettingsStore.Instance.TrackerEnterUrl,
                Verb = "Open",
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        // Выбран выход из приложения
        private void exitItem_Click(object sender, EventArgs e)
        {
            viewModel.IsAppShutdown = true;
            this.Close();
        }
        #endregion

        // Окно потеряло фокус
        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Hide();
        }

        // Выполнен щелчок на значке в трее
        private void trayIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                if (!IsVisible)
                {
                    if(popupNotifyWindow.IsVisible)
                    {
                        popupNotifyWindow.Hide();
                    }
                    this.Show();
                    this.Activate();
                }
            }
        }

        // Срабатывание таймера проверки
        private async void checkTimer_Tick(object sender, EventArgs e)
        {
            await viewModel.CheckForActions();
        }

        // Окно закрывается
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if(viewModel.IsAppShutdown)
            {
                trayIcon.Dispose();
                popupNotifyWindow.Close();
            }
        }

        public ListWindow()
        {
            InitializeComponent();
        }
    }
}
