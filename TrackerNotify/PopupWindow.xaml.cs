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
using System.ComponentModel;
using TrackerNotify.Model;

namespace TrackerNotify
{
    /// <summary>
    /// Всплывающее сообщение
    /// </summary>
    public partial class PopupWindow : Window
    {
        // Модель представления данных окна
        private ActionsViewModel viewModel;

        // Часть общей функциональности окна
        private WindowOverTrayHelper uiHelper;

        // Получено новое недавнее действие
        private void viewModel_NewActionRetreived(object sender, EventArgs e)
        {
            this.Show();
            this.Activate();
        }

        public PopupWindow(ActionsViewModel vm)
        {
            InitializeComponent();

            this.viewModel = vm;
            this.uiHelper = new WindowOverTrayHelper(this, viewModel);
            this.DataContext = viewModel;
            this.viewModel.NewActionRetreived += viewModel_NewActionRetreived;
        }
    }
}
