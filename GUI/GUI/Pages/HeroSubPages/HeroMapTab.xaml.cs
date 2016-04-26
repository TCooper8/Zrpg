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

        Thickness garrisonLocation = new Thickness(340, 120, 0, 0);
        Thickness northshireLocation = new Thickness(970, 15, 0, 0);
        Thickness goldshireLocation = new Thickness(690, 420, 0, 0);

        public HeroMapTab()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.hero = e.Parameter as Hero;
            UpdateHeroStatus();
        }
        
        //Get client garrison information (owned regions, owned zones)

        //Update map tab with current information (quests available in zones)

        //Get current hero selected information (current quest, quest status, quest reward, location)

        //Update hero location location and information
        private void UpdateHeroStatus()
        {
            textBlockTest.Text = hero.name;
        }

        private void garrisonButton_Click(object sender, RoutedEventArgs e)
        {
            heroIcon.Margin = garrisonLocation;
            //this.Frame.Navigate(typeof(GarrisonZone));          
        }

        private void northshireButton_Click(object sender, RoutedEventArgs e)
        {
            heroIcon.Margin = northshireLocation;
            //this.Frame.Navigate(typeof(NorthshireZone));
        }

        private void goldshireButton_Click(object sender, RoutedEventArgs e)
        {
            heroIcon.Margin = goldshireLocation;
            //this.Frame.Navigate(typeof(GoldshireZone));           
        }
    }
}
