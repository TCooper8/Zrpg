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
    public sealed partial class ChooseFactionPage : Page
    {
        public ChooseFactionPage()
        {
            this.InitializeComponent();

            //Dummy data until decisions are made
            allianceInfoFrame.Content = "Alliance info 1\n" + 
                                        "Alliance info 2\n" +
                                        "Alliance info 3\n";

            hordeInfoFrame.Content = "Horde info 1\n" +
                                     "Horde info 2\n" +
                                     "Horde info 3";
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to login page
            this.Frame.Navigate(typeof(MainPage));
        }

        private void choseAllianceButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to alliance garrison creation page
            this.Frame.Navigate(typeof(AllianceGarrisonCreationPage));
        }

        private void choseHordeButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to horde garrison creation page
            this.Frame.Navigate(typeof(HordeGarrisonCreationPage));
        }
    }
}
