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
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicElementSymbolPicker.TextTools
{
  internal class EllipseTextTool : LayoutTool
  {
    public EllipseTextTool()
    {
      SketchType = SketchGeometryType.Ellipse;
    }
    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }
    protected override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      if (ActiveElementContainer == null)
        Task.FromResult(true);

      if (Module1.SelectedSymbol == null) return Task.FromResult(true);

      return QueuedTask.Run(() =>
      {
        ICollection<Segment> segments = null;
        (geometry as Polygon).GetAllSegments(ref segments);
        if (segments.Count == 0) return false;
        var ellipticalArcSegment = segments.First() as EllipticArcSegment; 
        //If this cast fails, this is not an ellipse
        if (ellipticalArcSegment == null) return false;
        var ellipsePoly = LayoutElementFactory.Instance.CreateEllipseGraphicElement(this.ActiveElementContainer, ellipticalArcSegment);
        var ge = LayoutElementFactory.Instance.CreateEllipseParagraphGraphicElement
                               (this.ActiveElementContainer,
                                ellipticalArcSegment, "Text",
                                Module1.SelectedSymbol as CIMTextSymbol);
        //The new text graphic element has been created
        //We now switch to Pro's out of box "esri_layouts_inlineEditTool" 
        //This will allow inline editing of the text 
        //This tool will work on graphics on both map view and layouts
        FrameworkApplication.SetCurrentToolAsync("esri_layouts_inlineEditTool");
        return true;
      });
    }
  }
}
