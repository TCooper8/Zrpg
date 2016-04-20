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
    public sealed partial class HordeGarrisonCreationPage : Page
    {
        IGameClient client;

        public HordeGarrisonCreationPage()
        {
            this.InitializeComponent();
            client = GameClient.RESTClient("http://localhost:8080");
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to choose faction page
            this.Frame.Navigate(typeof(ChooseFactionPage));
        }

        private async void createButton_Click(object sender, RoutedEventArgs e)
        {
            infoFrame.Content = "Creating Garrison!...";
            
            //Create horde garrison
            try
            {
                var reply = await client.AddGarrison("test client", "My garrison", Race.Human, Faction.Horde);

                //Create message on the server
                if (reply.IsEmptyReply)
                {
                    throw new Exception("Got empty reply from server");
                }
                else if (reply.IsExnReply)
                {
                    var exnReply = (Reply.ExnReply)reply;
                    var exn = exnReply.Item;
                    Debug.WriteLine("Error: {0}", exn);
                }
                else
                {
                    var message = ((Reply.MsgReply)reply).Item;
                    Debug.WriteLine("Response: {0}", message);                   
                }
            }

            catch
            {
            }

            //Just navigate to garrison page with default data
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
