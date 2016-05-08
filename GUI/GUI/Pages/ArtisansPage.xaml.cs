using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class ArtisansPage : Page
    {
        ClientState state = ClientState.state;

        public ArtisansPage()
        {
            this.InitializeComponent();
            LoadArtisans();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(GarrisonPage));
        }

        private void settingsPageButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SettingsPage));
        }

        private void createHeroButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(CreateArtisanPage));
        }

        private async void LoadArtisans()
        {
            var reply = await state.GetClientArtisans();

            foreach (var artisans in reply)
            {
                listView.Items.Add(artisans.name);

                foreach (var recipe in artisans.recipes)
                {
                    Debug.WriteLine("Recipe {0} [{1}]", recipe.id, String.Join("; ", recipe.tags));
                }
            }
        }
    }
}
