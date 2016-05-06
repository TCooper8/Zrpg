using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Zrpg.Game;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GarrisonPage : Page
    {
        ClientState state = ClientState.state;

        public GarrisonPage()
        {
            this.InitializeComponent();
            GetGarrisonInfo();
        }

        private void GetGarrisonInfo()
        {
            var garrison = state.Garrison;
            infoFrame.Content = string.Format(garrison.name + "\n" +
                                            "Faction: {0}\n" +
                                            "Race: {1}\n" +
                                            "Gold Income: {2}",
                                            garrison.faction.ToString(), garrison.race.ToString(), garrison.stats.goldIncome.ToString());
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var records = await state.GetNotifications();
            records = records.OrderBy(record => record.timestamp).ToArray();

            Debug.WriteLine("{0} notifications loaded", records.Length, null);
            foreach (var record in records)
            {
                var notify = record.item;
                var msg = String.Format(
                    "Notification: {0}\n\t{1}\n\nTime:{2}",
                    notify.Item.messageTitle,
                    notify.Item.messageBody,
                    record.timestamp
                );
                var dialog = new MessageDialog(msg);
                await dialog.ShowAsync();
                await state.RemNotification(record.id);
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Log user out           
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Heroes_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Navigate to hero page
            this.Frame.Navigate(typeof(HeroesPage));
        }

        private void settingsPageButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to settings page
            this.Frame.Navigate(typeof(SettingsPage));
        }
    }
}
