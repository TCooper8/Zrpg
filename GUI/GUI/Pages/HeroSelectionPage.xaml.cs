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
    /// 
    
    public sealed partial class HeroSelectionPage : Page
    {
        public HeroSelectionPage()
        {
            this.InitializeComponent();
            //GameClient.IGameClient game;
            
            //game.AddGarrison("","",Race.Human,Faction.Horde);
        }
        
        private void heroSelectBackButton_Click(object sender, RoutedEventArgs e)
        {
            //TO DO: Log current user out

            //Return to login page
            this.Frame.Navigate(typeof(MainPage));
        }

        private void heroCreateButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to hero creation page
            this.Frame.Navigate(typeof(HeroCreationPage));
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Gets current bounds when bounds changed
            Rect bounds = Window.Current.Bounds;
            
            //Position items accordingly to width bounds
            if(bounds.Width <= 1000)
            {
                textBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                textBox.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }


            //Position items accordingly to height
            if(bounds.Height <= 800)
            {
                selectHeroButton.SetValue(Grid.RowProperty, 0);
                selectHeroButton.SetValue(MarginProperty, new Thickness(0, 0, 0, 0));
            }
            else
            {
                selectHeroButton.SetValue(Grid.RowProperty, 1);
                selectHeroButton.SetValue(MarginProperty, new Thickness(0, 600, 0, 20));
            }

        }

        private void selectHeroButton_Click(object sender, RoutedEventArgs e)
        {
            //TO DO: Log hero into world and load data

            //Navigate to world map page
            this.Frame.Navigate(typeof(HeroScreenPage));
        }
    }
}
