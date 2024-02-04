using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using static X_IPTV.UserDataSaver;
using Microsoft.Win32;

namespace X_IPTV
{
    /// <summary>
    /// Interaction logic for UserLogin.xaml
    /// </summary>
    /// 
    public partial class UserLogin : Page
    {
        private static bool updateCheckDone = false;
        private CancellationTokenSource cts = new CancellationTokenSource();
        public static bool ReturnToLogin { get; set; } = false;

        public UserLogin()
        {
            InitializeComponent();
            if (!updateCheckDone)
            {
                updateCheckDone = true;
            }

            string mySetting = ConfigurationManager.GetSetting("MySettingKey");
            //MessageBox.Show("Setting: " + mySetting);
        }
    }
}
