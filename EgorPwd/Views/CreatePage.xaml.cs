using LibEgor32;
using LibEgor32.EgorModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

namespace EgorPwd.Views
{
    /// <summary>
    /// Interaction logic for CreatePage.xaml
    /// </summary>
    public partial class CreatePage : Page
    {
        public CreatePage(string name)
        {
            InitializeComponent();
            this.HidePassword_MouseLeftButtonDown(this, null);
            this.NameInput.Text = name;
        }
        private void ShowPassword_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.PasswordInput.IsPasswordInput = false;
            this.ShowPassword.Visibility = Visibility.Collapsed;
            this.HidePassword.Visibility = Visibility.Visible;
        }

        private void HidePassword_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.PasswordInput.IsPasswordInput = true;
            this.ShowPassword.Visibility = Visibility.Visible;
            this.HidePassword.Visibility = Visibility.Collapsed;
        }

        private void OpenRepo_Click(object sender, RoutedEventArgs e)
        {
            var loaderPage = new LoaderPage();
            loaderPage.LoadDatabase("");
            if(GlobalObjects.EncryptedRepository is not null)
                GlobalObjects.MainWindow.container.Content = loaderPage;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NameInput.Text.Length < 1)
                {
                    MessageBox.Show($"Cannot create database with empty name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (PasswordInput.Text.Length < 1 || PasswordInput.Text.Length > 32)
                {
                    MessageBox.Show($"Password should be less than 1 character and more than 32", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string filePath = "";
                SaveFileDialog sv = new SaveFileDialog();
                sv.Title = "Open your password repository";
                sv.DefaultExt = ".egor";
                sv.FileName = NameInput.Text.ToString();
                if (sv.ShowDialog() ?? false)
                {
                    filePath = sv.FileName;
                }
                else
                {
                    MessageBox.Show("Selecting database path halted by user", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                byte[] bKey = Encoding.UTF8.GetBytes(PasswordInput.Text);
                EgorKey key = EgorEngine.CreateNewKey(bKey);
                key.OpenKey(bKey);

                EgorRepository repo = EgorEngine.CreateNewRepository(key, NameInput.Text, filePath);
                GlobalObjects.Repository = repo;

                //Clearing key and Password
                Array.Clear(bKey, 0, bKey.Length);
                PasswordInput.Text = "";
                GlobalObjects.OpenedKey = key;
                GlobalObjects.MainWindow.container.Content = new DatabasePage();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to create database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
