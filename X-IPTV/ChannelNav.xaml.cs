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
            List<string> sortedCategories = new List<string>(Instance.categories.Keys);
            sortedCategories.Sort();
            for (int i = 0; i < sortedCategories.Count; i++)
            {
                categoriesComboBox.Items.Add(sortedCategories[i]);
            }
        }

        private void searchForChannelByCurrentShow()
        {

        }

        private static void searchForChannelByChannelName()
        {

        }

        private void categoriesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Instance.selectedCategory = this.categoriesComboBox.SelectedItem.ToString();
            this.Close();
        }

        private void quitBtn_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(1);
        }
    }
}
