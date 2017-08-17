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
        /// Создать объект их данных JSON
        /// </summary>
        /// <param name="json">Представление объекта в JSON</param>
        /// <returns>Объект недавнего действия</returns>
        public static RecentAction FromJson(JToken json)
        {
            if (json is JObject)
            {
                Func<JObject, string, object> jsonValue = (obj, key) => {
                    return (obj[key] as JValue).Value;
                };

                var dataObj = json as JObject;

                return new RecentAction
                {
                    UserId = (string)jsonValue(dataObj, "user_id"),
                    ItemTitle = (string)jsonValue(dataObj, "item_title"),
                    Description = (string)jsonValue(dataObj, "description"),
                    Timestamp = (DateTime)jsonValue(dataObj, "ts")
                };
            }
            else
            {
                throw new ApplicationException("Неверный тип данных!");
            }
        }
    }
}
