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
            this.NameInput.Text = name;
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
                if (PasswordInput.Password.Length < 1 || PasswordInput.Password.Length > 32)
                {
                    MessageBox.Show($"Password should be less than 1 character and more than 32", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string filePath = "";
                if(NameInput.Text != GlobalObjects.DefaultDbName)
                {
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
                }
                else
                {
                    filePath = GlobalObjects.DefaultDbName+ ".egor";
                    MessageBox.Show("This database will be used as default", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                byte[] bKey = Encoding.UTF8.GetBytes(PasswordInput.Password);
                EgorKey key = EgorEngine.CreateNewKey(bKey);
                key.OpenKey(bKey);

                EgorRepository repo = EgorEngine.CreateNewRepository(key, NameInput.Text, filePath);
                GlobalObjects.Repository = repo;

                //Clearing key and Password
                Array.Clear(bKey, 0, bKey.Length);
                PasswordInput.Password = "";
                GlobalObjects.OpenedKey = key;
                GlobalObjects.MainWindow.container.Content = new DatabasePage();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to create database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PasswordInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Button_Click(this, null);
                e.Handled = true;
            }
        }
    }
}
