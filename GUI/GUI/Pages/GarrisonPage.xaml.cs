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
    public sealed partial class GarrisonPage : Page
    {
        public GarrisonPage()
        {
            this.InitializeComponent();

            //Dummy data
            infoFrame.Content = "Heroes: 5\n\n" +
                                "Controlled Regions: 2\n\n" +
                                "Vault Tabs: 3\n\n" +
                                "Vendors Owned: 4\n\n" +
                                "Artisans Owned: 2\n\n" +
                                "Gold Income: 500 per day\n\n" +
                                "Food Income: 200 per day\n\n" +
                                "Research Points: 7";                               
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Log user out
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Heroes_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //Navigate to hero page
            this.Frame.Navigate(typeof(HeroesPage));
        }
    }
}
