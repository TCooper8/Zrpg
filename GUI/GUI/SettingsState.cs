using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ViewManagement;

namespace GUI
{
    public class SettingsState
    {
        public static SettingsState state = new SettingsState();

        //Local settings file
        ApplicationDataContainer localSettings = null;
        const string settingsFile = "settingsFile.txt";

        public bool isFullscreenEnabled;

        private SettingsState()
        {
            localSettings = ApplicationData.Current.LocalSettings;
        }

       public void LoadAllSettings()
        {
            //Retrive settings to last known state
            Object value = localSettings.Values[settingsFile];            

            try
            {
                //Last known state was fullscreen
                if (value.ToString() == "True")
                {
                    isFullscreenEnabled = true;
                    SetFullScreenSetting(isFullscreenEnabled);
                }
                //Last known state was windowed mode
                else
                {
                    isFullscreenEnabled = false;
                    SetFullScreenSetting(isFullscreenEnabled);
                }
            }
            catch
            {
            }
        }
        
        public void SetFullScreenSetting(bool isEnabled)
        {
            var view = ApplicationView.GetForCurrentView();           

            //Sets the full screen setting and saves new state
            if (isEnabled == true)
            {
                view.TryEnterFullScreenMode();
                isFullscreenEnabled = true;
            }
            else
            {
                view.ExitFullScreenMode();
                isFullscreenEnabled = false;
            }

            SaveAllSettings();
        }

        public void SaveAllSettings()
        {
            //Saves full screen setting
            localSettings.Values[settingsFile] = isFullscreenEnabled;
        }
    }
}
