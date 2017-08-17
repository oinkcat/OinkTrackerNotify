using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace TrackerNotify
{
    /// <summary>
    /// Диалог ввода адреса сайта Трекера
    /// </summary>
    public partial class LoginDataWindow : Window
    {
        private const string UrlRegEx = @"https?:\/\/[^\/]+\/enter\/[0-9a-f]+";

        private Regex tokenUrlRegex;

        public LoginDataWindow()
        {
            InitializeComponent();
            tokenUrlRegex = new Regex(UrlRegEx, RegexOptions.Compiled);
        }

        private async Task ParseAndSaveData(string enterUrl)
        {
            var siteUrl = new Uri(enterUrl);

            string url = String.Concat(siteUrl.Scheme, "://", siteUrl.Authority);
            SettingsStore.Instance.HostURL = url;
            string token = enterUrl.Substring(enterUrl.LastIndexOf('/') + 1);
            SettingsStore.Instance.EnterToken = token;

            await SettingsStore.Instance.Save();
        }

        private async void OK_Click(object sender, RoutedEventArgs e)
        {
            await ParseAndSaveData(SiteAddressBox.Text);

            this.DialogResult = true;
            this.Close();
        }

        private void SiteAddressBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OkBtn.IsEnabled = tokenUrlRegex.IsMatch(SiteAddressBox.Text);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SiteAddressBox.Focus();
        }
    }
}
