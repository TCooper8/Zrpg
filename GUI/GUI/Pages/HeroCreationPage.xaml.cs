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
    public sealed partial class HeroCreationPage : Page
    {
        ClientState state = ClientState.state;

        public HeroCreationPage()
        {
            this.InitializeComponent();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to hero selection page
            this.Frame.Navigate(typeof(HeroesPage));
        }

        private void maleToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            //Uncheck female toggle button
            femaleToggleButton.IsChecked = false;
        }

        private void femaleToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            //Uncheck male toggle button
            maleToggleButton.IsChecked = false;
        }

        private void allianceListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Unselect any horde list view items
            hordeListView.SelectedIndex = -1;           
        }

        private void hordeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Unselect any alliance list view items
            allianceListView.SelectedIndex = -1;
        }

        private async void doneButton_Click(object sender, RoutedEventArgs e)
        {
            //TO DO: Create specified hero
            var garrison = state.Garrison;
            var clientId = state.ClientId;

            await state.AddHero(clientId, "Hero1", Race.Human, Faction.Alliance, Gender.Male, HeroClass.Warrior);
            await state.AddHero(clientId, "Hero2", Race.Human, Faction.Horde, Gender.Female, HeroClass.Warrior);
            //Navigate back to hero selecion screen
            this.Frame.Navigate(typeof(HeroesPage));
        }  
    }
}
