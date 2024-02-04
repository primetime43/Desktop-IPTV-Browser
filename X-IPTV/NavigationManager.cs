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
            Uri pageUri = pageName switch
            {
                "Xtream Login" => new Uri("XtreamLogin.xaml", UriKind.Relative),
                "M3U Login" => new Uri("M3ULogin.xaml", UriKind.Relative),
                "Categories" => new Uri("CategoryNav.xaml", UriKind.Relative),
                "XtreamChannels" => new Uri("XtreamChannelList.xaml", UriKind.Relative),
                _ => null
            };

            if (pageUri != null)
            {
                _navigationService.Navigate(pageUri);
                (Application.Current.MainWindow as MainWindow)?.HighlightNavigationItem(pageName); // highlights the selected item in the MainWindow listbox
            }
        }
    }
}
