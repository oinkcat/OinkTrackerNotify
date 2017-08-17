using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using TrackerNotify.Model;

namespace TrackerNotify
{
    /// <summary>
    /// Реализует часть общей функциональнсти окна над треем
    /// </summary>
    public class WindowOverTrayHelper
    {
        // Окно приложения
        private Window window;

        // Модель представления
        private ActionsViewModel viewModel;

        /// <summary>
        /// Разместить окно на экране (в правом углу)
        /// </summary>
        public void PlaceWindow()
        {
            double width = SystemParameters.MaximizedPrimaryScreenWidth;
            double height = SystemParameters.MaximizedPrimaryScreenHeight;

            window.Left = width - window.ActualWidth;
            window.Top = height - window.ActualHeight;
        }

        // Окно потеряло фокус
        private void window_Deactivated(object sender, EventArgs e)
        {
            window.Hide();
        }

        // Окно закрывается
        private void window_Closing(object sender, CancelEventArgs e)
        {
            window.Hide();
            e.Cancel = !viewModel.IsAppShutdown;
        }

        // Изменена видимость окна
        private void window_IsVisibleChanged(object sender,
                                             DependencyPropertyChangedEventArgs e)
        {
            if((bool)e.NewValue == true)
            {
                PlaceWindow();
            }
        }

        public WindowOverTrayHelper(Window window, ActionsViewModel vm)
        {
            this.window = window;
            this.viewModel = vm;

            // Общие события
            this.window.Closing += window_Closing;
            this.window.Deactivated += window_Deactivated;
            this.window.IsVisibleChanged += window_IsVisibleChanged;
        }
    }
}
