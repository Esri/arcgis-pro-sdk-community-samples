/*

   Copyright 2017 Esri

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
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.ComponentModel;
using ConfigWithStartWizard.UI;
using ConfigWithStartWizard.UI.StartPages;


namespace ConfigWithStartWizard
{
    internal class ConfigurationManagerStartWizard : ConfigurationManager
    {
        /// <summary>
        /// Replaces the default ArcGIS Pro application name
        /// </summary>
        protected override string ApplicationName
        {
            get { return "ConfigWithStartWizard"; }
        }

        /// <summary>
        /// Replaces the ArcGIS Pro Main window icon.
        /// </summary>
        protected override ImageSource Icon
        {
            get
            {
                return new BitmapImage(new Uri(@"pack://application:,,,/ConfigWithStartWizard;component/Images/favicon.ico"));
            }
        }

        #region Override Startup Page

        private StartPageViewModel _vm;
        /// <summary>
        /// Called before ArcGIS Pro starts up. Replaces the default Pro start-up page (Optional)
        /// </summary>
        /// <returns> Implemented UserControl with start-up page functionality. 
        /// Return null if a custom start-up page is not needed. Default ArcGIS Pro start-up page will be displayed.</returns>
        protected override System.Windows.FrameworkElement OnShowStartPage()
        {
            if (_vm == null)
            {
                _vm = new StartPageViewModel();
            }
            var page = new StartPage();
            page.DataContext = _vm;
            return page;
        }

        ///<summary>
        ///During the start up this method is called after it is safe to access Portal and use ArcGIS.Desktop.Core. 
        ///ArcGIS Pro Theme has already been set. 
        ///</summary>
        ///<param name="cancelEventArgs">
        ///To cancel initialization, set the cancelEventArgs.Cancel property to true.
        ///</param>
        protected override void OnApplicationInitializing(CancelEventArgs cancelEventArgs)
        {

        }

        ///<summary>
        ///During the start up this method is called after the Application Window Start page is ready. From here on calls to ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask are safe.
        ///ArcGIS Pro Extension modules can now be accessed. 
        ///</summary>
        protected override void OnApplicationReady()
        {

        }

        #endregion

        #region Override Splash screen
        /// <summary>
        /// Called while ArcGIS Pro starts up. Replaces the default Pro splash screen. (Optional)
        /// </summary>
        /// <returns>Implemented Window with splash screen functionality. 
        /// Return null if a custom splash screen is not needed. Default ArcGIS Pro splash screen will be displayed.</returns>
        protected override System.Windows.Window OnShowSplashScreen()
        {
            return new SplashScreen();
        }
        #endregion

        #region Override About page
        /// <summary>
        /// Customized UserControl is displayed in ArcGIS Pro About property page. Allows to add information about this specific managed configuration.
        /// </summary>
        /// <returns>Implemented UserControl with about box information. 
        /// Return null if a custom about box is not needed. Default ArcGIS Pro About box will be displayed.</returns>
        protected override System.Windows.FrameworkElement OnShowAboutPage()
        {
            return new AboutPage();
        }
        #endregion
    }
}
