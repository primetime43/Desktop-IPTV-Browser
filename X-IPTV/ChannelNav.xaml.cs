using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for ChannelNav.xaml
    /// </summary>
    public partial class ChannelNav : Window
    {
        public ChannelNav()
        {
            InitializeComponent();
            loadCategories();
        }

        private void loadCategories()
        {
            List<ChannelGroups> itemsTest = Instance.ChannelGroupsArray.OrderBy(x => x.category_name + " test").ToList();
            listViewTest.ItemsSource = itemsTest;


            //Could eventually use an api call each time to just get the channels that are in the selected category.
            //player_api.php?username=X&password=X&action=get_live_streams&category_id=X (This will get All LIVE Streams in the selected category ONLY)
        }

        private void searchForChannelByCurrentShow()
        {

        }

        private static void searchForChannelByChannelName()
        {

        }

        private void quitBtn_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            Instance.selectedCategory = tb.Text;
            this.Close();
        }
    }
}
