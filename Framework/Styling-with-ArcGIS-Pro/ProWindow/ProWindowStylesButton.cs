using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace ControlStyles.ProWindow
{
    internal class ProWindowStylesButton : Button
    {
        private WindowStyling _dlg = null;
        private static bool _isOpen = false;
        protected override void OnClick()
        {
            if (_isOpen)
                return;
            _isOpen = true;
            _dlg = new WindowStyling();
            _dlg.Closing += bld_Closing;
            _dlg.Owner = FrameworkApplication.Current.MainWindow;
            _dlg.Show();

        }

        void bld_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _dlg = null;
            _isOpen = false;
        }
    }
}
