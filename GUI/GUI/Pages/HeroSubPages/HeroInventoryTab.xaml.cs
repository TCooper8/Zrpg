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

        private void gridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            itemTextBlock.Text = "Item Index: " + gridView.SelectedIndex.ToString();
        }

        private void sellButton_Click(object sender, RoutedEventArgs e)
        {
            gridView.Items.Remove(gridView.SelectedItem);
            itemTextBlock.Text = "Item sold!";
        }

        private void toVaultButton_Click(object sender, RoutedEventArgs e)
        {
            gridView.Items.Remove(gridView.SelectedItem);
            itemTextBlock.Text = "Item moved to vault!";
        }
    }
}
