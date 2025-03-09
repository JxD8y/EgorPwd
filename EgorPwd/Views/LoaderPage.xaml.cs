using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using LibEgor32;
using LibEgor32.Parser;
using LibEgor32.EgorModels;
using LibEgor32.Crypto;

namespace EgorPwd.Views
{
    /// <summary>
    /// Interaction logic for LoaderPage.xaml
    /// </summary>
    public partial class LoaderPage : Page
    {
        EgorEncryptedRepository? LoadedRepo = null;
        public LoaderPage()
        {
            InitializeComponent();
            this.HidePassword_MouseLeftButtonDown(this, null);
        }
        public LoaderPage(string name) : base() => this.LoadDatabase(name);
        private void LoadDatabase(string name)
        {
            try
            {
                if (name == "")
                {
                    this.OpenRepo_Click(this, null);
                    return;
                }
                if (name != "" && !File.Exists(name))
                {
                    GlobalObjects.MainWindow.container.Content = new CreatePage(name);
                    return;
                }
                if (name != "" && File.Exists(name))
                {
                    string path = name;
                    if (!name.EndsWith(".egor"))
                    {
                        path = System.IO.Path.Combine(name, ".egor");
                    }
                    EgorEncryptedRepository eRepo = EgorEngine.LoadEgorFile(path);
                    this.version.Content = eRepo.Version.ToString();
                    this.keySlotSize.Content = eRepo.EncryptedKeySlot.Count;
                    this.repoName.Content = eRepo.Name;
                    GlobalObjects.EncryptedRepository = eRepo;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to load database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.PasswordInput.Focus();
            Keyboard.Focus(this.PasswordInput);
        }

        private void CreateRepo_Click(object sender, RoutedEventArgs e)
        {
            GlobalObjects.MainWindow.container.Content = new CreatePage("");
        }

        private void OpenRepo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opn = new OpenFileDialog();
            opn.Multiselect = false;
            opn.Title = "Open your password repository";
            opn.DefaultExt = ".Egor";
            if (opn.ShowDialog() ?? false)
            {
                string repoPath = opn.FileName;
                this.LoadDatabase(repoPath);
            }
            else
            {
                MessageBox.Show("Opening new repository halted by user.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                this.LoadDatabase(GlobalObjects.DefaultDbName);
            }
        }

        private void DecryptDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GlobalObjects.EncryptedRepository is null)
                {
                    MessageBox.Show($"No database selected to decrypt.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    OpenRepo_Click(this, null);
                    return;
                }

                if (PasswordInput.Text.ToString().Length < 1 || PasswordInput.Text.ToString().Length > 32)
                {
                    MessageBox.Show($"Password should be more than 1 character and less than 32", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                byte[] bKey = Encoding.UTF8.GetBytes(PasswordInput.Text);

                var keys = GlobalObjects.EncryptedRepository.EncryptedKeySlot.Where((EgorKey k) => { return k.KeyHash is not null && HashUtil.ComputeHash(EgorVersion.V1, bKey).SequenceEqual(k.KeyHash); });

                if (keys.ToList().Count == 0)
                {
                    MessageBox.Show($"Wrong password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                EgorKey key = keys.First();

                key.OpenKey(bKey);

                GlobalObjects.Repository = EgorEngine.DecryptRepository(GlobalObjects.EncryptedRepository, key);
                GlobalObjects.MainWindow.container.Content = new DatabasePage();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Cannot decrypt database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void OpenDefRepo_Click(object sender, RoutedEventArgs e)
        {
            this.LoadDatabase(GlobalObjects.DefaultDbName);
        }
    }
}
