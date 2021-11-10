using ArcGIS.Desktop.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace GraphicElementSymbolPicker
{
  /// <summary>
  /// Represents the tool item in the gallery of tools
  /// </summary>
  public class ElementCreationToolGalleryItem
  {
    internal ElementCreationToolGalleryItem(string id, string group, IPlugInWrapper plugin, string name)
    {
      CommandID = id;
      PlugInWrapper = plugin;
      if (PlugInWrapper.LargeImage is ImageSource)
        Icon32 = (ImageSource)PlugInWrapper.LargeImage;
      else
        Icon32 = PlugInWrapper.LargeImage;
      Group = group;
      Name = name;
      switch (Group)
      {
        case "Point Tools":
          ToolShapeType = Module1.ToolType.Point;
          break;
        case "Line Tools":
          ToolShapeType = Module1.ToolType.Line;
          break;
        case "Polygon Tools":
          ToolShapeType = Module1.ToolType.Polygon;
          break;
        case "Text Tools":
          ToolShapeType = Module1.ToolType.Text;
          break;
      }
    }
    public IPlugInWrapper PlugInWrapper { get; private set; }
    public object Icon32 { get; private set; }

    public string CommandID { get; private set; }

    public string Group { get; private set; }

    public string Name { get; private set; }
    internal Module1.ToolType ToolShapeType { get; set; }
    internal void Execute()
    {
      System.Diagnostics.Debug.WriteLine("Tool Execute");
      if (PlugInWrapper.IsRelevant)
      {
        System.Diagnostics.Debug.WriteLine($"PlugInWrapper.IsRelevant: {PlugInWrapper.IsRelevant}");
        ((ICommand)PlugInWrapper).Execute(null);
      }
    }
  }
}
