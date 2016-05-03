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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GUI.Pages.HeroSubPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HeroInventoryTab : Page
    {
        public HeroInventoryTab()
        {
            this.InitializeComponent();
        }

        private void sellButton_Click(object sender, RoutedEventArgs e)
        {
            gridViewTab1.Items.Remove(gridViewTab1.SelectedItem);
            gridViewTab2.Items.Remove(gridViewTab2.SelectedItem);
            itemTextBlock.Text = "Item sold!";
        }

        private void toVaultButton_Click(object sender, RoutedEventArgs e)
        {
            gridViewTab1.Items.Remove(gridViewTab1.SelectedItem);
            gridViewTab2.Items.Remove(gridViewTab2.SelectedItem);
            itemTextBlock.Text = "Item moved to vault!";
        }

        private void gridViewTab1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            itemTextBlock.Text = "Item Index: " + gridViewTab1.SelectedIndex;
        }

        private void gridViewTab2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            itemTextBlock.Text = "Item Index: " + gridViewTab2.SelectedIndex;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            itemTextBlock.Text = "Item Index: ";
            gridViewTab1.SelectedIndex = -1;
            gridViewTab2.SelectedIndex = -1;
        }
    }
}
