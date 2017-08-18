using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Globalization;

namespace TrackerNotify
{
    /// <summary>
    /// Выдает изображение пользователя по его логину
    /// </summary>
    public class UserIdToPictureConverter : IValueConverter
    {
        /// <summary>
        /// Конвертировать логин пользователя в его аватар
        /// </summary>
        public object Convert(object value, Type type, object p, CultureInfo c)
        {
            var userPics = SettingsStore.Instance.UserPicsData;

            string login = (string)value;
            string safeLogin = userPics.ContainsKey(login) ? login : "default";

            var userPic = new BitmapImage();
            userPic.BeginInit();
            userPic.StreamSource = new MemoryStream(userPics[safeLogin]);
            userPic.EndInit();

            return userPic;
        }

        /// <summary>
        /// Конвертировать обратно
        /// </summary>
        public object ConvertBack(object value, Type type, object p, CultureInfo c)
        {
            throw new NotImplementedException();
        }
    }
}
