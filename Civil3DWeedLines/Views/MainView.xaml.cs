using Civil3DWeedLines.ViewModels;
using Prism.Events;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Civil3DWeedLines.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : Window
    {
        private readonly IEventAggregator _eventAggregator;

        public MainView(MainViewViewModel mainViewViewModel)
        {
            InitializeComponent();
            mainViewViewModel.OnRequestClose += (s, e) => this.Close();
            DataContext = mainViewViewModel;
        }

        private void shapeCheckbox_PreviewMouseDown(object sender, MouseButtonEventArgs eventArgs)
        {
            CheckBox checkbox = sender as CheckBox;

            if (checkbox != null)
            {
                checkbox.IsChecked = !checkbox.IsChecked;
                eventArgs.Handled = true;
            }
        }
    }
}
