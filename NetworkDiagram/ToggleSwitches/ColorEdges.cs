/*

   Copyright 2022 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.NetworkDiagrams;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using static ToggleSwitches.CommonTools;

namespace ToggleSwitches
{
  /// <summary>
  /// This class color the diagram edge according to their subnetwork name
  /// </summary>
  internal class ColorEdges : Button
  {
    internal static Dictionary<string, Dictionary<string, CIMSymbol>> TheSymbols = new Dictionary<string, Dictionary<string, CIMSymbol>>();

    /// <summary>
    /// Run the coloration
    /// </summary>
    protected override void OnClick()
    {
      if (MapView.Active != null)
      {
        ExecuteReductionEdgeColorBySubnetwork(GetDiagramLayerFromMap(MapView.Active.Map));
      }
    }

    /// <summary>
    /// Search the qualified field name in the field list
    /// </summary>
    /// <param name="fields">Fields list</param>
    /// <param name="name"></param>
    /// <returns>Qualified field name</returns>
    internal static string GetFieldName(IReadOnlyList<Field> fields, string name)
    {
      string fieldName = name;
      foreach (Field field in fields)
      {
        string upperFieldName = field.Name.ToUpper();
        if (upperFieldName.Contains(fieldName))
          fieldName = field.Name;
      }

      return fieldName;
    }

    /// <summary>
    /// Get a GlogalID list per subnework name
    /// </summary>
    /// <param name="diagram">Network Diagram</param>
    /// <returns>Dictionary(Subnetwork name, GlobalID list)</returns>
    internal static Dictionary<string, List<string>> GetSubnetworkNameToGlobalIDs(NetworkDiagram diagram)
    {
      string subnetworkNameLabel;
      const string subnetworkNameLabelFeature = "Subnetwork name";
      const string subnetworkNameLabelAssembly = "Supported subnetwork name";
      const string unknwonLabel = "Unknown";

      string useSubnetworkNameLabelAssembly = GetSchemaVersion(diagram) <= 3 ? subnetworkNameLabelFeature : subnetworkNameLabelAssembly;

      Dictionary<int, string> jctIDToSubnetworkName = new Dictionary<int, string>();
      Dictionary<string, List<string>> subnetworkNameToGlobalIDs = new Dictionary<string, List<string>>();
      List<string> assemblySources = new List<string>();
      JObject content = JObject.Parse(diagram.GetContent(false, false, true, false));

      JObject mapping = content["sourceMapping"] as JObject;
      foreach (var item in mapping)
      {
        if (item.Key.Contains("Assembly"))
          assemblySources.Add((string)item.Value);
      }

      JToken junctions = content["junctions"];
      foreach (var item in junctions)
      {
        string source = item["assocSourceID"].ToString();
        if (assemblySources.Contains(source))
          subnetworkNameLabel = useSubnetworkNameLabelAssembly;
        else
          subnetworkNameLabel = subnetworkNameLabelFeature;

        if (item["attributes"] is JObject attributes && attributes.ContainsKey(subnetworkNameLabel))
          jctIDToSubnetworkName.Add((int)item["id"], attributes[subnetworkNameLabel].ToString());
        else
          jctIDToSubnetworkName.Add((int)item["id"], "");
      }

      bool needToIterate = true;
      int loopCount = 0;
      List<string> updatedGlobalIDs = new List<string>();

      JToken edges = content["edges"];
      while (needToIterate && loopCount < 10)
      {
        needToIterate = false;
        loopCount++;

        foreach (var item in edges)
        {
          string source = item["assocSourceID"].ToString();
          if (assemblySources.Contains(source))
            subnetworkNameLabel = useSubnetworkNameLabelAssembly;
          else
            subnetworkNameLabel = subnetworkNameLabelFeature;

          string currentGlobalId = item["assocGlobalID"].ToString();
          if (updatedGlobalIDs.Contains(currentGlobalId))
            continue;

          string subnetworkName = "";

          JObject attributes = item["attributes"] as JObject;
          if (attributes.ContainsKey(subnetworkNameLabel))
            subnetworkName = attributes[subnetworkNameLabel].ToString();
          else
          {
            int fromID = (int)item["fromID"];
            int toID = (int)item["toID"];
            jctIDToSubnetworkName.TryGetValue(fromID, out string fromSubnetworkName);
            jctIDToSubnetworkName.TryGetValue(toID, out string toSubnetworkName);

            if (string.IsNullOrEmpty(fromSubnetworkName) && string.IsNullOrEmpty(toSubnetworkName))
              needToIterate = true;
            else
            {
              if (!string.IsNullOrEmpty(fromSubnetworkName) && fromSubnetworkName == unknwonLabel)
                subnetworkName = fromSubnetworkName;
              else if (!string.IsNullOrEmpty(toSubnetworkName) && toSubnetworkName == unknwonLabel)
                subnetworkName = toSubnetworkName;

              if (string.IsNullOrEmpty(subnetworkName) && !string.IsNullOrEmpty(fromSubnetworkName) && !fromSubnetworkName.Contains(':'))
            {
              subnetworkName = fromSubnetworkName;
                if (string.IsNullOrEmpty(toSubnetworkName))
                  jctIDToSubnetworkName[toID] = subnetworkName;
            }

              if (string.IsNullOrEmpty(subnetworkName) && !string.IsNullOrEmpty(toSubnetworkName) && !toSubnetworkName.Contains(':'))
            {
              subnetworkName = toSubnetworkName;
                if (string.IsNullOrEmpty(fromSubnetworkName))
                  jctIDToSubnetworkName[fromID] = subnetworkName;
              }
            }
          }

          if (string.IsNullOrEmpty(subnetworkName))
            continue;

          updatedGlobalIDs.Add(currentGlobalId);

          if (!subnetworkNameToGlobalIDs.TryGetValue(subnetworkName, out List<string> globalIDs))
          {
            globalIDs = new List<string>();
            subnetworkNameToGlobalIDs.Add(subnetworkName, globalIDs);
          }

          if (!globalIDs.Contains(currentGlobalId))
            globalIDs.Add(currentGlobalId);
        }
      }

      return subnetworkNameToGlobalIDs;
    }

    /// <summary>
    /// Modify the layer renderer according to the subnetwork name
    /// </summary>
    /// <param name="LayerFeature">Layer to treat</param>
    /// <param name="SubnetworkNameToGlobalIDs">GlobalID List</param>
    /// <param name="SubnetColor">Dictionary( Subnetwork name, symbol)</param>
    internal static void SetEdgeLayerRenderer(FeatureLayer LayerFeature, 
        Dictionary<string, List<string>> SubnetworkNameToGlobalIDs, 
        ref Dictionary<string, CIMSymbol> SubnetColor)
    {
      int i = 0, nbColors = SubnetworkNameToGlobalIDs.Keys.Count();

      int[] myColors = GetColorValue();
      int NbColors = myColors.Count();

      List<CIMUniqueValueClass> uniqueValueClasses = new List<CIMUniqueValueClass>();

      foreach (string subnetworkName in SubnetworkNameToGlobalIDs.Keys)
      {
        List<CIMUniqueValue> subnetworkGroups = new List<CIMUniqueValue>();

        foreach (string globalID in SubnetworkNameToGlobalIDs[subnetworkName])
        {
          CIMUniqueValue uniqueValue = new CIMUniqueValue
          {
            FieldValues = new string[] { globalID }
          };
          subnetworkGroups.Add(uniqueValue);
        }

        if (!SubnetColor.TryGetValue(subnetworkName, out CIMSymbol rowSymbol))
        {
          if (subnetworkName == "Unknown")
            rowSymbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.CreateRGBColor(255.0, 0.0, 0.0), 2, SimpleLineStyle.Dash);
          else
          {
            int colIndex = (3 * i++) % NbColors;
            CIMColor color = ColorFactory.Instance.CreateRGBColor(myColors[colIndex], myColors[colIndex + 1], myColors[colIndex + 2]);

            rowSymbol = SymbolFactory.Instance.ConstructLineSymbol(color, 2, SimpleLineStyle.Solid);
            SubnetColor.Add(subnetworkName, rowSymbol);
          }
        }

        CIMUniqueValueClass rowClass = new CIMUniqueValueClass
        {
          Values = subnetworkGroups.ToArray(),
          Symbol = new CIMSymbolReference { Symbol = rowSymbol },
          Label = subnetworkName
        };

        uniqueValueClasses.Add(rowClass);
      }

      using (FeatureClass fc = LayerFeature.GetFeatureClass())
      {

        CIMUniqueValueRenderer renderer = new CIMUniqueValueRenderer
        {
          UseDefaultSymbol = true,
          DefaultLabel = "Undetermined",
          DefaultSymbol = new CIMSymbolReference { Symbol = SymbolFactory.Instance.ConstructLineSymbol(ColorFactory.Instance.CreateRGBColor(78, 78, 78), 2, SimpleLineStyle.Dash) },
          Fields = new string[] { GetFieldName(fc.GetDefinition().GetFields(), "ASSOCIATEDOBJECTGUID") },
          Groups = new CIMUniqueValueGroup[] { new CIMUniqueValueGroup { Classes = uniqueValueClasses.OrderBy(valueClass => valueClass.Label).ToArray() } }
        };

        LayerFeature.SetRenderer(renderer);
        MapView.Active.Redraw(true);
      }
    }

    /// <summary>
    /// Get a default list of color
    /// </summary>
    /// <returns></returns>
    public static int[] GetColorValue()
    {
      return new int[72] {   0,  60, 180, // Blue
                             0, 212,   0, // Green limon
                           212,   0,   0, // Red
                             0, 120,   0, // Dark green
                           120,   0,   0, // Dark red
                           212, 212,   0, // Light brown
                             0,   0, 120, // Dark blue
                            96,  30,  60, // Dark brown
                           180,  60,   0, // Orange
                            60, 180,   0, // Lignt Green 
                           180,   0,  60, // Dark Rose 
                             0, 180,  60, // Lignt Yellow
                            60,   0, 180, // Navy
                            30,  60,  96, // Blue
                           120,   0,  64, // Purple
                            60,  96,  30, // Dark Green
                             0,   0, 212, // Dark blue
                           212,   0, 212, // Light Purple
                             0, 212, 212, // Yellow
                           120,  64,   0, // Brown
                             0,  64, 120, // Blue black
                           196,  30,  60, // Red carmin
                            30,  60, 196, // Another blue
                            60, 196,  30  // Light Green
      };
    }

    /// <summary>
    /// Execute the color change on the layer
    /// </summary>
    /// <param name="diagramLayer">Layer to treat</param>
    internal static void ExecuteReductionEdgeColorBySubnetwork(DiagramLayer diagramLayer)
    {
      if (TheSymbols == null)
        TheSymbols = new Dictionary<string, Dictionary<string, CIMSymbol>>();

      if (diagramLayer != null)
      {
        QueuedTask.Run(() =>
        {
          using (NetworkDiagram diagram = diagramLayer.GetNetworkDiagram())
          {
            if (diagram == null)
              return;

            UtilityNetwork un = diagram.DiagramManager?.GetNetwork<UtilityNetwork>();
            string connectionString = ((Geodatabase)un.GetDatastore()).GetConnectionString();
            string DefaultLabel = "Undetermined";
            CIMSymbol DefaultSymbol = null;

            try
            {
              Dictionary<string, List<string>> subnetworkNameToGlobalIDs = GetSubnetworkNameToGlobalIDs(diagram);
              Dictionary<string, CIMSymbol> SubnetColor = GetSubnetColor(connectionString, DefaultLabel: ref DefaultLabel, DefaultSymbol: ref DefaultSymbol);
              IReadOnlyList<Layer> dgSublayers = diagramLayer.GetLayersAsFlattenedList();

              foreach (Layer subLayer in dgSublayers)
              {
                if (!(subLayer is FeatureLayer featureLayer) || featureLayer.ShapeType != esriGeometryType.esriGeometryPolyline)
                  continue;

                SetEdgeLayerRenderer(featureLayer, subnetworkNameToGlobalIDs, ref SubnetColor                  );
              }
            }
            catch (Exception ex)
            {
              ShowException(ex);
            }
          }

          MapView.Active.Redraw(false);
        });

      }
    }

    /// <summary>
    /// Get the symbol per subnetwork name
    /// </summary>
    /// <param name="ConnectionString">Connection String</param>
    /// <param name="DefaultLabel">Default Label</param>
    /// <param name="DefaultSymbol">Default Symbol</param>
    /// <returns>Dictionary( Subnetwork name, symbol)</returns>
    internal static Dictionary<string, CIMSymbol> GetSubnetColor(string ConnectionString, ref string DefaultLabel, ref CIMSymbol DefaultSymbol)
    {
      // Search the definition of renderer for Subnetwork
      Dictionary<string, CIMSymbol> SubnetColor = RetrieveRendererFromMaps(ConnectionString: ConnectionString, DefaultLabel: ref DefaultLabel, DefaultSymbol: ref DefaultSymbol);

      // Search the first definition 
      if (!TheSymbols.TryGetValue(ConnectionString, out Dictionary<string, CIMSymbol> SubnetColor1))
      {
        SubnetColor1 = new Dictionary<string, CIMSymbol>();
        TheSymbols.Add(ConnectionString, SubnetColor1);
      }

      // Update the symbol, or add new
      foreach (var v in SubnetColor)
      {
        if (!SubnetColor1.TryGetValue(v.Key, out _))
        {
          SubnetColor1.Add(v.Key, v.Value);
        }
      }

      return SubnetColor1;
    }

    /// <summary>
    /// Retrieve Renderer from projects map, according to the connection string
    /// </summary>
    /// <param name="ConnectionString">Connection string to compare with the Project map connection</param>
    /// <param name="DefaultLabel">Default Label</param>
    /// <param name="DefaultSymbol">Default Symbol</param>
    /// <returns>Dictionary(string, Symbol)</returns>
    private static Dictionary<string, CIMSymbol> RetrieveRendererFromMaps(string ConnectionString, ref string DefaultLabel, ref CIMSymbol DefaultSymbol)
    {
      Dictionary<string, CIMSymbol> TheSymbols = new Dictionary<string, CIMSymbol>();

      List<MapProjectItem> mapList = Project.Current.GetItems<MapProjectItem>().ToList();
      UtilityNetwork un = null;

      List<FeatureLayer> subnetLayers = new List<FeatureLayer>();
      string NameFieldSubNet = "";

      foreach (MapProjectItem MapItem in mapList)
      {
        if (MapItem.MapType != MapType.Map)
          continue;

        Map map = MapItem.GetMap();

        if (map == null)
          continue;

        if (GetUNFromMap(SearchMap: map, SearchConnection: ConnectionString, UN: ref un, SubnetLayers: ref subnetLayers, NameFieldSubNet: ref NameFieldSubNet))
          break;
      }

      if (un != null && subnetLayers != null && subnetLayers.Count > 0)
      {
        foreach (FeatureLayer fl in subnetLayers)
        {
          if (fl.GetRenderer() is CIMUniqueValueRenderer renderer)
          {
            string[] fieldsNames = renderer.Fields;

            if (!fieldsNames.Contains(NameFieldSubNet))
              break;

            try
            {
              DefaultLabel = renderer.DefaultLabel;
              DefaultSymbol = renderer.DefaultSymbol.Symbol;
            }
            catch { }


            CIMUniqueValueGroup[] groups = renderer.Groups;
            {
              foreach (CIMUniqueValueGroup group in groups)
              {
                CIMUniqueValueClass[] classes = group.Classes;
                if (classes == null)
                  continue;

                foreach (CIMUniqueValueClass uniqueValue in classes)
                {
                  // Only keep the first found, if it exists in another map
                  try
                  { TheSymbols.Add(uniqueValue.Label, uniqueValue.Symbol.Symbol); }
                  catch { }
                }
              }
            }
          }
        }

      }
      return TheSymbols;
    }
  }
}
