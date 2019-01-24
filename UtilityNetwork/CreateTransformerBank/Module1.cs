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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System.Threading.Tasks;

namespace CreateTransformerBank
{
  /// <summary>
  /// This add-in demonstrates the creation of an electric distribution transformer bank.  It creates the bank feature, three transformer features (one per phase),
  /// three fuses, three arresters and the associations between them.  It demonstrates how to accomplish this within a single EditOperation.
  ///  
  /// Utility network SDK samples require a utility network service to run.  For the Create Transformer Bank sample, you will need to do the following: 
  /// * Configure a utility network database and service using the ArcGIS for Electric data model.  Instructions for setting up and configuring this data are located on the [ArcGIS for Electric website](http://solutions.arcgis.com/electric/help/electric-utility-network-configuration/). 
  /// * If you are not using the sample Naperville dataset, you should change the constants in the CreateTransformerBank.cs file to match your data model.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu.  Then select Build Solution.
  /// 1. Click Start button to open ArcGIS Pro.
  /// 1. ArcGIS Pro will open.
  /// 1. Open a map view that contains at least one Feature Layer whose source points to a Feature Class that participates in a utility network.
  /// 1. Select a feature layer or subtype group layer that participates in a utility network or a utility network layer
  /// 1. Click on the Add-in tab on the ribbon
  /// 1. Click on the Create Transformer Bank tool
  /// 1. Click on the map to create a transformer bank at that location
  /// ![UI](Screenshots/Screenshot1.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CreateTransformerBank_Module"));
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

    #endregion Overrides

  }
}
