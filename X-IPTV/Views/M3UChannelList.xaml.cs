using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using X_IPTV.Models;
using static X_IPTV.M3UPlaylist;
using X_IPTV.Utilities;

namespace X_IPTV.Views
{
    /// <summary>
    /// Interaction logic for M3UChannelList.xaml
    /// </summary>
    public partial class M3UChannelList : Page
    {
        private M3UChannelModel _model;
        private bool _isRightClickSelection = false;
        public M3UChannelList()
        {
            InitializeComponent();

            this._model = new M3UChannelModel();
            this._model.Initialize();

            //the model is the array that holds all of the M3UChannel Objects set in the M3UChannelModel class
            if (Instance.M3uChecked)
            {
                M3UChannelLst.DataContext = this._model;
            }
        }

        private void M3USearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox != null && this._model != null)
            {
                var filterText = SearchTextBox.Text.ToLower();
                var filteredItems = this._model.MyListBoxItems
                    .Where(channel => (channel.ChannelName?.ToLower().Contains(filterText) ?? false) ||
                                      (channel.EPGData?.Description?.ToLower().Contains(filterText) ?? false))
                    .ToList();

                M3UChannelLst.ItemsSource = filteredItems; // Update the ItemsSource of your ListBox
            }
        }

        private void M3UChannelLst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && Instance.M3uChecked && !_isRightClickSelection) // Check if not a right-click selection
            {
                M3UChannel m3uChannel = e.AddedItems[0] as M3UChannel;
                if (m3uChannel != null)
                {
                    ChannelOptions channelOptionsPage = new ChannelOptions(m3uChannel);
                    channelOptionsPage.Show();
                }
            }
        }

        private void listBox1_MouseDown(object sender, RoutedEventArgs e)
        {
            if (M3UChannelLst.SelectedItem is M3UChannel m3uChannel && sender is MenuItem menuItem)
            {
                var commandParameter = menuItem.CommandParameter as string;
                switch (commandParameter)
                {
                    case "CopyURL":
                        Clipboard.SetText(m3uChannel.StreamUrl);
                        Xceed.Wpf.Toolkit.MessageBox.Show("URL copied to clipboard.");
                        break;
                    case "OpenInPlayer":
                        string defaultPlayer = ConfigurationManager.GetDefaultPlayer();
                        string playerKey = defaultPlayer == "vlc" ? "vlcLocationPath" : "genericPlayerPath";

                        ChannelOptions.OpenStreamInPlayer(m3uChannel.StreamUrl, playerKey);
                        break;
                    default:
                        Xceed.Wpf.Toolkit.MessageBox.Show("Unknown action.");
                        break;
                }
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No channel selected.");
            }
        }

        private void M3UChannelLst_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = FindAncestorOrSelf<ListBoxItem>(e.OriginalSource as DependencyObject);
            if (item != null)
            {
                _isRightClickSelection = true; // Indicate this is a right-click selection
                M3UChannelLst.SelectedItem = item.DataContext;
                _isRightClickSelection = false; // Reset the flag
            }
            /* Important: This event handler is used to prevent the ListBox selection from showing on a right-click
             * Tells this event has been fully handled. Do not continue routing this event to other handlers
            */
            e.Handled = true;
        }

        // Helper method to find the ListBoxItem that is the ancestor of the current target.
        public static T FindAncestorOrSelf<T>(DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                if (obj is T tObj)
                    return tObj;
                obj = VisualTreeHelper.GetParent(obj);
            }
            return null;
        }
    }

    public class M3UMyMockClass
    {
        public M3UMyMockClass()
        {
            MyListBoxItems = new ObservableCollection<M3UChannel>
        {
            new M3UChannel
            {
                ChannelName = "|FR| TF1 UHD",
                LogoUrl = "https://avatars.githubusercontent.com/u/12754111",
                EPGData = new M3UEPGData
                {
                    ProgramTitle = "Title 1",
                    Description = "Description 1",
                    StartTime = DateTime.Now,
                }
            },
            new M3UChannel
            {
                ChannelName = "|FR| CSTAR FHD",
                LogoUrl = "https://avatars.githubusercontent.com/u/12754111",
                EPGData = new M3UEPGData
                {
                    ProgramTitle = "Title 2",
                    Description = "Description 2",
                    StartTime = DateTime.Now.AddHours(1)
                }
            }
        };
        }
        public ObservableCollection<M3UChannel> MyListBoxItems { get; set; }
    }
}
