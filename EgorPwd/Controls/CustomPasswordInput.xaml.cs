using System;
using System.Collections.Generic;
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

namespace EgorPwd.Controls
{
    /// <summary>
    /// Interaction logic for CustomPasswordInput.xaml
    /// </summary>
    public partial class CustomPasswordInput : UserControl
    {
        private bool _isUpdating = false;
        public CustomPasswordInput()
        {
            InitializeComponent();
            DataObject.AddPastingHandler(HiddenPassword, OnPaste);
            DataObject.AddPastingHandler(textbox, OnPaste);
            HiddenPassword.PreviewTextInput += HiddenPasswordBox_PreviewTextInput;
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(CustomPasswordInput),
                new PropertyMetadata(string.Empty, OnPasswordChanged));

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CustomPasswordInput)d;
            if (!control._isUpdating)
            {
                control._isUpdating = true;
                string newPassword = e.NewValue as string ?? string.Empty;
                control.HiddenPassword.Password = newPassword;
                control.textbox.Text = newPassword;
                control._isUpdating = false;
            }
        }
        public string Password
        {
            get { return (string)GetValue(PasswordProperty); }
            set { SetValue(PasswordProperty, value); }
        }

        private void HiddenPasswordBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            PasswordControl.IsEnabled = true;
        }

        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            PasswordControl.IsEnabled = true;
            textbox.Visibility = Visibility.Collapsed;
        }

        private void HiddenPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isUpdating)
            {
                _isUpdating = true;
                Password = HiddenPassword.Password;
                _isUpdating = false;
            }
        }

        private void PasswordVisibility_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (textbox.Visibility == Visibility.Collapsed)
            {
                textbox.Visibility = Visibility.Visible;
                textbox.Text = Password;
                textbox.Focus();
                textbox.CaretIndex = textbox.Text.Length;
            }
            else
            {
                textbox.Visibility = Visibility.Collapsed;
                HiddenPassword.Password = Password;
                HiddenPassword.Focus();
            }
        }

        private void textbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            PasswordControl.IsEnabled = true;
        }

        private void HiddenPassword_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            PasswordControl.IsEnabled = true;
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUpdating)
            {
                _isUpdating = true;
                Password = textbox.Text;
                _isUpdating = false;
            }
        }
    }
}
