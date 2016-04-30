using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
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
    public sealed partial class SettingsPage : Page
    {
        SettingsState state = SettingsState.state;
        
        public SettingsPage()
        {
            this.InitializeComponent();

            //Load settings page visuals
            if (state.isFullscreenEnabled == true)
            {
                fullScreenToggleSwitch.IsOn = true;
            }
            else
            {
                fullScreenToggleSwitch.IsOn = false;
            }
        }

        public void fullScreenToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            //Change settings state to current mode
            if(fullScreenToggleSwitch.IsOn == true)
            {
                state.SetFullScreenSetting(true);
            }
            else
            {
                state.SetFullScreenSetting(false);
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate back to last used page
            this.Frame.GoBack();
        }
    }
}
