/*

   Copyright 2019 Esri

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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace ConfigWithStartWizard.UI {

    class SignOnStatusViewModel : PropertyChangedBase {

        private ImageSource _greenUserImageSource = null;
        private ImageSource _redUserImageSource = null;
        private bool _isSignedOn = false;
        private string _activePortalURL = "";
        private string _userName = "";

        public SignOnStatusViewModel() {
            _greenUserImageSource = new BitmapImage(new Uri("pack://application:,,,/ConfigWithStartWizard;component/Images/User32.png"));
            _redUserImageSource = new BitmapImage(new Uri("pack://application:,,,/ConfigWithStartWizard;component/Images/UserRed32.png"));
            //block
            var ok = QueuedTask.Run(() => {
                var portal = ArcGISPortalManager.Current.GetActivePortal();
                if (portal != null) {
                    _activePortalURL = portal.PortalUri.ToString();
                    _isSignedOn = portal.IsSignedOn();
                    if (_isSignedOn)
                        _userName = portal.GetSignOnUsername();
                }
                return true;
            }).Result;
        }

        public ImageSource UserImage
        {
            get
            {
                return _isSignedOn ? _greenUserImageSource : _redUserImageSource;
            }
        }

        public ImageSource GreenImage => _greenUserImageSource;

        public ImageSource RedImage => _redUserImageSource;

        public string ActivePortalURL => _activePortalURL;

        public string UserName
        {
            get
            {
                return _userName;
            }
            set
            {
                _userName = value;
                NotifyPropertyChanged(() => UserName);
            }
        }

        public bool IsSignedOn
        {
            get
            {
                return _isSignedOn;
            }
            set
            {
                _isSignedOn = value;
                NotifyPropertyChanged(() => IsSignedOn);
            }
        }

        public void Signin() {
            QueuedTask.Run(() => {
                var portal = ArcGISPortalManager.Current.GetActivePortal();
                if (portal != null) {
                    portal.SignIn();
                    IsSignedOn = portal.IsSignedOn();
                    if (IsSignedOn)
                        UserName = portal.GetSignOnUsername();
                }
                else {
                    //arcgis online should always be there by default
                    var online = ArcGISPortalManager.Current.GetPortal(new Uri("http://www.arcgis.com", UriKind.Absolute));
                    online.SignIn();
                    ArcGISPortalManager.Current.SetActivePortal(online);
                    IsSignedOn = online.IsSignedOn();
                    if (IsSignedOn)
                        UserName = online.GetSignOnUsername();
                }
            });
        }

        public void Signout() {
            QueuedTask.Run(() => {
                var portal = ArcGISPortalManager.Current.GetActivePortal();
                if (portal != null) {
                    portal.SignOut();
                }
                IsSignedOn = false;
            });
        }
    }
}
