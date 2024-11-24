using System.ComponentModel;
using System.Windows;

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
    }
}
