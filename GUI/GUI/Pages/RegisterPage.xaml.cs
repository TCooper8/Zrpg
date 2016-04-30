using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            this.InitializeComponent();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to user login page
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void createButton_Click(object sender, RoutedEventArgs e)
        {
            //TO DO: Create user account
            relativePanel.Visibility = Visibility.Visible;
            progressRing.IsActive = true;
            await Task.Delay(2000);

            //Navigate to login page
            this.Frame.Navigate(typeof(MainPage));
        }

        private void settingsPageButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to settings page
            this.Frame.Navigate(typeof(SettingsPage));
        }
    }
}
