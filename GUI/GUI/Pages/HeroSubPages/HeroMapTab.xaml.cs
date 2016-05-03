using GUI.Pages.HeroSubPages.ZoneTestPages;
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
    public sealed partial class HeroMapTab : Page
    {
        ClientState state = ClientState.state;
        Hero hero;

        public HeroMapTab()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.hero = e.Parameter as Hero;
            UpdateHeroStatus();
            zoneInfoTextBlock.Text = "Garrison Info";
            informationTextBlock.Text = "Gold Income: 580 per day\n" +
                                        "Soldiers: 250\n" +
                                        "Trade Influence: +10%\n" +
                                        "Holy Power: 5\n";
        }
        
        //Get client garrison information (owned regions, owned zones)

        //Update map tab with current information (quests available in zones)

        //Get current hero selected information (current quest, quest status, quest reward, location)

        //Update hero location location and information
        private void UpdateHeroStatus()
        {
            string heroLocation = hero.name + "'s Current Location: ";

            if (zoneInfoTextBlock.Text == "Goldshire Info")
            {
                heroLocationText.Text = heroLocation + "\nGoldshire";
            }

            else if (zoneInfoTextBlock.Text == "Northshire Info")
            {
                heroLocationText.Text = heroLocation + "\nNorthshire";
            }

            else
            {
                heroLocationText.Text = heroLocation + "\nGarrison";
            }
        }

        private void garrisonButton_Click(object sender, RoutedEventArgs e)
        {
            zoneInfoTextBlock.Text = "Garrison Info";
            informationTextBlock.Text = "Gold Income: 580 per day\n" +
                                        "Soldiers: 250\n" +
                                        "Trade Influence: +10%\n" +
                                        "Holy Power: 5\n";    
        }

        private void northshireButton_Click(object sender, RoutedEventArgs e)
        {
            zoneInfoTextBlock.Text = "Northshire Info";
            informationTextBlock.Text = "Gold Income: 60 per day\n" +
                                        "Soldiers: 30\n" +
                                        "Trade Influence: +5%\n" +
                                        "Holy Power: 10\n";
        }

        private void goldshireButton_Click(object sender, RoutedEventArgs e)
        {
            zoneInfoTextBlock.Text = "Goldshire Info";
            informationTextBlock.Text = "Gold Income: 80 per day\n" +
                                       "Soldiers: 50\n" +
                                       "Trade Influence: +3%\n" +
                                       "Holy Power: 2\n";
        }

        private void sendHeroButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateHeroStatus();
        }
    }
}
