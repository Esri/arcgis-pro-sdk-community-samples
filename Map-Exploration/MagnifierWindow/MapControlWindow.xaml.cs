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
using System.Windows.Shapes;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Mapping.Controls;


namespace MagnifierWindow
{
    /// <summary>
    /// Interaction logic for MapControlWindow.xaml
    /// </summary>
    public partial class MapControlWindow : Window
    {
        internal MapControlWindow_ViewModel viewModel = null;
        internal static MapControl _mapControl = null;
        public MapControlWindow()
        {
            InitializeComponent();
            _mapControl = this.mapControl;
            viewModel = new MapControlWindow_ViewModel();
            viewModel.UpdateMapControlContent();
            DataContext = viewModel;
        }
    }
}
