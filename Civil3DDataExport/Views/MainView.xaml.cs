using Civil3DDataExport.ViewModels;
using System.Windows;

namespace Civil3DDataExport.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView(Models.Log log)
        {
            InitializeComponent();
            MainViewViewModel mainViewViewModel = new MainViewViewModel(log);
            this.DataContext = mainViewViewModel;
        }
    }
}
