using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Desktop_IPTV_Browser.Views;

namespace Desktop_IPTV_Browser
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DataTemplate _selectedPageTemplate;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SelectedPage = "HomeTemplate"; // Set the default page

            var homeView = new HomeView();
            var loginView = new LoginView();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _selectedPage;
        public string SelectedPage
        {
            get => _selectedPage;
            set
            {
                _selectedPage = value;
                OnPropertyChanged(nameof(SelectedPage));
                UpdateSelectedPageTemplate();
            }
        }

        public DataTemplate SelectedPageTemplate
        {
            get => _selectedPageTemplate;
            set
            {
                _selectedPageTemplate = value;
                OnPropertyChanged(nameof(SelectedPageTemplate));
            }
        }

        private void UpdateSelectedPageTemplate()
        {
            // Update the template dynamically based on SelectedPage
            SelectedPageTemplate = FindResource(SelectedPage) as DataTemplate;
        }

        // Add this method to handle ListBox SelectionChanged event
        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem selectedItem)
            {
                // Update the SelectedPage property based on the selected item's Tag
                SelectedPage = selectedItem.Tag?.ToString();
            }
        }
    }
}
