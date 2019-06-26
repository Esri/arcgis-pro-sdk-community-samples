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
using ArcGIS.Desktop.Framework.Controls;

namespace CustomCategoriesExample
{
    /// <summary>
    /// An example of implementing a custom category. In this case we declare
    /// a custom category **AcmeCustom_Reports** and a contract:
    /// **IAcmeCustomReport**
    /// </summary>
    /// <remarks>
    /// CustomCategoriesExample add-in declares the category AcmeCustom_Reports.
    /// It also provides a default component, **CustomCategoriesExample.Report.DefaultReport**
    /// that implements it via the **IAcmeCustomReport** contract. DefaultReport registers
    /// in the AcmeCustom_Reports category in the Config.daml.  
    ///   
    /// ExtraReport1 add-in also creates a component for the AcmeCustom_Reports category
    /// and likewise registers it in the category within ~its~ config.daml. 
    /// ExtraReport1.ExtraReport1 class implements the IAcmeCustomReport contract (as
    /// required by the category creator - CustomCategoriesExample add-in in this case).
    ///   
    /// When the CustomCategoriesExample Module is initialized, it reads its 
    /// AcmeCustom_Reports category via **Categories.GetComponentElements** and tests
    /// each one for the presence of the IAcmeCustomReport contract. Any component
    /// that registers in the category but does not implement the contract is skipped.
    /// The rest are instantiated and loaded into the ReportsWindow dialog for selection.
    /// ![UI](Screenshots/screen1.png)
    ///   
    /// Try making additional add-ins that implement the IAcmeCustomReport contract and
    /// register their component in the AcmeCustom_Reports category. Depending on the
    /// number of components loaded that implement the AcmeCustom_Reports category, the
    /// list of available reports in the ReportsWindow will increase or decrease
    /// respectively.
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("CustomCategoriesExample_Module"));
      }
    }

    protected override bool Initialize()
    {
      LoadComponents();

      return true;
    }

    internal void LoadComponents()
    {
      // Get all the report components registered in our category
      foreach (var component in Categories.GetComponentElements("AcmeCustom_Reports"))
      {
        try
        {
          //access the content element
          var content = component.GetContent();
          var version = content.Attribute("version").Value;

          //get the underlying report and test for the presence of
          //the contract
          var reportItem = component.CreateComponent() as IAcmeCustomReport;
          if (reportItem != null)
          {
            CustomReports.Add(reportItem);
          }
        }
        catch (Exception e)
        {
		      //TODO handle exception as needed
          string x = e.Message;
        }
      }
    }

    internal List<IAcmeCustomReport> CustomReports { get; } = new List<IAcmeCustomReport>();


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
