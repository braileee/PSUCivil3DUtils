using Civil3DBatchExtractSolids.ViewModels;
using Prism.Events;
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

namespace Civil3DBatchExtractSolids.Views
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

        private void shapeCheckbox_PreviewMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (sender is CheckBox checkbox)
            {
                checkbox.IsChecked = !checkbox.IsChecked;
                eventArgs.Handled = true;
            }
        }
    }
}
