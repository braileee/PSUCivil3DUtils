using Civil3DAlignmentStationCoordinatesTable.ViewModels;
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

namespace Civil3DAlignmentStationCoordinatesTable.Views
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
