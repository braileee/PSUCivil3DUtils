using Civil3DPropertyValuesReport.ViewModels;
using System.Windows;

namespace Civil3DPropertyValuesReport.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView(MainViewViewModel mainViewViewModel)
        {
            InitializeComponent();
            mainViewViewModel.OnRequestClose += (s, e) => this.Close();
            DataContext = mainViewViewModel;
        }
    }
}
