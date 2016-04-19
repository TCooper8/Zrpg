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
using Zrpg.Game;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        IGameClient client;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            client = GameClient.RESTClient("http://localhost:8080");
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            //TO DO: Authenticate credentials

            var reply = await client.AddGarrison("test client", "My garrison", Race.Human, Faction.Alliance);
            if (reply.IsEmptyReply)
            {
                throw new Exception("Got empty reply from server");
            }
            else if (reply.IsExnReply)
            {
                throw ((Reply.ExnReply)reply).Item;
            }

            var message = ((Reply.MsgReply)reply).Item;
            // Yay! Got the message.

            //Navigate to new faction page if no previous garrison
            this.Frame.Navigate(typeof(ChooseFactionPage));

            //TO DO: Navigate to garrison page if user has previous garrison
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to register page
            this.Frame.Navigate(typeof(RegisterPage));
        }
    }
}
