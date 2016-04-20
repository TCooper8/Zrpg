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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HeroesPage : Page
    {
        public HeroesPage()
        {
            this.InitializeComponent();
            string hero = GetHeroSelected(listView.SelectedIndex);
            infoFrame.Content = hero + "'s Stats Info Here";
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate back to garrison page
            this.Frame.Navigate(typeof(GarrisonPage));
        }

        //All data is static and manually entered until the heros are generated from the server
        //This is just used as a preview for the data and code will be adjusted later
        //However, logic and behavior will stay the same
        private string GetHeroSelected(int index)
        {
            string hero;
            if(index == 0)
            {
                hero = hero1.Content.ToString();
            }
            else if(index == 1)
            {
                hero = hero2.Content.ToString();
            }
            else if(index == 2)
            {
                hero = hero3.Content.ToString();
            }
            else
            {
                hero = "No Hero Selected";
            }
            return hero;
        }

        private void statsButton_Click(object sender, RoutedEventArgs e)
        {
            statsButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            mapButton.IsEnabled = true;

            string hero = GetHeroSelected(listView.SelectedIndex);          
            infoFrame.Content = hero + "'s Stats Info Here";
        }

        private void gearButton_Click(object sender, RoutedEventArgs e)
        {
            gearButton.IsEnabled = false;

            statsButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            mapButton.IsEnabled = true;

            string hero = GetHeroSelected(listView.SelectedIndex);
            infoFrame.Content = hero + "'s Gear Info Here";
        }

        private void inventoryButton_Click(object sender, RoutedEventArgs e)
        {
            inventoryButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            statsButton.IsEnabled = true;
            mapButton.IsEnabled = true;

            string hero = GetHeroSelected(listView.SelectedIndex);
            infoFrame.Content = hero + "'s Inventory Info Here";
        }

        private void mapButton_Click(object sender, RoutedEventArgs e)
        {
            mapButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            statsButton.IsEnabled = true;

            string hero = GetHeroSelected(listView.SelectedIndex);
            infoFrame.Content = hero + "'s Map Info Here";
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
    }
}
