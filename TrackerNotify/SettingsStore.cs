using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace TrackerNotify
{
    /// <summary>
    /// Хранит настройки приложения
    /// </summary>
    public class SettingsStore
    {
        private const string DirName = "TrackerNotify";

        private const string FileName = "config.xml";

        /// <summary>
        /// Экземпляр класса настроек
        /// </summary>
        public static SettingsStore Instance { get; private set; }

        /// <summary>
        /// Запущено ли без предварительных настроек
        /// </summary>
        public bool IsCleanRun { get; private set; }

        /// <summary>
        /// Ссылка на сайт Трекера
        /// </summary>
        public string HostURL { get; set; }

        /// <summary>
        /// Токен для входа пользователя
        /// </summary>
        public string EnterToken { get; set; }

        /// <summary>
        /// Хэш последнего просмотренного элемента
        /// </summary>
        public string LastItemHash { get; set; }

        /// <summary>
        /// Данные иконок пользователей
        /// </summary>
        public Dictionary<string, byte[]> UserPicsData { get; private set; }

        /// <summary>
        /// Имеются ли сохраненные настройки
        /// </summary>
        public bool HasStoredSettings
        {
            get 
            {
                return !String.IsNullOrEmpty(HostURL) && 
                       !String.IsNullOrEmpty(EnterToken);
            }
        }

        /// <summary>
        /// Выдать ссылку на вход в Трекер
        /// </summary>
        public string TrackerEnterUrl
        {
            get
            {
                return String.Concat(HostURL, "/enter/", EnterToken);
            }
        }

        // Путь к файлу настроек
        private string SettingsFilePath
        {
            get
            {
                var spFolder = Environment.SpecialFolder.LocalApplicationData;
                string folder = Environment.GetFolderPath(spFolder);

                return Path.Combine(folder, DirName, FileName);
            }
        }

        /// <summary>
        /// Загрузить настройки
        /// </summary>
        public async Task Load()
        {
            if(File.Exists(SettingsFilePath))
            {
                XDocument settingsDoc;

                using(var file = new FileStream(SettingsFilePath, FileMode.Open))
                using(var memStream = new MemoryStream())
                {
                    await file.CopyToAsync(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);
                    settingsDoc = XDocument.Load(memStream);
                }

                var root = settingsDoc.Element("Settings");
                this.HostURL = root.Element(XName.Get("Url")).Value;
                this.EnterToken = root.Element(XName.Get("Token")).Value;
                this.LastItemHash = root.Element(XName.Get("LastHash")).Value;
            }
            else
            {
                IsCleanRun = true;
            }
        }

        /// <summary>
        /// Сохранить настройки
        /// </summary>
        public async Task Save()
        {
            if(!File.Exists(SettingsFilePath))
            {
                string dirPath = Path.GetDirectoryName(SettingsFilePath);
                Directory.CreateDirectory(dirPath);
            }

            var settingsDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"),
                new XElement(XName.Get("Settings"),
                    new XElement(XName.Get("Url"), this.HostURL),
                    new XElement(XName.Get("Token"), this.EnterToken),
                    new XElement(XName.Get("LastHash"), this.LastItemHash)));
            
            using(var memStream = new MemoryStream())
            using(var file = new FileStream(SettingsFilePath, FileMode.Create))
            {
                settingsDoc.Save(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                await memStream.CopyToAsync(file);
            }
        }

        // Синглтон
        private SettingsStore()
        {
            this.UserPicsData = new Dictionary<string, byte[]>();
        }

        static SettingsStore()
        {
            Instance = new SettingsStore();
        }
    }
}
