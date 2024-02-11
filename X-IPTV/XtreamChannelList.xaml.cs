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
using static X_IPTV.M3UPlaylist;
using static X_IPTV.XtreamCodes;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for XtreamChannelList.xaml
    /// </summary>
    public partial class XtreamChannelList : Page
    {
        private XtreamChannelModel _model;
        private bool _isRightClickSelection = false;
        public XtreamChannelList()
        {
            InitializeComponent();

            this._model = new XtreamChannelModel();
            this._model.Initialize();

            if (Instance.XtreamCodesChecked)
            {
                XtreamChannelLst.DataContext = this._model;
            }
        }

        private void XtreamSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchTextBox != null && this._model != null)
            {
                var filterText = SearchTextBox.Text.ToLower();
                var filteredItems = this._model.MyListBoxItems
                    .Where(channel => (channel.ChannelName?.ToLower().Contains(filterText) ?? false) ||
                                      (channel.EPGData?.Description.ToLower().Contains(filterText) ?? false))
                    .ToList();

                XtreamChannelLst.ItemsSource = filteredItems; // Update the ItemsSource of your ListBox
            }
        }

        private void XtreamChannelLst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && Instance.XtreamCodesChecked && !_isRightClickSelection) // Check if not a right-click selection
            {
                XtreamChannel xtreamChannel = e.AddedItems[0] as XtreamChannel;
                if (xtreamChannel != null)
                {
                    // Open and show the channel options window for the selected channel
                    ChannelOptions channelOptionsPage = new ChannelOptions(xtreamChannel);
                    channelOptionsPage.Show();
                }
            }
        }

        private void listBox1_MouseDown(object sender, RoutedEventArgs e)
        {
            if (XtreamChannelLst.SelectedItem is XtreamChannel xtreamChannel && sender is MenuItem menuItem)
            {
                var commandParameter = menuItem.CommandParameter as string;
                switch (commandParameter)
                {
                    case "CopyURL":
                        Clipboard.SetText(xtreamChannel.StreamUrl);
                        Xceed.Wpf.Toolkit.MessageBox.Show("URL copied to clipboard.");
                        break;
                    case "OpenInVLC":
                        ChannelOptions.OpenStreamInVLC(xtreamChannel.StreamUrl);
                        //Xceed.Wpf.Toolkit.MessageBox.Show("Opening in VLC...");
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

        private void XtreamChannelLst_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = FindAncestorOrSelf<ListBoxItem>(e.OriginalSource as DependencyObject);
            if (item != null)
            {
                _isRightClickSelection = true; // Indicate this is a right-click selection
                XtreamChannelLst.SelectedItem = item.DataContext;
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

    public class XtreamChannelModel
    {
        private bool isInitialized = false;

        public XtreamChannelModel()
        {
            MyListBoxItems = new ObservableCollection<XtreamChannel>();
        }


        public void Initialize()
        {
            if (isInitialized) return; // Ensure logic runs only once

            if (Instance.XtreamCodesChecked)
            {
                // Get the category ID from the selected category name
                int selectedCategory = int.Parse(Instance.XtreamCategoryList
                    .FirstOrDefault(category => category.CategoryName == Instance.selectedCategory).CategoryId);

                // Find all channels that belong to the selected category by CategoryId
                var channelsInCategory = Instance.XtreamChannels
                    .Where(channel => channel.CategoryIds.Contains(selectedCategory))
                    .ToList();

                foreach (var channel in channelsInCategory)
                {
                    MyListBoxItems.Add(channel);
                }
            }
            isInitialized = true;
        }
        public ObservableCollection<XtreamChannel> MyListBoxItems { get; set; }
    }
    public class XtreamMyMockClass
    {
        public XtreamMyMockClass()
        {
            MyListBoxItems = new ObservableCollection<XtreamChannel>
            { 
            /*MyListBoxItems.Add(new XtreamChannel()
            {
                ChannelName = "|FR| TF1 UHD",
                LogoUrl = "http://f.iptv-pure.com/tf14k.png",
                EPGData = new XtreamEPGData
                {
                    ProgramTitle = "Title 1",
                    Description = "Description 1",
                    StartTime = DateTime.Now,
                }
            });
            MyListBoxItems.Add(new XtreamChannel()
            {
                ChannelName = "|FR| CSTAR FHD",
                LogoUrl = "http://f.iptv-pure.com/cstar.png",
                EPGData = new XtreamEPGData
                {
                    ProgramTitle = "Title 2",
                    Description = "Description 2",
                    StartTime = DateTime.Now.AddHours(1),
                }
            });*/
            new XtreamChannel
            {
                ChannelName = "|FR| TF1 UHD",
                LogoUrl = "http://f.iptv-pure.com/tf14k.png",
                EPGData = new XtreamEPGData
                {
                    ProgramTitle = "Title 1",
                    Description = "Description 1",
                    StartTime = DateTime.Now,
                }
            },
            new XtreamChannel
            {
                ChannelName = "|FR| CSTAR FHD",
                LogoUrl = "http://f.iptv-pure.com/cstar.png",
                EPGData = new XtreamEPGData
                {
                    ProgramTitle = "Title 2",
                    Description = "Description 2",
                    StartTime = DateTime.Now.AddHours(1),
                }
            }
            };
        }
        public ObservableCollection<XtreamChannel> MyListBoxItems { get; set; }

    }
}
