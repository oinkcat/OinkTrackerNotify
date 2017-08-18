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
            const int BordersSize = 8;

            double width = SystemParameters.MaximizedPrimaryScreenWidth;
            double height = SystemParameters.MaximizedPrimaryScreenHeight;

            int k = window.WindowStyle == WindowStyle.None ? 2 : 1;

            window.Left = width - window.Width - BordersSize * k;
            window.Top = height - window.Height - BordersSize * k;
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
            this.window.IsVisibleChanged += window_IsVisibleChanged;
        }
    }
}
