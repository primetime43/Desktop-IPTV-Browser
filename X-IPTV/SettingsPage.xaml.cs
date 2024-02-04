using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using static X_IPTV.GitHubReleaseChecker;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void checkForUpdate_Btn_Click(object sender, RoutedEventArgs e)
        {
            checkForUpdate();
        }

        private async void checkForUpdate()
        {
            var release = await ReleaseChecker.CheckForNewRelease("primetime43", "Xtream-Browser");
            string programVersion = Instance.programVersion;

            if (release != null)
            {
                int latestReleaseInt = ReleaseChecker.convertVersionToInt(release.tag_name);
                int localProgramVersionInt = ReleaseChecker.convertVersionToInt(programVersion);

                if (latestReleaseInt > localProgramVersionInt)
                {
                    MessageBoxResult result = Xceed.Wpf.Toolkit.MessageBox.Show("Current version: " + programVersion + "\nNew release available: " + release.name + " (" + release.tag_name + ")\nDo you want to download it?", "New Release", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        try
                        {
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = ReleaseChecker.releaseURL,
                                UseShellExecute = true
                            };

                            Process.Start(startInfo);
                        }
                        catch (System.ComponentModel.Win32Exception ex)
                        {
                            Xceed.Wpf.Toolkit.MessageBox.Show("An error occurred: " + ex.Message);
                        }
                    }
                }
                else if(latestReleaseInt == localProgramVersionInt)
                {
                    Xceed.Wpf.Toolkit.MessageBox.Show("You are using the latest version.");
                }
                else
                    Debug.WriteLine("Release null, skipping check.");
            }
            else
            {
                Xceed.Wpf.Toolkit.MessageBox.Show("No new releases available.");
            }
        }
    }
}
