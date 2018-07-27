/*

   Copyright 2018 Esri

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
using System.Windows;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using BingStreetside.Utility;

namespace BingStreetside
{
    internal sealed class BingStreetsideViewModel : DockPane
    {
        private const string DockPaneId = "BingStreetside_BingStreetside";

        private BingStreetsideViewModel()
        {
            NotifyPropertyChanged(() => BingKey);
            SetBingMapKey = new RelayCommand(() =>
            {
                if (string.IsNullOrEmpty(BingKey))
                {
                    MessageBox.Show("Please enter your Bing Map Developer Key which you can get from: ");
                    return;
                }
                WebPage = Properties.Resources.BingStreetsidePage.Replace(@"Your Bing Maps Key", BingKey);
                BingKeyVisibile = Visibility.Collapsed;
            }, () => !string.IsNullOrEmpty(BingKey));
        }

        public ICommand SetBingMapKey { get; }

        /// <summary>
        /// Show the DockPane.
        /// </summary>
        internal static void Show()
        {
            var pane = FrameworkApplication.DockPaneManager.Find(DockPaneId);
            pane?.Activate();
        }

        private string _webPage = @"<html><body>Map Not Loaded</body></html>";
        public string WebPage
        {
            get
            {
                return _webPage;
            }
            set
            {
                SetProperty(ref _webPage, value, () => WebPage);
            }
        }


        /// <summary>
        /// TODO: define your bing map key here:
        /// </summary>
        private string _bingKey = @"";
        public string BingKey
        {
            get
            {
                return _bingKey;
            }
            set
            {
                SetProperty(ref _bingKey, value, () => BingKey);                
            }
        }

        private Visibility _bingKeyVisibile = Visibility.Visible;

        public Visibility BingKeyVisibile
        {
            get { return _bingKeyVisibile; }
            set
            {
                SetProperty(ref _bingKeyVisibile, value, () => BingKeyVisibile);
                switch (value)
                {
                    case Visibility.Collapsed:
                        BingKeyInvisible = Visibility.Visible;
                        break;
                    case Visibility.Visible:
                        BingKeyInvisible = Visibility.Collapsed;
                        break;
                }
            }
        }


        private Visibility _bingKeyInvisible = Visibility.Collapsed;

        public Visibility BingKeyInvisible
        {
            get { return _bingKeyInvisible; }
            set
            {
                SetProperty(ref _bingKeyInvisible, value, () => BingKeyInvisible);
            }
        }
        #region Adjust Heading

        /// <summary>
        /// Property bound to by the heading slider Value property.
        /// </summary>
        private int _headingValue = 0;
        public double HeadingValue
        {
            get { return Convert.ToDouble(_headingValue); }
            set
            {
                var cameraHeading = Convert.ToInt32(value > 180 ? value - 360 : value);
                if (cameraHeading == _headingValue) return;
                _headingValue = Convert.ToInt32(value);
                WebBrowserUtility.InvokeScript("setHeadingFromWPF", new object[] { cameraHeading });
            }
        }

        #endregion Adjust Heading
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class BingStreetside_ShowButton : Button
    {
        protected override void OnClick()
        {
            BingStreetsideViewModel.Show();
        }
    }
}
