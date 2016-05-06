using GUI.Pages.HeroSubPages;
using GUI.Pages.HeroSubPages.ZoneTestPages;
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
    public sealed partial class HeroMapTab : Page
    {
        ClientState state = ClientState.state;

        string heroId;
        HeroesPage heroesPage;

        Dictionary<string, Button> zoneButtons;
        Dictionary<string, Zone> zones;
        Dictionary<string, Region> ownedRegions;
        Dictionary<string, AssetPositionInfo> zonePositions;

        Quest activeQuest;

        public HeroMapTab()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var cell = e.Parameter as MapTabCell;
            this.heroId = cell.HeroId;
            this.heroesPage = cell.HeroesPage;

            UpdateHeroStatus();
            //zoneInfoTextBlock.Text = "Garrison Info";
            //informationTextBlock.Text = "Gold Income: 580 per day\n" +
            //                            "Soldiers: 250\n" +
            //                            "Trade Influence: +10%\n" +
            //                            "Holy Power: 5\n";
        }

        //Get client garrison information (owned regions, owned zones)

        //Update map tab with current information (quests available in zones)

        //Get current hero selected information (current quest, quest status, quest reward, location)

        //Update hero location location and information
        private async void UpdateHeroStatus()
        {
            zoneButtons = new Dictionary<string, Button>();
            zones = new Dictionary<string, Zone>();
            ownedRegions = new Dictionary<string, Region>();
            zonePositions = new Dictionary<string, AssetPositionInfo>();

            var _regions = await state.GetOwnedRegions();
            foreach (var region in _regions)
            {
                Debug.WriteLine("Populating region {0}:{1}", region.id, region.name, null);

                this.ownedRegions.Add(region.id, region);
                // Get the zones in each region.
                var _zones = await state.GetRegionZones(region);
                // Should list all viewable zones in the region.
                foreach (var zone in _zones)
                {
                    Debug.WriteLine("Populating region {0} zone {1}:{2}", region.name, zone.id, zone.name);

                    zones.Add(zone.id, zone);
                }

                // Get all of the zone position information.

                var positionInfos = await state.GetZoneAssetPositionInfo(_zones.Select(zone => zone.id));
                foreach (var info in positionInfos)
                {
                    zonePositions.Add(info.id, info);
                }
            }

            // Go through the owned zones and give them a button.
            var _ownedZoneIds = state.GetOwnedZoneIds();
            var m = this.garrisonButton;

            foreach (var zoneId in _ownedZoneIds)
            {
                Debug.WriteLine("Getting zone {0}", zoneId, null);
                var zone = zones[zoneId];
                Debug.WriteLine("Getting asset info for zone {0}:{1}", zone.id, zone.name);
                var info = zonePositions[zoneId];
                Debug.WriteLine("Asset info for zone {0} = {1}", zone, info.ToString());
                // Create a button for the zone.
                var button = new Button();
                button.Width = image1.ActualWidth * info.right - image1.ActualWidth * info.left;
                button.Height = image1.ActualHeight * info.bottom - image1.ActualHeight * info.top;
                button.Margin = new Thickness(
                    info.left * image1.ActualWidth,
                    info.top * image1.ActualHeight,
                    0,
                    0
                );
                button.HorizontalAlignment = HorizontalAlignment.Left;
                button.VerticalAlignment = VerticalAlignment.Top;
                button.Click += Button_Click;
                button.Name = zone.id;

                this.mapGrid.Children.Add(button);
                Grid.SetColumn(button, 1);
                zoneButtons.Add(zone.id, button);
            }
            Debug.WriteLine("Asset info loaded");

            var hero = await state.GetHero(heroId, true);
            Zone heroZone = zones[hero.zoneId];
            heroLocationText.Text = hero.name + "'s Current Location: " + heroZone.name;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var zone = zones[button.Name];
            listView.Items.Clear();

            zoneInfoTextBlock.Text = zone.name + " info:";           

            var data = new List<string>();
            data.Add(String.Format("Terrain = {0}", zone.terrain.ToString()));

            var quests = await state.GetZoneQuests(zone.id);
            foreach (var quest in quests)
            {
                Debug.WriteLine("Loading quest view for {0}", quest.title, null);

                var questMsg = String.Format(
                    "Quest: {0}",
                    quest.title
                );
                var view = new TextBlock();
                view.Name = quest.id;
                view.Text = questMsg;

                listView.Items.Add(view);
            }

            informationTextBlock.Text = "\n  " + String.Join("\n  ", data);
        }

        private void garrisonButton_Click(object sender, RoutedEventArgs e)
        {
            zoneInfoTextBlock.Text = "Garrison Info:";
            informationTextBlock.Text = "Gold Income: 580 per day\n" +
                                        "Soldiers: 250\n" +
                                        "Trade Influence: +10%\n" +
                                        "Holy Power: 5\n";
            listView.Items.Clear();
        }

        private void sendHeroButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateHeroStatus();
        }

        private void image1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var pair in zoneButtons)
            {
                var button = pair.Value;
                var zoneId = pair.Key;
                var zone = zones[zoneId];
                var info = zonePositions[zoneId];

                button.Width = image1.ActualWidth * info.right - image1.ActualWidth * info.left;
                button.Height = image1.ActualHeight * info.bottom - image1.ActualHeight * info.top;
                button.Margin = new Thickness(
                    info.left * image1.ActualWidth,
                    info.top * image1.ActualHeight,
                    0,
                    0
                );
            }

        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("Selection changed");


            //Update title text block
            //titleTextBlock.Text = ?


            if (listView.SelectedItem == null)
            {
                this.activeQuest = null;
                questPopUp.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                return;
            }

            Debug.WriteLine("Loading selection control for quest");
            var view = listView.SelectedItem as TextBlock;
            Debug.WriteLine("Loading quest from control {0}", view.Name, null);
            var quest = state.GetQuest(view.Name);
            this.activeQuest = quest;

            Debug.WriteLine("Loading quest dialogue for {0}", quest.title, null);

            titleTextBlock.Text = quest.title;
            bodyTextBlock.Text = quest.body;

            //Change view to visible
            questPopUp.Visibility = Windows.UI.Xaml.Visibility.Visible;

            //Update body text block
            //bodyTextBlock.Text = 
        }

        private async void BeginPollingHeroQuestState (QuestRecord record, Quest quest)
        {
            var hero = await state.GetHero(heroId, true);
            // Estimate the time to quest completion.
            var ti = record.startTime;
            var ticks = quest.objective.Item.timeDelta;
            var tf = ti + ticks + 2;
            var cur = await state.GetGameTime(false);

            // If the current game time is less than the tf, the quest will not be resolved.
            while (cur <= tf)
            {
                await Task.Delay(1000);
                cur = await state.GetGameTime(false);
                await heroesPage.UpdateHero(hero.id);
            }
        }

        private async void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            var quest = activeQuest;
            await state.BeginHeroQuest(heroId, quest.id);

            //Change view to collapsed
            questPopUp.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            //Add accept behavior here
            listView.SelectedIndex = -1;
            activeQuest = null;

            var data = await state.GetHeroQuest(heroId, false);
            // Start a task to update the hero once the quest is done.
            BeginPollingHeroQuestState(data.Item1, data.Item2);
        }

        private void declineButton_Click(object sender, RoutedEventArgs e)
        {
            //Change view to collapsed
            questPopUp.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            //Add decline behavior here
            listView.SelectedIndex = -1;
            activeQuest = null;
        }
    } 
}
