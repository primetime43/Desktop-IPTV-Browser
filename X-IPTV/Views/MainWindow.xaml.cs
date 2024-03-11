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
using X_IPTV.Utilities;

namespace X_IPTV.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Initialize the configuration settings
            ConfigurationManager.InitializeConfiguration();

            this.Title = "User Login " + Instance.programVersion;
            ContentFrame.Navigate(new Uri("Views/XtreamLogin.xaml", UriKind.Relative));
            HighlightNavigationItem("XtreamLoginPage");
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem != null)
            {
                var selectedItem = listBox.SelectedItem as ListBoxItem;
                if (selectedItem != null)
                {
                    switch (selectedItem.Name.ToString())
                    {
                        case "XtreamLoginPage":
                            ContentFrame.Navigate(new Uri("Views/XtreamLogin.xaml", UriKind.Relative));
                            break;
                        case "M3ULoginPage":
                            ContentFrame.Navigate(new Uri("Views/M3ULogin.xaml", UriKind.Relative));
                            break;
                        case "CategoriesPage":
                            ContentFrame.Navigate(new Uri("Views/CategoryNav.xaml", UriKind.Relative));
                            CategoriesPage.Visibility = Visibility.Visible;
                            AllChannelsSearchPage.Visibility = Visibility.Visible;
                            break;
                        case "XtreamChannelsPage":
                            ContentFrame.Navigate(new Uri("Views/XtreamChannelList.xaml", UriKind.Relative));
                            XtreamChannelsPage.Visibility = Visibility.Visible;
                            break;
                        case "M3UChannelPage":
                            ContentFrame.Navigate(new Uri("Views/M3UChannelList.xaml", UriKind.Relative));
                            M3UChannelPage.Visibility = Visibility.Visible;
                            break;
                        case "AllChannelsSearchPage":
                            ContentFrame.Navigate(new Uri("Views/UniversalSearchList.xaml", UriKind.Relative));
                            break;
                        case "AppSettings":
                            ContentFrame.Navigate(new Uri("Views/SettingsPage.xaml", UriKind.Relative));
                            break;
                    }
                }
                (Application.Current.MainWindow as MainWindow)?.HighlightNavigationItem(selectedItem.Name.ToString()); // highlights the selected item in the MainWindow listbox
            }
        }

        public void HighlightNavigationItem(string pageName)
        {
            foreach (var item in SideMenu.Items.OfType<ListBoxItem>())
            {
                if (item.Name == pageName)
                {
                    SideMenu.SelectedItem = item;
                    break; // Break once the matching item is found and selected
                }
            }
        }
    }
}