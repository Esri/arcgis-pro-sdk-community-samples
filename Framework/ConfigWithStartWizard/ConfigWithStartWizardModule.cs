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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace ConfigWithStartWizard
{
  /// <summary>
  /// “Managed Configurations” allow branding of ArcGIS Pro meaning you can customize the splash and startup screens, application icon, and modify the runtime ArcGIS Pro User Interface to best fit your user’s business needs.  This sample illustrates a configuration solution that shows various startup screen options in form of a wizard interface.  
  /// </summary>
  /// <remarks>
  /// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data).  The sample data contains a folder called 'C:\Data\Configurations\Projects' with sample data required for this solution.  Make sure that the Sample data is unzipped in c:\data and "C:\Data\Configurations\Projects" is available.
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. This solution is using the **Newtonsoft.Json NuGet**.  If needed, you can install the NuGet from the "NuGet Package Manager Console" by using this script: "Install-Package Newtonsoft.Json".
  /// 1. Click Start button to debug ArcGIS Pro.
  /// 1. ArcGIS Pro displays first the custom splash screen.
  /// 1. Pro will then display the startup screen utilizing a wizard style user interface.
  /// ![UI](Screenshots/Startup1.png)
  /// 1. The code for this startup user control can be found in OutofBoxStartPage.xaml and OutofBoxStartPageViewModel.cs.
  /// ![UI](Screenshots/Startup2.png)
  /// 1. The code for this startup user control can be found in StockStartPage.xaml and StockStartPageViewModel.cs.
  /// ![UI](Screenshots/Startup3.png)
  /// 1. The code for this startup user control can be found in OnlineItemStartPage.xaml and OnlineItemStartPageViewModel.cs.
  /// ![UI](Screenshots/Startup4.png)
  /// 1. The code for this startup user control can be found in OnlineItemStartPage.xaml and OnlineItemStartPageViewModel.cs.
  /// ![UI](Screenshots/Startup5.png)
  /// 1. The code for this startup user control can be found in OnlineItemStartPage.xaml and OnlineItemStartPageViewModel.cs.
  /// ![UI](Screenshots/Startup6.png)
  /// 1. The code for this startup user control can be found in OnlineItemStartPage.xaml and OnlineItemStartPageViewModel.cs.
  /// ![UI](Screenshots/Startup7.png)
  /// 1. The code for this startup user control can be found in CaliforniaStartPage.xaml and CaliforniaStartPageViewModel.cs.
  /// </remarks>
  internal class ConfigWithStartWizardModule : Module
    {
        private static ConfigWithStartWizardModule _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static ConfigWithStartWizardModule Current
        {
            get
            {
                return _this ?? (_this = (ConfigWithStartWizardModule)FrameworkApplication.FindModule("ConfigWithStartWizard_Module"));
            }
        }


        public static string UniqueFileName(string fileName)
        {
            if (!System.IO.File.Exists(fileName))
                return fileName;
            //var fileName = System.IO.Path.Combine(pathDownload, result.Name);
            var path = System.IO.Path.GetDirectoryName(fileName);
            var temp = System.IO.Path.GetFileNameWithoutExtension(fileName);
            var ext = System.IO.Path.GetExtension(fileName);

            int i = 1;
            while (System.IO.File.Exists(fileName))
            {
                fileName = System.IO.Path.Combine(path, $"{temp} ({i}){ext}");
                i++;
            }
            return fileName;
        }

        public static string DefaultFolder()
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments), @"ArcGIS\Projects");
        }

        public static string PakageFolder()
        {
            return System.IO.Path.Combine(System.Environment.GetFolderPath(
                          System.Environment.SpecialFolder.MyDocuments), @"ArcGIS\Packages");
        }

        public static string GetDefaultTemplateFolder()
        {
            string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string root = dir.Split(new string[] { @"\bin" }, StringSplitOptions.RemoveEmptyEntries)[0];
            return System.IO.Path.Combine(root, @"Resources\ProjectTemplates");
        }

        public static IReadOnlyList<string> GetDefaultTemplates()
        {
            string templatesDir = GetDefaultTemplateFolder();
            return
                Directory.GetFiles(templatesDir, "*", SearchOption.TopDirectoryOnly)
                    .Where(f => f.EndsWith(".ppkx") || f.EndsWith(".aptx")).ToList();
        }


        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}