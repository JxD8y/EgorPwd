using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace EgorPwd.Controls
{
    /// <summary>
    /// Interaction logic for LabelTextBox.xaml
    /// </summary>
    public partial class LabelTextBox : UserControl,INotifyPropertyChanged
    {
        public static readonly DependencyProperty IsReadonlyProperty =
       DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(LabelTextBox), new PropertyMetadata(false));
        public static readonly DependencyProperty HelperContentProperty =
       DependencyProperty.Register("HelperContent", typeof(object), typeof(LabelTextBox), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty TextProperty =
      DependencyProperty.Register("Text", typeof(string), typeof(LabelTextBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
         public static readonly DependencyProperty IsPasswordInputProperty =
      DependencyProperty.Register("IsPasswordInput", typeof(bool), typeof(LabelTextBox), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsPasswordInput {
            get { return (bool)GetValue(IsPasswordInputProperty);}
            set { SetValue(IsPasswordInputProperty, value); if (!value) { ShowText(); } else { HideText(); } }
        }
        public object HelperContent
        {
            get { return GetValue(HelperContentProperty); }
            set { SetValue(HelperContentProperty, value); }
        }
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadonlyProperty); }
            set { SetValue(IsReadonlyProperty, value); }
        }
        public string Text
        {
            get { return textbox.Text; }
            set { SetValue(TextProperty, value); textbox.Text = value; OnPropertyChanged(); }
        }
        public LabelTextBox()
        {
            InitializeComponent();
            this.textbox.DataContext = this;
        }

        bool textHidden = false;
        string _text = "";
        private void ShowText()
        {
            this.textbox.Text = _text;
            _text = "";
            textbox.CaretIndex = textbox.Text.Length;
            textHidden = false;
        }
        private void HideText()
        {
            int length = this.textbox.Text.Length;
            _text = this.textbox.Text;
            this.textbox.Text = "";
            string hidden = new string('*', _text.Length);
            this.textbox.Text = hidden;
            textHidden = true;
            textbox.CaretIndex = textbox.Text.Length;
        }
        private void textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (textHidden)
            {
                _text += e.Text;
                e.Handled = true;
                textbox.Text += "*";
                textbox.CaretIndex = textbox.Text.Length;
            }
        }

        private void textbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (textHidden)
            {
                if(e.Key == Key.Back && textbox.Text.Length > 0)
                {
                    _text = _text.Substring(0, _text.Length - 1);
                    textbox.Text = textbox.Text.Substring(0, _text.Length - 1);
                    textbox.CaretIndex = textbox.Text.Length;
                    e.Handled = true;
                }
            }
        }
    }
}
