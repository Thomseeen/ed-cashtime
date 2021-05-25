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



namespace ed_cashtime {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window {
        #region Private Fields
        private MainViewModel _viewModel;
        #endregion

        #region Constructor
        public MainView() {
            _viewModel = new MainViewModel();

            DataContext = _viewModel;
            InitializeComponent();
        }
        #endregion

        private async void StartListening_Click(object sender, RoutedEventArgs e) {
            await _viewModel.StartListeningAsync();
        }

        private void StopListening_Click(object sender, RoutedEventArgs e) {
            _viewModel.StopListening();
        }
    }
}
