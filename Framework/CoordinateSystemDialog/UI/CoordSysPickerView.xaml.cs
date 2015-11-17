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
using System.Windows.Threading;

namespace CoordinateSystemAddin.UI {
    /// <summary>
    /// Interaction logic for CoordinateSystemPicker.xaml
    /// </summary>
    /// <remarks>Handy reference on how to style the TreeView:
    /// <a href="http://blogs.msdn.com/b/mikehillberg/archive/2009/10/30/treeview-and-hierarchicaldatatemplate-step-by-step.aspx"/>
    /// </remarks>
    public partial class CoordSysPickerView : UserControl {
        public CoordSysPickerView() {
            InitializeComponent();
        }

        private void TreeViewItem_BringIntoView(object sender, RoutedEventArgs e) {
            TreeViewItem item = e.OriginalSource as TreeViewItem;

            var count = VisualTreeHelper.GetChildrenCount(item);
            if (0 < count) {
                for (int i = count - 1; i >= 0; --i) {
                    var childItem = VisualTreeHelper.GetChild(item, i);
                    ((FrameworkElement)childItem).BringIntoView();
                }
            }
            else
                item.BringIntoView();

            // Make sure item has focus
            if (Keyboard.FocusedElement != item) {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() => {
                    Keyboard.Focus(item);
                    item.Focus();
                }));
            }
        }

        private void CoordSystemTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            SelectedItemHelper.Content = e.NewValue;
        }
    }
}
