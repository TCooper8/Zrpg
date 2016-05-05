using GUI.Pages.HeroSubPages;
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
    public sealed partial class HeroesPage : Page
    {
        ClientState state = ClientState.state;
        Dictionary<string, TextBlock> heroViews;
        Dictionary<string, ProgressBar> heroBars;
        Hero hero;
        
        public HeroesPage()
        {
            this.InitializeComponent();
            this.heroViews = new Dictionary<string, TextBlock>();
            this.heroBars = new Dictionary<string, ProgressBar>();

            GetUserGarrison();
            LoadHeroes();
            GetHeroStats();
        }

        public async Task UpdateHero(string heroId)
        {
            Debug.WriteLine("Updating hero {0}", heroId, null);

            if (listView.ItemsSource == null)
            {
                return;
            }
            var heroViews = listView.ItemsSource as List<TextBlock>;
            var xpViews = heroXpList.ItemsSource as List<ProgressBar>;
            //var newHeroViews = new List<TextBlock>();

            // Find the hero in the controls.
            var i = -1;
            foreach (var view in heroViews)
            {
                ++i;
                var hero = await state.GetHero(view.Name, true);
                Debug.WriteLine("Checking hero {0}:{1}", hero.id, hero.name);

                if (hero.id == heroId)
                {
                    // Need to update this hero.
                    var heroF = await state.GetHero(heroId, false);
                    view.Name = heroF.id;
                    view.Text = heroF.ToString();

                    var xpProgress = xpViews[i];

                    // Determine the state of the hero and set the progress.
                    if (hero.state.IsQuesting)
                    {
                        var questData = await state.GetHeroQuest(hero.id, true);
                        await this.UpdateHeroQuesting(heroId);
                        //this.SetHeroQuesting(hero.id, questData.Item2, questData.Item1);
                    }
                    else
                    {
                        await this.UpdateHeroXp(heroId);
                    }

                    Debug.WriteLine("Updated hero {0} Status = {1}", hero.name, hero.state.IsQuesting, null);
                    return;
                }
            }

            //listView.ItemsSource = newHeroViews;
        }

        private async Task UpdateHeroXp(string heroId)
        {
            var bar = heroBars[heroId];
            var hero = await state.GetHero(heroId, true);

            bar.Foreground = new SolidColorBrush(Windows.UI.Colors.Blue);

            bar.Maximum = hero.stats.finalXp;
            bar.Value = hero.stats.xp;
            bar.Width = 250;
            bar.Height = 40;
        }

        private async Task UpdateHeroQuesting(string heroId)
        {
            var data = await state.GetHeroQuest(heroId, true);
            var quest = data.Item2;
            var record = data.Item1;

            var bar = heroBars[heroId];
            var hero = await state.GetHero(heroId, true);
            if (!hero.state.IsQuesting)
            {
                return;
            }

            var ti = record.startTime;
            var dt = quest.objective.Item.timeDelta;
            var tf = ti + dt;
            var now = await state.GetGameTime(false);
            var elapsed = now - ti;
            var timeLeft = dt - elapsed;

            bar.Foreground = new SolidColorBrush(Windows.UI.Colors.Yellow);
            bar.Value = elapsed;
            bar.Maximum = timeLeft;
        }

        private async void LoadHeroes()
        {
            var reply = await state.GetHeroes();
            
            if (reply.IsSuccess)
            {
                Debug.WriteLine("Generating hero controls");
                var success = (GetHeroArrayReply.Success)reply;
                var heroes = success.Item;
                var heroViews = new List<TextBlock>();

                var xpControls = new List<ProgressBar>();

                foreach (var hero in heroes)
                {
                    var xpProgress = new ProgressBar();
                    //xpProgress.Maximum = hero.stats.finalXp;
                    //xpProgress.Value = hero.stats.xp;
                    xpProgress.Width = 250;
                    xpProgress.Height = 40;

                    xpControls.Add(xpProgress);

                    var view = new TextBlock();
                    view.Name = hero.id;
                    view.Text = hero.ToString();

                    heroViews.Add(view);
                    this.heroViews.Add(hero.id, view);
                    this.heroBars.Add(hero.id, xpProgress);

                    await this.UpdateHero(hero.id);

                    //heroControl.Items.Add(heroDisplay);
                    //heroControl.Items.Add(xpProgress);
                }

                heroXpList.ItemsSource = xpControls;
                listView.ItemsSource = heroViews;

                if (listView.Items.Count == 0)
                {
                }
                else
                {
                    listView.SelectedIndex = 0;
                }
            }
        }

        private async void GetUserGarrison()
        {
            await state.GetGarrison();
        }

        private async void GetHeroStats()
        {
            var reply = await state.GetHeroes();

            if (reply.IsSuccess)
            {
                var success = (GetHeroArrayReply.Success)reply;

                if(listView.SelectedIndex == -1)
                {
                }
                else
                {
                    hero = success.Item[listView.SelectedIndex];
                    await this.UpdateHero(hero.id);
                    hero = await state.GetHero(hero.id, true);

                    infoFrame.Content = String.Format(
                        "Name: {0}\n" +
                        "Faction: {1}\n" +
                        "Gender: {2}\n" +
                        "Race: {3}\n" +
                        "Class: {4}\n" +
                        "Level: {5}\n" +
                        "Strength: {6}\n" +
                        "Stamina: {7}\n" +
                        "Travel Speed: {8}",
                        hero.name,
                        hero.faction.ToString(),
                        hero.gender.ToString(),
                        hero.race.ToString(),
                        hero.heroClass.ToString(),
                        hero.level.ToString(),
                        hero.stats.strength.ToString(),
                        hero.stats.stamina.ToString(),
                        hero.stats.groundTravelSpeed.ToString()
                   );
                }
            }        
        }

        private void statsButton_Click(object sender, RoutedEventArgs e)
        {
            statsButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            mapButton.IsEnabled = true;

            GetHeroStats();
        }

        private void gearButton_Click(object sender, RoutedEventArgs e)
        {
            gearButton.IsEnabled = false;

            statsButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            mapButton.IsEnabled = true;
        }

        private void inventoryButton_Click(object sender, RoutedEventArgs e)
        {
            inventoryButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            statsButton.IsEnabled = true;
            mapButton.IsEnabled = true;

            infoFrame.Navigate(typeof(HeroInventoryTab), hero);
        }

        private async void mapButton_Click(object sender, RoutedEventArgs e)
        {
            mapButton.IsEnabled = false;

            gearButton.IsEnabled = true;
            inventoryButton.IsEnabled = true;
            statsButton.IsEnabled = true;

            var reply = await state.GetHeroes();

            if (reply.IsSuccess)
            {
                var success = (GetHeroArrayReply.Success)reply;

                if (listView.SelectedIndex == -1)
                {
                }
                else
                {
                    hero = success.Item[listView.SelectedIndex];
                    var param = new MapTabCell(this, hero.id);
                    infoFrame.Navigate(typeof(HeroMapTab), param);
                }
            }      
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Logic for the behavior of buttons 
            try
            {
                if (statsButton.IsEnabled == false)
                {
                    statsButton_Click(sender, e);
                }
                else if (gearButton.IsEnabled == false)
                {
                    gearButton_Click(sender, e);
                }
                else if (inventoryButton.IsEnabled == false)
                {
                    inventoryButton_Click(sender, e);
                }
                else if (mapButton.IsEnabled == false)
                {
                    mapButton_Click(sender, e);
                }
                else
                {
                    statsButton_Click(sender, e);
                }
            }
            catch
            {
            }           
        }

        private void createHeroButton_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HeroCreationPage));
        }

        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate back to garrison page
            this.Frame.Navigate(typeof(GarrisonPage));
        }

        private void settingsPageButton_Click(object sender, RoutedEventArgs e)
        {
            //Navigate to settings page
            this.Frame.Navigate(typeof(SettingsPage));
        }
    }
}
