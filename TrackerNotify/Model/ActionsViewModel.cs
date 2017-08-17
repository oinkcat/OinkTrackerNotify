using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerNotify.Model
{
    /// <summary>
    /// Модель представления страниц последних действий
    /// </summary>
    public class ActionsViewModel : INotifyPropertyChanged
    {
        // Сеанс связи с Трекером
        private TrackerSession session;

        // Выполняется ли загрузка данных
        private bool loading;

        /// <summary>
        /// Свойство модели было изменено
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Оповещение о новом недавнем действии
        /// </summary>
        public event EventHandler NewActionRetreived;

        /// <summary>
        /// Выполняется ли загрузка данных
        /// </summary>
        public bool LoadingStateShown
        {
            get
            {
                return loading && Actions.Count == 0;
            }
        }

        /// <summary>
        /// Произошла ли ошибка связи с Трекером
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Список недавних действий
        /// </summary>
        public ObservableCollection<RecentAction> Actions { get; set; }

        /// <summary>
        /// Самое раннее выполненное действие
        /// </summary>
        public RecentAction LastAction
        {
            get
            {
                return Actions.FirstOrDefault();
            }
        }

        /// <summary>
        /// Выполняется ли завершение работы программы
        /// </summary>
        public bool IsAppShutdown { get; set; }

        /// <summary>
        /// Проверить наличие новых недавних действий
        /// </summary>
        public async Task CheckForActions()
        {
            loading = true;
            NotifyChange("LoadingStateShown");

            try
            {
                var currentActions = await session.RetrieveActions();
                if(currentActions.Count > 0)
                {
                    await ProcessActions(currentActions);
                }

                IsError = false;
                NotifyChange("IsError");
            }
            catch
            {
                IsError = true;
                NotifyChange("IsError");
            }

            loading = false;
            NotifyChange("LoadingStateShown");
        }

        // Обработать полученные недавние действия
        private async Task ProcessActions(IList<RecentAction> newActions)
        {
            string prevHash = SettingsStore.Instance.LastItemHash;
            string newHash = newActions.First().GetItemHash();
            
            bool gotNewAction = !newHash.Equals(prevHash);

            if (gotNewAction || Actions.Count == 0)
            {
                Actions.Clear();
                foreach (var action in newActions)
                {
                    Actions.Add(action);
                }

                SettingsStore.Instance.LastItemHash = newHash;
                await SettingsStore.Instance.Save();
            }

            if(gotNewAction && NewActionRetreived != null)
            {
                NewActionRetreived(this, EventArgs.Empty);
                NotifyChange("LastAction");
            }
        }

        // Данные модели изменены
        private void NotifyChange(string propName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        public ActionsViewModel()
        {
            this.Actions = new ObservableCollection<RecentAction>();
            this.session = new TrackerSession();
            this.IsError = false;
        }
    }
}
