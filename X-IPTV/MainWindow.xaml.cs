using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this;
            ContentFrame.Navigate(new Uri("UserLogin.xaml", UriKind.Relative));

            var optionsPopup = new OptionsPopup();
            optionsPopup.ButtonClickEvent += navigateToPageTest;
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem != null)
            {
                var selectedItem = listBox.SelectedItem as ListBoxItem;
                if (selectedItem != null)
                {
                    switch (selectedItem.Content.ToString())
                    {
                        case "Login":
                            ContentFrame.Navigate(new Uri("UserLogin.xaml", UriKind.Relative));
                            break;
                        case "Xtream Login":
                            ContentFrame.Navigate(new Uri("XtreamLogin.xaml", UriKind.Relative));
                            break;
                        case "M3U Login":
                            ContentFrame.Navigate(new Uri("M3ULogin.xaml", UriKind.Relative));
                            break;
                        case "Categories":
                            ContentFrame.Navigate(new Uri("CategoryNav.xaml", UriKind.Relative));
                            break;
                        /*case "Channels":
                            ContentFrame.Navigate(new Uri("ChannelOptions.xaml", UriKind.Relative));
                            break;*/
                        case "ChannelOptions":
                            ContentFrame.Navigate(new Uri("ChannelOptions.xaml", UriKind.Relative));
                            break;
                        case "Search":
                            ContentFrame.Navigate(new Uri("UniversalSearchList.xaml", UriKind.Relative));
                            break;
                        case "Settings":
                            ContentFrame.Navigate(new Uri("SettingsPage.xaml", UriKind.Relative));
                            break;
                    }
                }
            }
        }

        public void navigateToPageTest()
        {
            ContentFrame.Navigate(new Uri("XtreamLogin.xaml", UriKind.Relative));
        }
    }
}