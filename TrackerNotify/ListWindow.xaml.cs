using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TrackerNotify.Model;

using NotifyIcon = System.Windows.Forms.NotifyIcon;

namespace TrackerNotify
{
    /// <summary>
    /// Отображает последние действия в виде списка
    /// <remarks>Главное окно</remarks>
    /// </summary>
    public partial class ListWindow : Window
    {
        private const int CheckIntervalSeconds = 20;

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

            checkTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            checkTimer.Interval = TimeSpan.FromSeconds(CheckIntervalSeconds);
            checkTimer.Tick += checkTimer_Tick;
            checkTimer.Start();

            // Проверить сразу же
            checkTimer_Tick(this, EventArgs.Empty);
        }

        // Показать иконку в трее
        private void SetupNotifyIcon()
        {
            trayIcon = new NotifyIcon();

            trayIcon.Click += trayIcon_Click;
            trayIcon.Text = "Оповещения Трекера";
            trayIcon.Icon = TrackerNotify.Properties.Resources.notify;
            trayIcon.Visible = true;
        }

        // Выполнен щелчок на значке в трее
        private void trayIcon_Click(object sender, EventArgs e)
        {
            if(!IsVisible)
            {
                this.Show();
                this.Activate();
            }
        }

        // Срабатывание таймера проверки
        private async void checkTimer_Tick(object sender, EventArgs e)
        {
            await viewModel.CheckForActions();
        }

        public ListWindow()
        {
            InitializeComponent();
        }
    }
}
