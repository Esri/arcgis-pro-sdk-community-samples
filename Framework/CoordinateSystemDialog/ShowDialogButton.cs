using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using CoordinateSystemAddin.UI;

namespace CoordinateSystemAddin {
    internal class ShowDialogButton : Button {

        private CoordSysDialog _dlg = null;
        private static bool _isOpen = false;

        protected override void OnClick() {
            if (_isOpen)
                return;
            _isOpen = true;
            _dlg = new CoordSysDialog();
            _dlg.Closing += bld_Closing;
            _dlg.Owner = FrameworkApplication.Current.MainWindow;
            _dlg.Show();
        }

        void bld_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            if (_dlg.SpatialReference != null) {
                MessageBox.Show(string.Format("You picked {0}", _dlg.SpatialReference.Name), "Pick Coordinate System");
                //Do something with the selected spatial reference
                //....

            }
            _dlg = null;
            _isOpen = false;
        }
    }
}
