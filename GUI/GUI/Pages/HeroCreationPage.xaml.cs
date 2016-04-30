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

            //Behavior for the horde/alliance list views
            if(state.Garrison.faction == Faction.Alliance)
            {
                hordeListView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                allianceListView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                hordeListView.Margin = new Thickness(0, 135, 0, 0);
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to hero selection page
            this.Frame.Navigate(typeof(HeroesPage));
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
            var clientId = state.ClientId;
            var garrison = state.Garrison;
            Gender gender;

            //Get user selected gender
            if (maleToggleButton.IsChecked == true)
            {
                gender = Gender.Male;
            }
            else
            {
                gender = Gender.Female;
            }

            //Get user selected name
            if(heroNameTextBox.Text == "")
            {
            }
            else
            {
                //Behavior for loading visual. Using a delay for testing purposes
                relativePanel.Visibility = Visibility.Visible;
                progressRing.IsActive = true;
                await Task.Delay(2000);

                var create = await state.AddHero(clientId, heroNameTextBox.Text, Race.Human, garrison.faction, gender, HeroClass.Warrior);

                if(create.IsSuccess)
                {
                    //Navigate back to hero selecion screen
                    this.Frame.Navigate(typeof(HeroesPage));
                }
            }
        }

        //Behavior for gender buttons
        private void maleToggleButton_Click(object sender, RoutedEventArgs e)
        {
            maleToggleButton.IsEnabled = false;
            femaleToggleButton.IsEnabled = true;

            femaleToggleButton.IsChecked = false;        
        }

        private void femaleToggleButton_Click(object sender, RoutedEventArgs e)
        {
            femaleToggleButton.IsEnabled = false;
            maleToggleButton.IsEnabled = true;

            maleToggleButton.IsChecked = false;
        }
    }
}
