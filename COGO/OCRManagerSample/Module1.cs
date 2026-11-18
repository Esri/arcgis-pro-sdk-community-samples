/*

   Copyright 2026 Esri

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

using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Editing.COGO;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OCRManagerSample
{
  /// <summary>
  /// Uses optical character recognition to extract document text and traverse information in dockpanes and display recognized text with its tab-separated OCR output.
  /// </summary>
  /// <remarks>
  /// 1. In Visual Studio click the Build menu. Then select Build Solution.
  /// 1. Click Start button to debug ArcGIS Pro.
  /// 1. Open a project. Navigate to the Add-In tab on the ribbon. 
  /// ![UI](Screenshots/Screenshot1.png)
  /// 1. Open the DocReader dockpane.  Click the open button to choose a document to perform the OCR. 
  /// Documents that are supported are *.pdf;*.tif;*.tiff;*.jpg;*.jpeg;*.png;*.bmp.
  /// The document text will be displayed in the bottom half of the dockpane. 
  /// ![UI](Screenshots/Screenshot2.png)
  /// 1. Click Clear to remove the data or choose another document.
  /// 1. Open the TraverseReader dockpane.  Click the open button to choose a document to extract traverse information.
  /// Documents that are supported are*.pdf;*.tif;*.tiff;*.jpg;*.jpeg;*.png;*.bmp
  /// The number of traverses in the document will be shown along with a set of navigation buttons.
  /// The first traverse will be displayed in the bottom half of the dockpane in the ArcMap traverse file format.
  /// ![UI](Screenshots/Screenshot3.png)
  /// 1. Use the navigation buttons to move to the first, previous, next or last traverse.
  /// 1. Click "Add to Map" to add the courses of the current traverse to the map's overlay.  
  /// </remarks>
  internal class Module1 : Module
  {
    private static Module1 _this = null;

    /// <summary>
    /// Retrieve the singleton instance to this module here
    /// </summary>
    public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("OCRManagerSample_Module");

    public List<IDisposable> Overlays = new List<IDisposable>();

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

    internal void RemoveOverlays()
    {
      var disps = Overlays;
      foreach (var disp in disps)
      {
        disp.Dispose();
      }

      Overlays.Clear();
    }
  }
}
