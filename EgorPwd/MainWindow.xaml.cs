using EgorPwd.Views;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace EgorPwd;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        GlobalObjects.MainWindow = this;

        if (!File.Exists(GlobalObjects.DefaultDbName + ".egor"))
            this.container.Content = new CreatePage(GlobalObjects.DefaultDbName);
        else
        {
            var LoaderPage = new LoaderPage();
            LoaderPage.LoadDatabase(GlobalObjects.DefaultDbName + ".egor");
            this.container.Content = LoaderPage;
        }
    }
}