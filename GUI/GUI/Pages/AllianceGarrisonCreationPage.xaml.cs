using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public sealed partial class AllianceGarrisonCreationPage : Page
    {
        static ClientState client = ClientState.state;

        public AllianceGarrisonCreationPage()
        {
            this.InitializeComponent();
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to choose faction page
            this.Frame.Navigate(typeof(ChooseFactionPage));
        }

        private async void createButton_Click(object sender, RoutedEventArgs e)
        {
            //Behavior for loading visual. Using a delay for testing purposes
            relativePanel.Visibility = Visibility.Visible;
            progressRing.IsActive = true;
            await Task.Delay(2000);

            //Create alliance garrison
            try
            {
                var reply = await client.CreateGarrison(Race.Human, Faction.Alliance);

                if (reply.IsClientHasGarrison)
                {                    
                    var getReply = await client.GetGarrison();
                    if (getReply.IsEmpty)
                    {
                        throw new Exception("Cannot create garrison and client has no garrison to get");
                    }                  
                    
                    this.Frame.Navigate(typeof(GarrisonPage));
                    return;
                }
                else if (!reply.IsSuccess)
                {
                    // Sanity check on the messages.
                    throw new Exception("Unable to create garrison");
                }

                // Retrieve the client's garrison.
                await client.GetGarrison();
                this.Frame.Navigate(typeof(GarrisonPage));
            }

            catch
            {
            }

            //Just navigate to garrison page with default data
            this.Frame.Navigate(typeof(GarrisonPage));
        }

        private void Human_Tapped(object sender, TappedRoutedEventArgs e)
        {
            infoFrame.Content = "Human info detail 1\n\n" +
                                "Human info detail 2\n\n" +
                                "Human info detail 3\n\n" +
                                "Human info detail 4";
        }

        private void Dwarf_Tapped(object sender, TappedRoutedEventArgs e)
        {
            infoFrame.Content = "Dwarf info detail 1\n\n" +
                                "Dwarf info detail 2\n\n" +
                                "Dwarf info detail 3\n\n" +
                                "Dwarf info detail 4";
        }

        private void NightElf_Tapped(object sender, TappedRoutedEventArgs e)
        {
            infoFrame.Content = "Night Elf info detail 1\n\n" +
                                "Night Elf info detail 2\n\n" +
                                "Night Elf info detail 3\n\n" +
                                "Night Elf info detail 4";
        }

        private void Gnome_Tapped(object sender, TappedRoutedEventArgs e)
        {
            infoFrame.Content = "Gnome info detail 1\n\n" +
                                "Gnome info detail 2\n\n" +
                                "Gnome info detail 3\n\n" +
                                "Gnome info detail 4";
        }
    }
}
