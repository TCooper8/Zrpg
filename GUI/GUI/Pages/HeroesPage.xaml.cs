using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class HeroesPage : Page
    {
        ClientState state = ClientState.state;
        
        public HeroesPage()
        {
            this.InitializeComponent();
            var garrison = state.Garrison;                   
           
            LoadHeroes();
            GetHeroStats();
        }

        private async void LoadHeroes()
        {
            var reply = await state.GetHeroes();
            var garrison = state.Garrison;

            if (reply.IsSuccess)
            {
                var success = (GetHeroArrayReply.Success)reply;
                listView.ItemsSource = success.Item;            
            }
            listView.SelectedIndex = 0;
        }
        
        private async void GetHeroStats()
        {
            var reply = await state.GetHeroes();
            var garrison = state.Garrison;
            Hero hero; 

            if (reply.IsSuccess)
            {
                var success = (GetHeroArrayReply.Success)reply;
                hero = success.Item[listView.SelectedIndex];
                infoFrame.Content = hero.faction;
            }
           
        }

        private void statsButton_Click(object sender, RoutedEventArgs e)
        {
            statsButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            mapButton.IsEnabled = true;
        }

        private void gearButton_Click(object sender, RoutedEventArgs e)
        {
            gearButton.IsEnabled = false;

            statsButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            mapButton.IsEnabled = true;
        }

        private void inventoryButton_Click(object sender, RoutedEventArgs e)
        {
            inventoryButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            statsButton.IsEnabled = true;
            mapButton.IsEnabled = true;
        }

        private void mapButton_Click(object sender, RoutedEventArgs e)
        {
            mapButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            statsButton.IsEnabled = true;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Logic for the behavior of buttons
            try
            {
                if (statsButton.IsEnabled == false)
                {
                    statsButton_Click(sender, e);
                }
                else if (gearButton.IsEnabled == false)
                {
                    gearButton_Click(sender, e);
                }
                else if (inventoryButton.IsEnabled == false)
                {
                    inventoryButton_Click(sender, e);
                }
                else if (mapButton.IsEnabled == false)
                {
                    mapButton_Click(sender, e);
                }
                else
                {
                    statsButton_Click(sender, e);
                }
            }
            catch
            {
            }
        }

        private void createHeroButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HeroCreationPage));
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate back to garrison page
            this.Frame.Navigate(typeof(GarrisonPage));
        }
    }
}
