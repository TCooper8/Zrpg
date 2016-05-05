using GUI.Pages.HeroSubPages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Hero hero;
        
        public HeroesPage()
        {
            this.InitializeComponent();

            GetUserGarrison();
            LoadHeroes();
            GetHeroStats();
        }

        private async void LoadHeroes()
        {
            var reply = await state.GetHeroes();
            
            if (reply.IsSuccess)
            {
                Debug.WriteLine("Generating hero controls");
                var success = (GetHeroArrayReply.Success)reply;
                var heroes = success.Item;
                listView.ItemsSource = heroes;

                var heroControls = new List<ItemsControl>();
                var xpControls = new List<ProgressBar>();

                foreach (var hero in heroes)
                {
                    var xpProgress = new ProgressBar();
                    xpProgress.Maximum = hero.stats.finalXp;
                    xpProgress.Value = hero.stats.xp;
                    xpProgress.Width = 250;
                    xpProgress.Height = 40;

                    xpControls.Add(xpProgress);
                    //heroControl.Items.Add(heroDisplay);
                    //heroControl.Items.Add(xpProgress);
                }

                if (listView.Items.Count == 0)
                {
                }
                else
                {
                    listView.SelectedIndex = 0;
                }

                heroXpList.ItemsSource = xpControls;
                //listView.ItemsSource = heroControls;
            }
        }

        private async void GetUserGarrison()
        {
            await state.GetGarrison();
        }

        private async void GetHeroStats()
        {
            var reply = await state.GetHeroes();

            if (reply.IsSuccess)
            {
                var success = (GetHeroArrayReply.Success)reply;

                if(listView.SelectedIndex == -1)
                {
                }
                else
                {
                    hero = success.Item[listView.SelectedIndex];
                    infoFrame.Content = String.Format(
                        "Name: {0}\n" +
                        "Faction: {1}\n" +
                        "Gender: {2}\n" +
                        "Race: {3}\n" +
                        "Class: {4}\n" +
                        "Level: {5}\n" +
                        "Strength: {6}\n" +
                        "Stamina: {7}\n" +
                        "Travel Speed: {8}",
                        hero.name,
                        hero.faction.ToString(),
                        hero.gender.ToString(),
                        hero.race.ToString(),
                        hero.heroClass.ToString(),
                        hero.level.ToString(),
                        hero.stats.strength.ToString(),
                        hero.stats.stamina.ToString(),
                        hero.stats.groundTravelSpeed.ToString()
                   );
                }
            }        
        }

        private void statsButton_Click(object sender, RoutedEventArgs e)
        {
            statsButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            mapButton.IsEnabled = true;

            GetHeroStats();
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

            infoFrame.Navigate(typeof(HeroInventoryTab), hero);
        }

        private async void mapButton_Click(object sender, RoutedEventArgs e)
        {
            mapButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            statsButton.IsEnabled = true;

            var reply = await state.GetHeroes();

            if (reply.IsSuccess)
            {
                var success = (GetHeroArrayReply.Success)reply;

                if (listView.SelectedIndex == -1)
                {
                }
                else
                {
                    hero = success.Item[listView.SelectedIndex];
                    infoFrame.Navigate(typeof(HeroMapTab), hero);
                }
            }      
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

        private void settingsPageButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to settings page
            this.Frame.Navigate(typeof(SettingsPage));
        }
    }
}
