using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using TrackerNotify.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrackerNotify
{
    /// <summary>
    /// Сеанс соединения с Трекером
    /// </summary>
    public class TrackerSession
    {
        private const int RequestTimeout = 3000;

        // Cookie, содержащий идентификатор сеанса
        private string sessionCookie;

        /// <summary>
        /// Получить список недавних действий с Трекера
        /// </summary>
        /// <returns>Недавние действия, выполняемые пользователями</returns>
        public async Task<IList<RecentAction>> RetrieveActions()
        {
            await RenewAuthCookieIfExpired();

            var actionsResp = await GetActionResponse("last_actions");

            JArray actionsJson = null;
            using(var respReader = new StreamReader(actionsResp.GetResponseStream()))
            {
                string json = await respReader.ReadToEndAsync();
                actionsJson = (JArray)JsonConvert.DeserializeObject(json);
            }

            var recentActions = actionsJson.Select(j => RecentAction.FromJson(j));

            // Загрузить аватарки
            var logins = recentActions.Select(a => a.UserId).Distinct().ToList();
            logins.Add("default");
            await DownloadAllMissingUserPics(logins.ToArray());

            return recentActions.ToList();
        }

        // Загрузить все иконки пользователей
        private async Task DownloadAllMissingUserPics(string[] userLogins)
        {
            const string ImgType = "image/jpeg";

            var picsDictionary = SettingsStore.Instance.UserPicsData;

            foreach(string login in userLogins)
            {
                if(!picsDictionary.ContainsKey(login))
                {
                    string loadAction = String.Concat("tiles/", login, ".jpg");

                    WebResponse picResp = null;
                    try
                    {
                        picResp = await GetActionResponse(loadAction);
                    }
                    catch(WebException)
                    {
                        continue;
                    }

                    if(picResp.ContentType == ImgType)
                    {
                        using(var imgStream = picResp.GetResponseStream())
                        {
                            byte[] buffer = new byte[picResp.ContentLength];
                            await imgStream.ReadAsync(buffer, 0, buffer.Length);
                            picsDictionary.Add(login, buffer);
                        }
                    }
                }
            }
        }

        // Обновить cookie для сессии, если просрочена
        private async Task RenewAuthCookieIfExpired()
        {
            const int ExpirePartIdx = 2;

            bool needRenew = String.IsNullOrEmpty(sessionCookie);
            
            if(!needRenew)
            {
                string expirePart = sessionCookie.Split(';')[ExpirePartIdx];
                var expireTs = DateTime.Parse(expirePart.Split('=')[1]);
                needRenew = DateTime.Now >= expireTs;
            }

            if(needRenew)
            {
                await Authorize();
            }
        }

        // Выполнить авторизацию
        private async Task Authorize()
        {
            this.sessionCookie = null;

            string authToken = SettingsStore.Instance.EnterToken;
            string action = String.Concat("enter/", authToken);

            var resp = await GetActionResponse(action);
            this.sessionCookie = resp.Headers["Set-Cookie"];
            resp.Dispose();
        }

        // Сделать запрос и выдать данные ответа
        private async Task<WebResponse> GetActionResponse(string action)
        {
            string baseUrl = SettingsStore.Instance.HostURL;

            string fullUrl = String.Concat(baseUrl, "/", action);
            var req = (HttpWebRequest)HttpWebRequest.Create(fullUrl);
            req.AllowAutoRedirect = false;
            req.Method = "GET";
            req.Timeout = RequestTimeout;

            if(sessionCookie != null)
            {
                req.Headers.Add("Cookie", sessionCookie);
            }
            
            return await req.GetResponseAsync();
        }
    }
}
