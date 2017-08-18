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
using System.ComponentModel;
using System.Media;
using TrackerNotify.Model;

using AppResources = TrackerNotify.Properties.Resources;

namespace TrackerNotify
{
    /// <summary>
    /// Всплывающее сообщение
    /// </summary>
    public partial class PopupWindow : Window
    {
        // Сколько секунд отображается окно
        private const int ShowTimeoutSeconds = 5;

        // Модель представления данных окна
        private ActionsViewModel viewModel;

        // Часть общей функциональности окна
        private WindowOverTrayHelper uiHelper;

        // Таймер для закрытия окна
        private DispatcherTimer hideTimer;

        // Звук для воспроизведения
        private SoundPlayer chimePlayer;

        // Получено новое недавнее действие
        private void viewModel_NewActionRetreived(object sender, EventArgs e)
        {
            hideTimer.Start();
            this.Show();

            chimePlayer.Play();
        }

        // Окно отображается дольше заданного интервала
        private void hideTimer_Tick(object sender, EventArgs e)
        {
            hideTimer.Stop();
            this.Hide();
        }

        // В окне выполнен щелчок
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            hideTimer.Stop();
            hideTimer.Start();
        }

        public PopupWindow(ActionsViewModel vm)
        {
            InitializeComponent();

            this.viewModel = vm;
            this.uiHelper = new WindowOverTrayHelper(this, viewModel);
            this.DataContext = viewModel;
            this.viewModel.NewActionRetreived += viewModel_NewActionRetreived;

            this.hideTimer = new DispatcherTimer(DispatcherPriority.ContextIdle);
            hideTimer.Interval = TimeSpan.FromSeconds(ShowTimeoutSeconds);
            hideTimer.Tick += hideTimer_Tick;

            this.chimePlayer = new SoundPlayer(AppResources.chime);
        }
    }
}
