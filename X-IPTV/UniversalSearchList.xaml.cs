using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using static X_IPTV.XtreamCodes;
using static X_IPTV.M3UPlaylist;
using System.Collections;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class UniversalSearchList : Page
    {
        private UniversalSearchModel _model;
        private bool _isRightClickSelection = false;
        public UniversalSearchList()
        {
            InitializeComponent();
            _model = new UniversalSearchModel();
            _model.Initialize(); // populate the allChannels list
        }

        private void USearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (USearchTextBox != null)
            {
                var filterText = USearchTextBox.Text.ToLower();

                IList currentList = Instance.XtreamCodesChecked
                    ? (IList)_model.MyXtreamListBoxItems
                    : (IList)_model.MyM3uListBoxItems;

                currentList.Clear();
                var searchResult = Instance.XtreamCodesChecked
                    ? _model.SearchXtreamChannels(filterText).Cast<object>()
                    : _model.SearchM3uChannels(filterText).Cast<object>();

                foreach (var item in searchResult)
                {
                    currentList.Add(item);
                }

                // The UI will now update to show only filtered items
                USearchChannelLst.ItemsSource = currentList;
            }
        }

        private void USearchChannelLst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && Instance.XtreamCodesChecked && !_isRightClickSelection) // Check if not a right-click selection
            {
                XtreamChannel xtreamChannel = e.AddedItems[0] as XtreamChannel;
                if (xtreamChannel != null)
                {
                    ChannelOptions channelOptionsPage = new ChannelOptions(xtreamChannel);
                    channelOptionsPage.Show();
                }
            }
            else if (e.AddedItems.Count > 0 && Instance.M3uChecked && !_isRightClickSelection) // Check if not a right-click selection
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
            if (sender is MenuItem menuItem)
            {
                var commandParameter = menuItem.CommandParameter as string;

                // Define an action to handle common logic
                Action<string> handleAction = (streamUrl) =>
                {
                    switch (commandParameter)
                    {
                        case "CopyURL":
                            Clipboard.SetText(streamUrl);
                            Xceed.Wpf.Toolkit.MessageBox.Show("URL copied to clipboard.");
                            break;
                        case "OpenInVLC":
                            ChannelOptions.OpenStreamInVLC(streamUrl);
                            //Xceed.Wpf.Toolkit.MessageBox.Show("Opening in VLC...");
                            break;
                        default:
                            Xceed.Wpf.Toolkit.MessageBox.Show("Unknown action.");
                            break;
                    }
                };

                // Use pattern matching to simplify type checks and conditions
                if (USearchChannelLst.SelectedItem is XtreamChannel xtreamChannel && Instance.XtreamCodesChecked)
                {
                    handleAction(xtreamChannel.StreamUrl);
                }
                else if (USearchChannelLst.SelectedItem is M3UChannel m3UChannel && Instance.M3uChecked)
                {
                    handleAction(m3UChannel.StreamUrl);
                }
                else
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("No channel selected.");
                }
            }
        }

        private void USearchChannelLst_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = FindAncestorOrSelf<ListBoxItem>(e.OriginalSource as DependencyObject);
            if (item != null)
            {
                _isRightClickSelection = true; // Indicate this is a right-click selection
                USearchChannelLst.SelectedItem = item.DataContext;
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

    public class UniversalSearchModel
    {
        private bool isInitialized = false;
        private List<XtreamChannel> allXtreamChannels;
        private List<M3UChannel> allM3uChannels;

        public ObservableCollection<XtreamChannel> MyXtreamListBoxItems { get; set; }
        public ObservableCollection<M3UChannel> MyM3uListBoxItems { get; set; }

        public UniversalSearchModel()
        {
            MyXtreamListBoxItems = new ObservableCollection<XtreamChannel>();
            MyM3uListBoxItems = new ObservableCollection<M3UChannel>();
            allXtreamChannels = new List<XtreamChannel>();
            allM3uChannels = new List<M3UChannel>();
        }


        public void Initialize()
        {
            if (isInitialized) return; // Ensure logic runs only once

            if (Instance.XtreamCodesChecked)
            {
                allXtreamChannels.AddRange(Instance.XtreamChannels); // Load all channels, but do not add to MyListBoxItems
            }
            else if(Instance.M3uChecked)
            {
                allM3uChannels.AddRange(Instance.M3UChannels); // Load all channels, but do not add to MyListBoxItems
            }
            isInitialized = true;
        }

        public IEnumerable<XtreamChannel> SearchXtreamChannels(string filterText)
        {
            // Return an empty list if allXtreamChannels is null
            if (allXtreamChannels == null)
            {
                return Enumerable.Empty<XtreamChannel>();
            }

            // Use StringComparison.OrdinalIgnoreCase for a case-insensitive comparison that is also faster than converting strings to lower case.
            var comparisonType = StringComparison.OrdinalIgnoreCase;

            return allXtreamChannels
                .Where(channel => channel != null && // Check that the channel is not null
                    (channel.ChannelName != null && channel.ChannelName.IndexOf(filterText, comparisonType) >= 0) ||
                    (channel.EPGData != null && channel.EPGData.Description != null && channel.EPGData.Description.IndexOf(filterText, comparisonType) >= 0));
        }

        public IEnumerable<M3UChannel> SearchM3uChannels(string filterText)
        {
            // Return an empty list if allM3uChannels is null
            if (allM3uChannels == null)
            {
                return Enumerable.Empty<M3UChannel>();
            }

            // Use StringComparison.OrdinalIgnoreCase for a case-insensitive comparison that is also faster than converting strings to lower case.
            var comparisonType = StringComparison.OrdinalIgnoreCase;

            return allM3uChannels
                .Where(channel => channel != null && // Check that the channel is not null
                    (channel.ChannelName != null && channel.ChannelName.IndexOf(filterText, comparisonType) >= 0) ||
                    (channel.EPGData != null && channel.EPGData.Description != null && channel.EPGData.Description.IndexOf(filterText, comparisonType) >= 0));
        }
    }
    public class UniversalSearchMyMockClass
    {
        public UniversalSearchMyMockClass()
        {
            MyListBoxItems = new ObservableCollection<XtreamChannel>();
            MyListBoxItems.Add(new XtreamChannel()
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
            });
        }
        public ObservableCollection<XtreamChannel> MyListBoxItems { get; set; }

    }
}
