/*

   Copyright 2023 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       https://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TrayButtons
{
    internal class ColorPickerTrayButton : MapTrayButton
    {
        /// <summary>
        /// Invoked after construction, and after all DAML settings have been loaded. 
        /// Use this to perform initialization such as setting ButtonType.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // set the button type
            //  change for different button types
            ButtonType = TrayButtonType.PopupToggleButton;

            // ClickCommand is used for TrayButtonType.Button only
            ClickCommand = new RelayCommand(DoClick);
        }
        /// <summary>
        /// Override to perform some button initialization.  This is called the first time the botton is loaded.
        /// </summary>
        protected override void OnButtonLoaded()
        {
            base.OnButtonLoaded();
        }

        #region TrayButtonType.Button
        private void DoClick()
        {
            // do something when the tray button is clicked
            // refresh the popup VM checked state
            if ((_popupVM != null) && (_popupVM.IsChecked != this.IsChecked))
                _popupVM.IsChecked = this.IsChecked;
        }
        #endregion

        #region TrayButtonType.ToggleButton / TrayButtonType.PopupToggleButton
        // 
        // this method fires when ButtonType = TrayButtonType.ToggleButton or PopupToggleButton
        // 

        /// <summary>
        /// Called when the toggle button check state changes
        /// </summary>
        protected override void OnButtonChecked()
        {
            // get the checked state
            var isChecked = this.IsChecked;

            // do something with the checked state
            // refresh the popup VM checked state
            if ((_popupVM != null) && (_popupVM.IsChecked != this.IsChecked))
                _popupVM.IsChecked = this.IsChecked;
        }
        #endregion

        #region TrayButtonType.PopupToggleButton

        // These methods fire when ButtonType = TrayButtonType.PopupToggleButton

        private ColorPickerTrayButtonPopupViewModel _popupVM = null;

        /// <summary>
        /// Construct the popup view and return it. 
        /// </summary>
        /// <returns></returns>
        protected override ContentControl ConstructPopupContent()
        {
            // set up the tray button VM
            _popupVM = new ColorPickerTrayButtonPopupViewModel()
            {
                Heading = this.Name,
                IsChecked = this.IsChecked,
                CanPopupAutoClose = this.CanAutoClose
            };

            // return the UI with the datacontext set
            return new ColorPickerTrayButtonPopupView() { DataContext = _popupVM };
        }

        private bool _subscribed = false;

        /// <summary>
        /// Called when the popup is shown. 
        /// </summary>
        protected override void OnShowPopup()
        {
            base.OnShowPopup();
            // track property changes
            if (!_subscribed)
            {
                _popupVM.PropertyChanged += ColorPickerTrayButtonPopupViewModel_PropertyChanged;
                _subscribed = true;
            }
        }

        /// <summary>
        /// Called when the popup is hidden.
        /// </summary>
        protected override void OnHidePopup()
        {
            // cleanup
            if (_subscribed)
            {
                _popupVM.PropertyChanged -= ColorPickerTrayButtonPopupViewModel_PropertyChanged;
                _subscribed = false;
            }
            base.OnHidePopup();
        }

        private void ColorPickerTrayButtonPopupViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_popupVM == null)
                return;
            // make sure MapTrayButton class has correct checked state when it changes on the VM
            if (e.PropertyName == nameof(ColorPickerTrayButtonPopupViewModel.IsChecked))
            {
                // Since we are changing IsChecked in OnButtonChecked
                //We don't want property notification to trigger (another) callback to OnButtonChecked
                this.SetCheckedNoCallback(_popupVM.IsChecked);
            }
            //Control auto close behavior of tray button 
            else if (e.PropertyName == nameof(ColorPickerTrayButtonPopupViewModel.CanPopupAutoClose))
            {
                this.CanAutoClose = _popupVM.CanPopupAutoClose;
            }
        }

        // Provided to show you how to manually close the popup via code. 
        private void ManuallyClosePopup()
        {
            this.ClosePopup();
        }
        #endregion
    }
}
