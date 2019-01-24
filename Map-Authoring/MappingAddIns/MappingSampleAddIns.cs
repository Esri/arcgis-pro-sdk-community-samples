//   Copyright 2019 Esri
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

namespace MappingSampleAddIns
{
    /// <summary>
    /// This sample provides buttons to change basemap layer and to add new layer 
    /// to the active map.
    /// </summary>
    /// <remarks>
    /// 1. In Visual Studio click the Build menu. Then select Build Solution.
    /// 1. Click Start button to open ArcGIS Pro.
    /// 1. ArcGIS Pro will open. 
    /// 1. Open a map view. 
    /// 1. Click on 'ADD-IN TAB' on the ribbon
    /// 1. Within this tab there are 2 buttons in 'Mapping' group
    /// 1. The button on the left sets the current map's basemap layer to ArcGIS Online's Street basemap 
    /// 1. The other button allows you to add a new layer just by using a path or url and if it happens to be a feature layer, it shows total number of features in that layer
    /// ![UI](Screenshots/Screen1.png)
    /// </remarks>
    internal class MappingSampleAddIns : Module
  {
    private static MappingSampleAddIns _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static MappingSampleAddIns Current
    {
      get
      {
        return _this ?? (_this = (MappingSampleAddIns)FrameworkApplication.FindModule("SampleAddIns_Module"));

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
