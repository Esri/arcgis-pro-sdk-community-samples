//   Copyright 2018 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace DomainsUsage
{
    /// <summary>
    /// This addin will list all domains in the workspace corresponding to the selected layer and on selecting a domain
    /// a query layer will be generated which lists the datasets which reference the selected domains
    /// 
    /// NOTE: In order to list the domains, the owner of the GDB_ITEMS is used which is inferred using the owner of the default version
    ///       It is assumed that the default version will either be dbo.DEFAULT or sde.DEFAULT
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 2. Click Start button to open ArcGIS Pro.
    /// 3. ArcGIS Pro will open. 
    /// 4. Open a map view that contains at least one Feature Layer whose source points to a Feature Class from an Enterprise Geodatabase (SQL Server)
    /// 5. Make sure the Map Pane and the corresponding Table of Contents (TOC) Pane are Open and the Map Pane is Active
    /// 6. Click on the Add-In Tab
    /// 7. Observe that the Domains Combo Box is disabled until a SQL Server FeatureClass backed Feature Layer is selected in the TOC
    /// 8. On selecting a SQL Server FeatureClass backed layer, the combo box is enabled
    /// 9. The combobox lists all domains and selecting a domain will generate a Query Layer with the name "RelatedDomains"+ the domain name selected
    /// 10. The Query layer lists the datasets referencing the domain selected 
    /// </remarks>
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current
        {
            get
            {
                return _this ?? (_this = (Module1)FrameworkApplication.FindModule("DomainsUsage_Module"));
            }
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

        /// <summary>
        /// Generic implementation of ExecuteCommand to allow calls to
        /// <see cref="FrameworkApplication.ExecuteCommand"/> to execute commands in
        /// your Module.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected override Func<Task> ExecuteCommand(string id)
        {

            //TODO: replace generic implementation with custom logic
            //etc as needed for your Module
            var command = FrameworkApplication.GetPlugInWrapper(id) as ICommand;
            if (command == null)
                return () => Task.FromResult(0);
            if (!command.CanExecute(null))
                return () => Task.FromResult(0);

            return () =>
            {
                command.Execute(null); // if it is a tool, execute will set current tool
                return Task.FromResult(0);
            };
        }
        #endregion Overrides

    }
}
