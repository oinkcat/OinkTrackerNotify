using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace TrackerNotify.Model
{
    /// <summary>
    /// Недавно выполненное действие
    /// </summary>
    public class RecentAction
    {
        /// <summary>
        /// Временная метка
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Название элемента, с котором произведено действие
        /// </summary>
        public string ItemTitle { get; set; }

        /// <summary>
        /// Описание действия
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Выдать отличительный хэш код для этого элемента
        /// </summary>
        /// <returns>Строка-хэш</returns>
        public string GetItemHash()
        {
            var md5Service = MD5.Create();
            string id = String.Concat(Timestamp.Ticks.ToString(), Description);
            byte[] idBytes = Encoding.Default.GetBytes(id);

            string hash = Convert.ToBase64String(md5Service.ComputeHash(idBytes));
            return hash;
        }

        /// <summary>
        /// Заполнить дополнительную информацию о действии
        /// </summary>
        /// <param name="actionProps">JSON - представление данных</param>
        public virtual void FillDetails(JObject actionProps)
        {
            ItemTitle = (string)GetJsonProp(actionProps, "item_title");
            Description = (string)GetJsonProp(actionProps, "description");

        }

        /// <summary>
        /// Создать объект из данных JSON
        /// </summary>
        /// <param name="json">Представление объекта в JSON</param>
        /// <returns>Объект недавнего действия</returns>
        public static RecentAction FromJson(JToken json)
        {
            const int ProgressTypeId = 3;

            if (json is JObject)
            {
                RecentAction action = null;

                var dataObj = json as JObject;
                long typeId = (long)GetJsonProp(dataObj, "type");

                if (typeId == ProgressTypeId)
                    action = new ProgressRecentAction();
                else
                    action = new RecentAction();

                action.UserId = (string)GetJsonProp(dataObj, "user_id");
                action.Timestamp = (DateTime)GetJsonProp(dataObj, "ts");
                action.FillDetails(dataObj);

                return action;
            }
            else
            {
                throw new ApplicationException("Неверный тип данных!");
            }
        }

        // Выдать значения свойства объекта JSON
        protected static object GetJsonProp(JObject obj, string propName)
        {
            return (obj[propName] as JValue).Value;
        }
    }

    /// <summary>
    /// Недавнее действие изменения прогресса элемента
    /// </summary>
    public class ProgressRecentAction : RecentAction
    {
        /// <summary>
        /// Заполнить данные действия изменения прогресса
        /// </summary>
        /// <param name="actionProps">JSON - представление данных</param>
        public override void FillDetails(JObject actionProps)
        {
            ItemTitle = (string)GetJsonProp(actionProps, "item_title");

            var additionalData = actionProps["data"] as JArray;
            int progress = additionalData[0].Value<int>();

            if (progress < 100)
                Description = String.Format("Прогресс выполнения: {0}%", progress);
            else
                Description = "Выполнено";
        }
    }
}
