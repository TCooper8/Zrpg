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

        public HeroInventoryTab()
        {
            this.InitializeComponent();
        }

        private async void PopulateInventory()
        {
            Debug.WriteLine("Generating inventory.");
            var heroInventory = await state.GetHeroInventory(hero.id);

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
                    button.Background = new SolidColorBrush(Windows.UI.Colors.Black);
                    button.Background.Opacity = 0.4;
                    button.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
                    button.Width = 100;
                    button.Height = 100;

                    buttons.Add(button);
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
