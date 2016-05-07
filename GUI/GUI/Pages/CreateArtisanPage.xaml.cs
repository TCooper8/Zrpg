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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GUI.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateArtisanPage : Page
    {
        public CreateArtisanPage()
        {
            this.InitializeComponent();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ArtisansPage));
        }

        private void settingsPageButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        private void blacksmithToggleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //If user clicked, untoggle any other toggle buttons
            tailorToggleButton.IsChecked = false;
            jewelerToggleButton.IsChecked = false;
            cookToggleButton.IsChecked = false;

            //Update text info for current toggled button
            artisanInfoTextBlock.Text = "Blacksmith information goes here";
        }

        private void tailorToggleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //If user clicked, untoggle any other toggle buttons
            blacksmithToggleButton.IsChecked = false;
            jewelerToggleButton.IsChecked = false;
            cookToggleButton.IsChecked = false;

            //Update text info for current toggled button
            artisanInfoTextBlock.Text = "Tailor information goes here";
        }

        private void jewelerToggleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //If user clicked, untoggle any other toggle buttons
            blacksmithToggleButton.IsChecked = false;
            tailorToggleButton.IsChecked = false;
            cookToggleButton.IsChecked = false;

            //Update text info for current toggled button
            artisanInfoTextBlock.Text = "Jeweler information goes here";
        }

        private void cookToggleButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            //If user clicked, untoggle any other toggle buttons
            blacksmithToggleButton.IsChecked = false;
            tailorToggleButton.IsChecked = false;
            jewelerToggleButton.IsChecked = false;

            //Update text info for current toggled button
            artisanInfoTextBlock.Text = "Cook information goes here";
        }

        private async void createButton_Click(object sender, RoutedEventArgs e)
        {
            //Behavior for loading visual. Using a delay for testing purposes
            relativePanel.Visibility = Visibility.Visible;
            progressRing.IsActive = true;
            await Task.Delay(2000);

            //Create artisan

            //Navigate back to artisan page
            this.Frame.Navigate(typeof(ArtisansPage));
        }
    }
}
