using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicElementSymbolPicker
{
  /// <summary>
  /// Represents each item displayed in the symbol gallery
  /// </summary>
  public class GeometrySymbolItem
  {
    public GeometrySymbolItem(SymbolStyleItem symbolStyleItem, string group)
    {
      Icon32 = symbolStyleItem.PreviewImage;
      Name = symbolStyleItem.Name;
      Group = group;
      if (symbolStyleItem != null)
        cimSymbol = symbolStyleItem.Symbol as CIMSymbol;

    }
    public object Icon32 { get; private set; }

    public string Name { get; private set; }

    public string Group { get; private set; }
    public CIMSymbol cimSymbol
    {
      get; private set;
    }
    internal void Execute()
    {
      if (cimSymbol != null)
        Module1.SelectedSymbol = cimSymbol;
    }
  }
}
