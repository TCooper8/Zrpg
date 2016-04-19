using GUI.Pages;
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

namespace GUI.MapZones
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Northshire : Page
    {
        public Northshire()
        {
            this.InitializeComponent();
        }

        private void northshireBackButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to World Map Page
            this.Frame.Navigate(typeof(WorldMapPage));
        }

        //Dummy data do show preview of secondary list view item for details
        private void vendorsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var i = new List<string>();

            i.Add("Gold Income: 100");
            i.Add("Soldiers: 10");
            i.Add("Trade Influence: -5%");
            i.Add("Holy Power: 2%");

            listViewDetailView.ItemsSource = i;
        }

        private void questsItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var i = new List<string>();

            i.Add("Quest 1: User Interface");
            i.Add("Quest 2: Connect to Server");
            i.Add("Quest 3: Update Data for UI");

            listViewDetailView.ItemsSource = i;
        }

        private void infoItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var i = new List<string>();
            i.Add("In construction");

            listViewDetailView.ItemsSource = i;
        }
    }
}
