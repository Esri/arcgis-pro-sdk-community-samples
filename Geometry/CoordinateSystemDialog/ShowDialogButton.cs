/*

   Copyright 2016 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
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
