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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Zrpg.Game;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GUI.Pages.HeroSubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HeroInventoryTab : Page
    {
        ClientState state = ClientState.state;
        Hero hero;
        Dictionary<string, ItemSlot> itemSlots;
        Dictionary<string, Button> slotButtons;

        public HeroInventoryTab()
        {
            this.InitializeComponent();
        }

        private async Task<Brush> LoadItemAssetIcon(ItemSlot slot)
        {
            var recordOption = slot.itemRecordId;
            if (recordOption.IsGameNone)
            {
                return new SolidColorBrush(Windows.UI.Colors.Black);
            }

            var recordId = (recordOption as GameOption<string>.GameSome).Item;
            var data = await state.GetItemRecordData(recordId);
            var item = data.Item2;

            var image = new ImageBrush();
            var path = String.Format("ms-appx:///Assets/{0}", item.assetId);
            image.ImageSource = new BitmapImage(new Uri(path));
            return image;
        }

        private async void PopulateInventory()
        {
            Debug.WriteLine("Generating inventory.");
            var heroInventory = await state.GetHeroInventory(hero.id);
            itemSlots = new Dictionary<string, ItemSlot>();
            slotButtons = new Dictionary<string, Button>();

            this.InventoryPivot.Items.Clear();

            foreach (var pane in heroInventory.panes)
            {
                Debug.WriteLine("Generating pane {0}", pane.position);
                // Add a pane to the view.
                //this.InventoryPivot
                var paneControl = new PivotItem();
                Debug.WriteLine("Created pivot item");

                paneControl.Header = pane.position.ToString();
                Debug.WriteLine("Creating list view");
                var view = new GridView();
                Debug.WriteLine("Creating buttons");
                var buttons = new List<Button>();

                // Add all of the slots into the pane.
                foreach (var slot in pane.slots)
                {
                    Debug.WriteLine("Generating slot {0}", slot.position);

                    var button = new Button();
                    button.Name = String.Format("{0}:{1}", pane.position, slot.position);
                    //button.Background = new SolidColorBrush(Windows.UI.Colors.Black);
                    button.Background = await this.LoadItemAssetIcon(slot);

                    if (slot.itemRecordId.IsGameSome)
                    {
                        var recordId = (slot.itemRecordId as GameOption<string>.GameSome).Item;
                        var data = await state.GetItemRecordData(recordId);
                        button.Content = data.Item1.quantity;
                        button.FontSize = 25.0;
                        button.HorizontalContentAlignment = HorizontalAlignment.Left;
                        button.VerticalContentAlignment = VerticalAlignment.Top;
                    }

                    button.Background.Opacity = 0.4;
                    button.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                    button.Width = 100;
                    button.Height = 100;

                    button.Click += Button_Click;

                    buttons.Add(button);
                    Debug.WriteLine("Generated button {0}", button.Name, null);
                    itemSlots.Add(button.Name, slot);
                    slotButtons.Add(button.Name, button);
                }

                Debug.WriteLine("Binding sources");

                view.ItemsSource = buttons;
                Debug.WriteLine("Setting pane content");
                paneControl.Content = view;
                Debug.WriteLine("Adding items");
                this.InventoryPivot.Items.Add(paneControl);
                this.InventoryPivot.SelectedIndex = 0;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var slot = itemSlots[button.Name];
            var recordOption = slot.itemRecordId;

            if (recordOption.IsGameSome)
            {
                var recordId = ((GameOption<string>.GameSome)recordOption).Item;
                var data = await state.GetItemRecordData(recordId);
                var record = data.Item1;
                var item = data.Item2;

                Debug.WriteLine("Adding item {0} to inventory", item.id, null);
                var info = item.info;

                // Determine the kind of item.
                if (info.IsTradeGood)
                {
                    var tradeGood = (info as ItemInfo.TradeGood).Item;
                    Debug.WriteLine("Adding trade good {0} to inventory", tradeGood.name, null);

                    itemTitle.Text = "TradeGood: " + tradeGood.name;
                    itemTitle.Foreground = new SolidColorBrush(Windows.UI.Colors.Gray);

                }

                //itemTitle.Text = 
            }
            else
            {
                itemTitle.Text = "Empty";
                itemInformation.Text = "";
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.hero = e.Parameter as Hero;
            base.OnNavigatedTo(e);

            if (hero != null)
            {
                this.PopulateInventory();
            }
        }

        private void sellButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void toVaultButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //itemTextBlock.Text = "Item Index: ";

            if (e.AddedItems.Count >= 1)
            {
                var button = e.AddedItems[0] as Button;
            }
            //gridViewTab1.SelectedIndex = -1;
            //gridViewTab2.SelectedIndex = -1;
        }
    }
}
