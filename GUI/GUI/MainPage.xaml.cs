using GUI.Pages;
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
        static ClientState state = ClientState.state;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            //TO DO: Authenticate credentials
            
            // This should populate the clientId.
            state.Authenticate(
                this.usernameTextBox.Text,
                this.passwordBox.Password
            );

            // After authenticating, check to see if the client has a garrison already.
            var reply = await state.GetGarrison();
            if (reply.IsEmpty)
            {
                // The client has no Garrison, take them to create a new one.
                this.Frame.Navigate(typeof(ChooseFactionPage));
                return;
            }
            else
            {
                // The client has a garrison, take them to the garrison page.
                this.Frame.Navigate(typeof(GarrisonPage));
            }
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to register page
            this.Frame.Navigate(typeof(RegisterPage));
        }
    }
}
