//   Copyright 2019 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       https://www.apache.org/licenses/LICENSE-2.0

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

namespace AnnoTools
{
	/// <summary>
	/// This sample illustrates construction tools and modify tools for annotation feature classes.  The modify tools show how to modify the text, geometry and symbol of 
	/// annotation features.  There are also some tools illustrating how to add callouts and leader lines to annotation features. 
	/// Annotation has the following special considerations
	/// 1.  Annotation feature classes store polygon geometry.  This polygon is the bounding box of the text of an annotation feature. The 
	/// bounding box is calculated from the text string, font, font size, angle orientation and other text formatting attributes of the feature. 
	/// It is automatically updated by the application each time the annotation attributes are modified. You should never need to access or 
	/// modify an annotation feature's polygon shape.
	/// 2. The text attributes of an annotation feature are represented as a CIMTextGraphic. The CIMTextGraphic 
	/// contains the text string, text formatting attributes (such as alignment, angle, font, color, etc), and other information (such as callouts 
	/// and leader lines). It also has a shape which represents the baseline geometry that the annotation text string sits upon. For annotation 
	/// features this CIMTextGraphic shape can be a point, polyline (typically a two point line or Bezier curve), multipoint or geometryBag. It is 
	/// this shape that you will typically interact with when developing annotation tools. For example when creating annotation features, the
	/// geometry passed to the EditOperation.Create method is the CIMTextGraphic geometry.
	/// 3.  In ArcGIS Pro, the only fields guaranteed to exist in an annotation schema are AnnotationClassID, SymbolID, Element, FeatureID, 
	/// ZOrder and Status along with the system ObjectID and Shape fields. All other fields which store text formatting attributes (such as 
	/// TextString, FontName, VerticalAlignment, HorizontalAlignment etc) are not guaranteed to exist in the physical schema. This is different 
	/// from the annotation schema in ArcGIS 10x where all fields existed and were unable to be deleted. In ArcGIS Pro, these text formatting 
	/// fields are able to be deleted by the user if they exist; they are no longer designated as protected or system fields. If you are writing 
	/// or porting tools that create or modify annotation features, it is essential to take this important concept into account.
	/// 4. Construction tools - Set the categoryRefID in the daml file to be "esri_editing_construction_annotation".  Also note that the geometry
	/// being passed to the EditOperation.Create method is the CIMTextGraphic geometry.
	/// 5. Editing tools - Use the GetAnnotationProperties and SetAnnotationPropertes methods on the Inspector object to modify the text formatting
	/// attributes.  (see AnnoModifySymbol.cs).  Any custom attributes in your schema can continue to be referenced via the inspector[fieldName] methodology.  
	/// </summary>
	/// <remarks>
	/// 1. Download the Community Sample data (see under the 'Resources' section for downloading sample data)
	/// 1. Make sure that the Sample data is unzipped in c:\data 
	/// 1. The project used for this sample is 'C:\Data\SampleAnno\SampleAnno.aprx'
	/// 1. In Visual studio click the Build menu. Then select Build Solution.
	/// 1. Start the Debugger to open ArcGIS Pro.
	/// 1. ArcGIS Pro will open, select the SampleAnno.aprx project
	/// 1. Activate an annotation template and see the two additional construction tools - Simple Anno Tool (Template) and Advanced Anno Tool.
	/// ![UI](Screenshots/Screen1.png)
	/// 1. Select the Simple Anno Tool and digitize a point.  An annotation feature will be created. 
	/// ![UI](Screenshots/Screen2.png)
	/// 1. Select the Advanced Anno Tool and digitize a line.  An annotation feature will be created. 
	/// ![UI](Screenshots/Screen3.png)
	/// 1. Click the 'Modify Anno Geometry' tool on the Add-In tab and drag a rectangle around the annotation features created by the advanced tool. The geometry of these features will be rotated 90 degrees.
	/// 1. Click the 'Modify Anno Symbol' tool on the Add-In tab and drag a rectangle around some annotation features. The text and symbol color of these features will change to 'Hello World' in red.
	/// ![UI](Screenshots/Screen4.png)
	/// 1. Click the 'Balloon Callout' tool on the Add-In tab and drag a rectangle around some annotation features. The text will change to a 'Balloon Callout'.
	/// ![UI](Screenshots/Screen5.png)
	/// 1. Click the 'Simple Line Callout' tool on the Add-In tab and drag a rectangle around some annotation features. The text will change to a 'Line Callout'.
	/// ![UI](Screenshots/Screen6.png)
	/// 1. Click the 'New Anno Template' button on the Add-In tab. Notice the new template created in the Create Feature pane.
  /// 1. Click the 'New Anno Template from Existing Tempate' button on the Add-In tab. Notice the new template created in the Create Feature pane.
	/// ![UI](Screenshots/Screen7.png)
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
        return _this ?? (_this = (Module1)FrameworkApplication.FindModule("AnnoTools_Module"));
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
