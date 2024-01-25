using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace X_IPTV
{
    public partial class OptionsPopup : UserControl
    {
        public OptionsPopup()
        {
            InitializeComponent();
        }

        // Textbox
        public static readonly DependencyProperty TextBoxTextProperty = DependencyProperty.Register("TextBoxText", typeof(string), typeof(OptionsPopup), new PropertyMetadata(default(string)));

        public string TextBoxText
        {
            get { return (string)GetValue(TextBoxTextProperty); }
            set { SetValue(TextBoxTextProperty, value); }
        }

        public static readonly DependencyProperty TextBoxHintProperty = DependencyProperty.Register("TextBoxHint", typeof(string), typeof(OptionsPopup), new PropertyMetadata("Setting 2"));

        public string TextBoxHint
        {
            get { return (string)GetValue(TextBoxHintProperty); }
            set { SetValue(TextBoxHintProperty, value); }
        }
        //End Textbox

        //Button
        public static readonly DependencyProperty Button1ContentProperty = DependencyProperty.Register("Button1Content", typeof(string), typeof(OptionsPopup), new PropertyMetadata("Default Button 1"));

        public string Button1Content
        {
            get { return (string)GetValue(Button1ContentProperty); }
            set { SetValue(Button1ContentProperty, value); }
        }

        public event Action ButtonClickEvent;

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hello World");
            ButtonClickEvent?.Invoke();
        }

        public static readonly DependencyProperty Button2ContentProperty = DependencyProperty.Register("Button2Content", typeof(string), typeof(OptionsPopup), new PropertyMetadata("Default Button 2"));

        public string Button2Content
        {
            get { return (string)GetValue(Button2ContentProperty); }
            set { SetValue(Button2ContentProperty, value); }
        }

        //End Button

    }
}
