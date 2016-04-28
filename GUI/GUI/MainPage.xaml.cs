using GUI.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

        // Initializes app data containers
        StorageFolder roamingFolder = null;
        ApplicationDataContainer roamingSettings = null;

        // Used to store text and settings in app data containers
        const string textFile = "fileName.txt";
        const string settingsFile = "settingFile.txt";

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;

            // Sets app data containers to current roaming folder
            roamingFolder = ApplicationData.Current.RoamingFolder;
            roamingSettings = ApplicationData.Current.RoamingSettings;
            ReadAppData();
        }

        async void ReadAppData()
        {
            try
            {
                //Reads and sets file values to last known state
                StorageFile file = await roamingFolder.GetFileAsync(textFile);
                string usernameText = await FileIO.ReadTextAsync(file);
                usernameTextBox.Text = usernameText;

                //Reads and sets setting values to last known state
                Object value = roamingSettings.Values[settingsFile];
                if (value.ToString() == "True")
                {
                    rememberCheckBox.IsChecked = true;
                }
                else
                {
                    rememberCheckBox.IsChecked = false;                    
                }
            }
            catch
            {
            }
        }

        async void WriteAppData()
        {
            StorageFile file = await roamingFolder.CreateFileAsync(textFile, CreationCollisionOption.ReplaceExisting);

            //Stores the file app data
            if (rememberCheckBox.IsChecked.Value == true)
            {               
                await FileIO.WriteTextAsync(file, usernameTextBox.Text);
            }
            else
            {
                await FileIO.WriteTextAsync(file, "");
            }

            //Stores the settings app data
            roamingSettings.Values[settingsFile] = rememberCheckBox.IsChecked.ToString();
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            // Write current app data
            WriteAppData();

            // Behavior for loading visuals. Using a delay for testing purposes
            relativePanel.Visibility = Visibility.Visible; 
            progressRing.IsActive = true;
            await Task.Delay(2000);
            
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
            // Navigate to register page
            this.Frame.Navigate(typeof(RegisterPage));
        }

        private void soundMuteButton_Click(object sender, RoutedEventArgs e)
        {
            char uVolume = '\uE767';
            char uMute = '\uE74F';

            string volume = uVolume.ToString();
            string mute = uMute.ToString();

            if(soundMuteButton.Content.ToString() == volume)
            {
                soundMuteButton.Content = mute;
                mediaElement.Stop();
            }
            else
            {
                soundMuteButton.Content = volume;
                mediaElement.Play();
            }
        }
    }
}
