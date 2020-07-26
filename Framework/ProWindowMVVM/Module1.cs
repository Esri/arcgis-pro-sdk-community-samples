/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Mapping;

namespace ProWindowMVVM
{
  /// <summary>
  /// the ProWindowMVVM sample demonstrates how to implement a ProWindow using the Model-View-ViewModel pattern (MVVM).  
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to debug ArcGIS Pro.
  /// 1. In ArcGIS Pro open any project.
  /// 1. Open the Add-in Tab and click on the 'Test ProWindow Dialog' button.
  /// 1. The Modal ProWindowDialog opens.  See TestProWindowDialog:OnClick on how the ProWindowDialog is instantiated and initialized.  
  /// 1. Find 'TODO: set data context' in the source code, this is where the ProWindowDialog datacontext property is set to ProWindowDialogVM. If you add a new ProWindow via the Pro SDK item templates you have to add this code snippet yourself.  
  /// ![UI](Screenshots/Screenshot1.png)  
  /// 1. Enter some input data.
  /// ![UI](Screenshots/Screenshot2.png)  
  /// 1. In MVVM you need to control the window closing from the ViewModel class.  This is accomplished by the addition of 'CommandParameter="{Binding ElementName=ProWindowDialogWin}"' to each button that closes the ProWindow.  
  /// 1. In the code behind you can then use the command parameter to control the DialogResult and Close () of the ProWindow with the following snippets: (proWindow as ProWindow).DialogResult = true; and (proWindow as ProWindow).Close();
  /// 1. Click either 'Ok' or 'Cancel' you get the appropriate DialogResult returned by the ShowDialog method:
  /// ![UI](Screenshots/Screenshot3.png)  
  /// 1. Finally you can use the dialog's data changes which have been stored in the Module class.
  /// ![UI](Screenshots/Screenshot4.png)    
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("ProWindowMVVM_Module"));
      }
    }

    #region ProWindowDialog Properties

    public static string ConnectionString { get; set; }
    public static string UserName { get; set; }
    public static string Password { get; set; }
    public static bool HasCredentials => !(string.IsNullOrEmpty(UserName) || string.IsNullOrEmpty(Password));

    #endregion ProWindowDialog Properties

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
