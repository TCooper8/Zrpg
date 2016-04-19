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
    public sealed partial class WorldMapPage : Page
    {
        public WorldMapPage()
        {
            this.InitializeComponent();
        }

        private void worldMapPageBackButton_Click(object sender, RoutedEventArgs e)
        {
            //TO DO: Save current hero data

            //Navigate to hero selection page
            this.Frame.Navigate(typeof(HeroScreenPage));
        }

        private void northshireZone_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to northshire map
            this.Frame.Navigate(typeof(MapZones.Northshire));
        }
    }
}
