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
    public sealed partial class HordeGarrisonCreationPage : Page
    {
        public HordeGarrisonCreationPage()
        {
            this.InitializeComponent();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to choose faction page
            this.Frame.Navigate(typeof(ChooseFactionPage));
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to garrison page
            this.Frame.Navigate(typeof(GarrisonPage));
        }

        private void Orc_Tapped(object sender, TappedRoutedEventArgs e)
        {
            infoFrame.Content = "Orc info detail 1\n\n" +
                                "Orc info detail 2\n\n" +
                                "Orc info detail 3\n\n" +
                                "Orc info detail 4";
        }

        private void Undead_Tapped(object sender, TappedRoutedEventArgs e)
        {
            infoFrame.Content = "Undead info detail 1\n\n" +
                                "Undead info detail 2\n\n" +
                                "Undead info detail 3\n\n" +
                                "Undead info detail 4";
        }

        private void Troll_Tapped(object sender, TappedRoutedEventArgs e)
        {
            infoFrame.Content = "Troll info detail 1\n\n" +
                                "Troll info detail 2\n\n" +
                                "Troll info detail 3\n\n" +
                                "Troll info detail 4";
        }

        private void Goblin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            infoFrame.Content = "Goblin info detail 1\n\n" +
                                "Goblin info detail 2\n\n" +
                                "Goblin info detail 3\n\n" +
                                "Goblin info detail 4";
        }
    }
}
