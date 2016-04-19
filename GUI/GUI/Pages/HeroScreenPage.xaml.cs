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
    public sealed partial class HeroScreenPage : Page
    {
        public HeroScreenPage()
        {
            this.InitializeComponent();
        }

        private void heroScreenBackButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to hero selection page
            this.Frame.Navigate(typeof(HeroSelectionPage));
        }

        private void mapButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to world map page
            
        }

        //Dummy data to see layout being used
        private void mapPageItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            
        }

        private void logsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            textBlock1.Text = "Logs";
        }

        private void gearItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            textBlock1.Text = "Gear";
        }

        private void statsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            textBlock1.Text = "Stats";
        }

        private void questsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            textBlock1.Text = "Quests";
        }
    }
}
