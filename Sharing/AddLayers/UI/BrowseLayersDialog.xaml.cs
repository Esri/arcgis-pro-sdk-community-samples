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

namespace AddLayers.UI {
    /// <summary>
    /// Interaction logic for BrowseLayersDialog.xaml
    /// </summary>
    public partial class BrowseLayersDialog : Window {

        private BrowseLayersViewModel _vm = null;

        public BrowseLayersDialog() {
            _vm = new BrowseLayersViewModel();
            this.DataContext = _vm;
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            _vm.AddLayerToMap(e.Uri.ToString());
            e.Handled = true;
        }

        private void Close_OnClick(object sender, RoutedEventArgs e) {
            Close();
        }
    }
}
