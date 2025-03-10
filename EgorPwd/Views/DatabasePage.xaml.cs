using LibEgor32;
using LibEgor32.Crypto;
using LibEgor32.EgorModels;
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

namespace EgorPwd.Views
{
    /// <summary>
    /// Interaction logic for DatabasePage.xaml
    /// </summary>
    public partial class DatabasePage : Page
    {
        public DatabasePage()
        {
            InitializeComponent();
            if(GlobalObjects.Repository is null)
            {
                MessageBox.Show($"No database selected", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            this.DBname.Content = GlobalObjects.Repository.Name;
            this.keySlotCount.Content = GlobalObjects.Repository.KeySlot.Count;
            this.KeyDataCount.Content = GlobalObjects.Repository.KeyData.Count;
            foreach(var data in GlobalObjects.Repository.KeyData)
            {
                this.KeyDataGrid.Items.Add(data);
            }
            this.Focus();
        }

        private async void DeleteSelectedKey_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (KeyDataGrid.SelectedItem is null)
                    ShowWarn("No data is selected");
                else
                {
                    this.Container.Visibility = Visibility.Visible;
                    this.DataRemoveNotifier.Visibility = Visibility.Visible;
                    await Task.Delay(4000);
                    if(this.UndoRemove)
                    {
                        this.DataRemoveNotifier.Visibility = Visibility.Collapsed;
                        this.ShowWarn("Data removal canceled");
                        this.UndoRemove = false;
                        return;
                    }
                    this.Container.Visibility = Visibility.Collapsed;
                    this.DataRemoveNotifier.Visibility = Visibility.Collapsed;
                    if (KeyDataGrid.SelectedItem is EgorKeyData data)
                    {
                        GlobalObjects.Repository.RemoveData(data, GlobalObjects.OpenedKey);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Cannot delete selected data\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.KeyDataGrid.Items.Clear();
                foreach (var data in GlobalObjects.Repository.KeyData)
                {
                    this.KeyDataGrid.Items.Add(data);
                }
                this.KeyDataCount.Content = GlobalObjects.Repository.KeyData.Count;
            }
        }
        private async void ShowWarn(string message,int delay =  2000)
        {
            if (this.Container.Visibility == Visibility.Collapsed)
            {
                this.Container.Visibility = Visibility.Visible;
                this.WarnNotifierContent.Content = message;
                this.WarnNotifier.Visibility = Visibility.Visible;
                await Task.Delay(delay);
                this.WarnNotifier.Visibility = Visibility.Collapsed;
                this.Container.Visibility = Visibility.Collapsed;
            }
        }
        private async void ShowInfo(string message,int delay = 2000)
        {
            if (this.Container.Visibility == Visibility.Collapsed)
            {
                this.Container.Visibility = Visibility.Visible;
                this.InfoNotifierContent.Content = message;
                this.InfoNotifier.Visibility = Visibility.Visible;
                await Task.Delay(delay);
                this.InfoNotifier.Visibility = Visibility.Collapsed;
                this.Container.Visibility = Visibility.Collapsed;
            }
        }
        private void KeyDataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is DependencyObject dep)
            {
                var row = VisualTreeHelper.GetParent(dep);
                while (row != null && !(row is DataGridRow))
                {
                    row = VisualTreeHelper.GetParent(row);
                }
                if (row == null)
                {
                    KeyDataGrid.SelectedItem = null;
                }
            }
        }
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Container.Visibility = Visibility.Visible;
                this.AddNewDataPrompt.Visibility = Visibility.Visible;

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot delete selected data\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        bool UndoRemove = false;
        private void RemoveDataUndo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.DataRemoveNotifier.Visibility == Visibility.Visible)
            {
                this.UndoRemove = true;
                this.Container.Visibility = Visibility.Collapsed;
                this.DataRemoveNotifier.Visibility = Visibility.Collapsed;
            }

        }

        private void AddPromptDiscard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Container.Visibility = Visibility.Collapsed;
            this.AddNewDataPrompt.Visibility = Visibility.Collapsed;
            this.AddPromptName.Text = "";
            this.AddPromptPassword.Text = "";
        }

        private void AddPromptAccept_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (this.AddPromptName.Text.Length < 1)
                {
                    MessageBox.Show($"Cannot create data with empty name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (this.AddPromptPassword.Text.Length < 1 || this.AddPromptPassword.Text.Length > 32)
                {
                    MessageBox.Show($"Password should be less than 1 character", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                EgorKeyData data = new EgorKeyData();

                if(GlobalObjects.Repository.KeyData.Count > 0)
                    data.ID = GlobalObjects.Repository.KeyData[GlobalObjects.Repository.KeyData.Count - 1].ID + 1;
                else
                    data.ID = 0;
                data.Name = this.AddPromptName.Text;
                data.Password = this.AddPromptPassword.Text;

                GlobalObjects.Repository.AddNewData(data, GlobalObjects.OpenedKey);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cannot add data\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.AddPromptName.Text = "";
                this.AddPromptPassword.Text = "";
                this.AddNewDataPrompt.Visibility = Visibility.Collapsed;
                this.Container.Visibility = Visibility.Collapsed;
                this.KeyDataGrid.Items.Clear();
                this.KeyDataCount.Content = GlobalObjects.Repository.KeyData.Count;
                foreach (var data in GlobalObjects.Repository.KeyData)
                {
                    this.KeyDataGrid.Items.Add(data);
                }
            }
        }

        private void CopySelected_Click(object sender, RoutedEventArgs e)
        {
            if (KeyDataGrid.SelectedItem is null)
                ShowWarn("No data is selected");
            else
            {
                string password = (KeyDataGrid.SelectedItem as EgorKeyData).Password;
                Clipboard.SetText(password);
                ShowInfo("Data copied",1500);
            }
        }

        private void LockRepository_Click(object sender, RoutedEventArgs e)
        {
            this.KeyDataGrid.Items.Clear();
            GlobalObjects.EncryptedRepository = EgorEngine.LoadEgorFile(GlobalObjects.Repository.FilePath); //through Create new
            GlobalObjects.OpenedKey.CloseKey();
            GlobalObjects.OpenedKey = null;

            GlobalObjects.Repository.KeyData.Clear();
            GlobalObjects.Repository = null;

            var loaderPage = new LoaderPage();
            loaderPage.LoadDatabase("");

            GlobalObjects.MainWindow.container.Content = loaderPage;
        }

        private void RepositorySettings_Click(object sender, RoutedEventArgs e)
        {
            if(this.Container.Visibility == Visibility.Collapsed)
            {
                DBnameSettings.Content = GlobalObjects.Repository.Name;
                this.Container.Visibility = Visibility.Visible;
                this.SettingsPrompt.Visibility = Visibility.Visible;
                this.SettingsName.Text = GlobalObjects.Repository.Name;
                this.KeySlotSettingsGrid.Items.Clear();

                foreach (var k in GlobalObjects.Repository.KeySlot)
                {
                    this.KeySlotSettingsGrid.Items.Add(k);
                }
                this.keySlotCount.Content = GlobalObjects.Repository.KeySlot.Count;

                this.SettingsNewPassword.Password = "";
            }
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.L)
                LockRepository_Click(this, null);

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
                AddItem_Click(this, null);
        }

        private void RemoveKeyEntrySettings_Click(object sender, RoutedEventArgs e)
        {
            if(this.KeySlotSettingsGrid.SelectedItem is EgorKey key)
            {
                if(GlobalObjects.Repository.KeySlot.Count == 1)
                {
                    MessageBox.Show($"Key slot cannot be empty after removal", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    if (MessageBox.Show("Removing the key is not reversible and once its done it wont be undone with discard button are you sure to continue?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        try
                        {
                            if (key.SecuredKey is not null)
                            {
                                MessageBox.Show($"Cannot remove current key", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }
                            GlobalObjects.Repository.RemoveKeyFromKeySlot(key, GlobalObjects.OpenedKey);
                            this.KeySlotSettingsGrid.Items.Clear();

                            foreach (var k in GlobalObjects.Repository.KeySlot)
                            {
                                this.KeySlotSettingsGrid.Items.Add(k);
                            }
                            this.keySlotCount.Content = GlobalObjects.Repository.KeySlot.Count;
                            MessageBox.Show($"Key removed successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed during key removal\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        private void ApplySettings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(this.SettingsName.Text.Length > 32 || this .SettingsName.Text.Length == 0)
            {
                MessageBox.Show($"Database name cannot be empty or more than 32 characters long.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                try
                {
                    GlobalObjects.Repository.Name = this.SettingsName.Text;

                    if (this.SettingsNewPassword.Password.Length > 0)
                        this.SettingsNewPassword.Password = "";

                    EgorEngine.WriteEgorFile(GlobalObjects.Repository, GlobalObjects.OpenedKey, GlobalObjects.Repository.FilePath);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Failed during settings update\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    this.DBname.Content = GlobalObjects.Repository.Name;
                    this.SettingsName.Text = "";
                    this.SettingsNewPassword.Password = "";
                    this.SettingsPrompt.Visibility = Visibility.Collapsed;
                    this.Container.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void DiscardSettings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.SettingsName.Text = "";
            this.SettingsNewPassword.Password = "";
            this.SettingsPrompt.Visibility = Visibility.Collapsed;
            this.Container.Visibility = Visibility.Collapsed;
        }

        private void AddPasswordSettings_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(this.SettingsNewPassword.Password.Length < 1 || this.SettingsNewPassword.Password.Length > 32)
            {
                MessageBox.Show($"Password should be more than 1 character and less than 32", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (MessageBox.Show("Are you sure to add new password to key slot?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        byte[] bPass = Encoding.UTF8.GetBytes(this.SettingsNewPassword.Password);
                        byte[] bHash = HashUtil.ComputeHash(EgorVersion.V1, bPass);
                        
                        if(GlobalObjects.Repository.KeySlot.Any((EgorKey k) => { return k.KeyHash is not null && k.KeyHash.SequenceEqual(bHash); }))
                        {
                            MessageBox.Show($"Password already exist in key slot\nAborting....", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        GlobalObjects.Repository.AddNewKeyToKeySlot(EgorEngine.CreateKey(bPass,GlobalObjects.OpenedKey), GlobalObjects.OpenedKey);
                        this.KeySlotSettingsGrid.Items.Clear();

                        foreach (var k in GlobalObjects.Repository.KeySlot)
                        {
                            this.KeySlotSettingsGrid.Items.Add(k);
                        }
                        this.keySlotCount.Content = GlobalObjects.Repository.KeySlot.Count;

                        MessageBox.Show($"New key added to key slot", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show($"Failed during adding new key\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

        }
    }
}
