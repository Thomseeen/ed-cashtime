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



namespace EdCashtime {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainView : Window, IDisposable {
        #region Private Fields
        private readonly MainViewModel viewModel;
        #endregion

        #region Constructor
        public MainView() {
            viewModel = new MainViewModel();

            DataContext = viewModel;
            InitializeComponent();
        }
        #endregion

        private void StartListening_Click(object sender, RoutedEventArgs e) {
            viewModel.StartListening();
        }

        private void StopListening_Click(object sender, RoutedEventArgs e) {
            viewModel.StopListening();
        }

        public void Dispose() {
            viewModel.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
