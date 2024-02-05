using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace X_IPTV
{
    public class NavigationManager
    {
        private readonly NavigationService _navigationService;

        public NavigationManager(NavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        public void NavigateToPage(string pageName)
        {
            // Match these with the case statements in MainWindow.xaml.cs (maybe use a common enum?)
            Uri pageUri = pageName switch
            {
                "XtreamLoginPage" => new Uri("XtreamLogin.xaml", UriKind.Relative),
                "M3ULoginPage" => new Uri("M3ULogin.xaml", UriKind.Relative),
                "CategoriesPage" => new Uri("CategoryNav.xaml", UriKind.Relative),
                "XtreamChannelsPage" => new Uri("XtreamChannelList.xaml", UriKind.Relative),
                "M3UChannelPage" => new Uri("M3UChannelList.xaml", UriKind.Relative),
                _ => null
            };

            if (pageUri != null)
            {
                _navigationService.Navigate(pageUri);
                /* highlights the selected item in the MainWindow listbox & triggers the MenuList_SelectionChanged in MainWindow.xaml.cs */
                (Application.Current.MainWindow as MainWindow)?.HighlightNavigationItem(pageName);
            }
        }
    }
}
