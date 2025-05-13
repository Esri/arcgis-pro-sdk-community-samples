/*

   Copyright 2025 Esri

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
using ActiproSoftware.Windows.Extensions;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Framework.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Controls;
using static GPToolInspector.TreeHelpers.TbxReader;

namespace GPToolInspector.TreeHelpers
{
  public class Param
  {
    public string ParamName { get; set; }
    public string type { get; set; }
    public string control_guid { get; set; }
    public string display_order { get; set; }
    public string direction { get; set; }
    public string displayname { get; set; }
    public Datatype datatype { get; set; }
    public Domain domain { get; set; }
    public List<RealDomain> RealDomains { get; set; }
    public string[] depends { get; set; }
    public string description { get; set; }
    public string value { get; set; }
  }

  public class Datatype
  {
    public string type { get; set; }
    public string displayname { get; set; }
    public Datatype[] datatypes { get; set; }
  }

  /// <summary>
  /// The domain class is used when the tool json is deserilized to a TbxToolInfo object.
  /// </summary>
  public class Domain
  {
    public string type { get; set; }
    public object[] items { get; set; }
    public Dictionary<string, string> range { get; set; }
    public dynamic low { get; set; }
    public dynamic high { get; set; }
    public string allowempty { get; set; }
    public string userasterfields { get; set; }
    public string guid { get; set; }
    public string checkfield { get; set; }
    public string singleband { get; set; }
    public string[] datatype { get; set; }
    public string[] geometrytype { get; set; }
    public string min { get; set; }
    public string max { get; set; }
    public string has_z { get; set; }
    public string include_z { get; set; }
    public string[] fieldtype { get; set; }
    public string[] filetypes { get; set; }
    public string chordaldistance { get; set; }
    public string[] neighbourtype { get; set; }
    public string[] maptype { get; set; }
    public string xml { get; set; }
    public string musthavejoins { get; set; }
    public string hidesubtypelayers { get; set; }
    public string hidesubtypegrouplayers { get; set; }
    public string[] featuretype { get; set; }
    public string[] filters { get; set; }
    public string hidejoinedlayers { get; set; }
    public string showonlystandalonetables { get; set; }
    public string[] portaltype { get; set; }
    public string[] workspacetype { get; set; }
    public string[] datasettype { get; set; }
    public string[] remaptype { get; set; }
    public RealDomain[] RealDomains { get; set; }
    public LowHigh RealLow { get; set; }
    public LowHigh RealHigh { get; set; }
  }

  public class RealDomain
  {
    public string DomainType { get; set; }
    public List<string> GeometryTypes { get; set; }
    public string Min { get; set; }
    public string Max { get; set; }
    public string UnitType { get; set; }
    public List<string> FieldTypes { get; set; }
    public List<string> ExcludeFields { get; set; }
    public string HasZ { get; set; }
    public string IncludeZ { get; set; }
    public string Guid { get; set; }
    public string UseRasterFields { get; set; }
    public List<string> FileTypes { get; set; }
    public List<string> WorkspaceType { get; set; }
    public List<string> DatasetTypes { get; set; }
    public string servicelayertype { get; set; }
    public List<string> ServiceLayerTypes { get; set; }
    public List<CodedValue> DomainCodedValues { get; set; }
    public List<string> GPUnitDomainValues { get; set; }
    public List<string> BrowseFiltersDomains { get; set; }
    // GPNumericDomain
    public string AllowEmpty { get; set; }
    public LowHigh RealLow { get; set; }
    public LowHigh RealHigh { get; set; }
    public bool CheckField { get; set; }
    public bool SingleBand { get; set; }
    public List<string> DataTypes { get; set; }
  }

  public class LowHigh
  {
    public string Inclusive { get; set; }
    public string Allow { get; set; }
    public string Value { get; set; }
  }

  public class CodedValue
  {
    public string Type { get; set; }
    public string Value { get; set; }
    public string Code { get; set; }
  }

  public class TbxToolInfo
  {
    public string Category { get; set; }
    public string FileName { get; set; }
    public string ToolName => $@"{Category}.{FileName}";

    public static readonly List<string> domains = [];

    public string type { get; set; }
    public string guid { get; set; }
    public string displayname { get; set; }
    public string helpcontent { get; set; }
    public string description { get; set; }
    public string product { get; set; }
    public Dictionary<string, Param> @params { get; set; }
    public string[] environments { get; set; }

    /// <summary>
    /// Read the tool content from the json file.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="tool"></param>
    /// <returns></returns>
    public static (bool Ok, string Message) ReadToolContent(string path, out TbxToolInfo tool)
    {
      tool = null;
      string content_file_path = string.Empty;
      try
      {
        var keyWordMap = TbxUtils.GetToolContentRcMap(path);
        content_file_path = System.IO.Path.Combine(path, ToolContentFileName);
        var json = File.ReadAllText(content_file_path, Encoding.UTF8);
        json = TbxReader.ReplaceUsingMap(json, keyWordMap);
        //File.WriteAllText(@"C:\temp\test.json", json);

        tool = System.Text.Json.JsonSerializer.Deserialize<TbxToolInfo>(json, TbxUtils.JsonOpt);
        foreach (var theParamKey in tool.@params.Keys)
        {
          var theParam = tool.@params[theParamKey];
          if (theParam.domain == null) continue;
          theParam.ParamName = theParamKey;
          theParam.RealDomains = [];
          theParam.RealDomains.AddRange(ParseParamDomain(theParam.domain, content_file_path));
          if (!domains.Contains(theParam.domain.type))
          {
            domains.Add(theParam.domain.type);
          }
        }
        return (true, string.Empty);
      }
      catch (Exception e)
      {
        var msg = $"Error reading '{content_file_path}' : {e.Message}";
        System.Diagnostics.Debug.Assert(false, msg);
        return (false, msg);
      }
    }

    private static List<RealDomain> ParseParamDomain(Domain domain, string contentFilePath)
    {
      var realDomains = new List<RealDomain>();
      var realDomain = new RealDomain();
      {
        realDomain.DomainType = domain.type;
        if (domain.geometrytype != null) realDomain.GeometryTypes = new List<string>(domain.geometrytype);
        if (domain.min != null) realDomain.Min = domain.min;
        if (domain.max != null) realDomain.Max = domain.max;
        if (domain.filetypes != null) realDomain.FileTypes = new List<string>(domain.filetypes);
      }
      switch (domain.type)
      {
        case "GPCodedValueDomain":
          if (domain.items == null) break;
          {
            if (realDomain.DomainCodedValues == null)
              realDomain.DomainCodedValues = [];
            foreach (var theItem in domain.items)
            {
              var itemDict = ((JsonElement)theItem).Deserialize<Dictionary<string, string>>();
              if (itemDict != null)
              {
                var codedValue = new CodedValue();
                if (itemDict.Remove("code", out string code)) codedValue.Code = code.ToString();
                if (itemDict.Remove("value", out string value)) codedValue.Value = value.ToString();
                if (itemDict.Remove("type", out string type)) codedValue.Type = type.ToString();
                realDomain.DomainCodedValues.Add(codedValue);
                if (itemDict.Count > 0)
                {
                  System.Diagnostics.Trace.Assert(false);
                }
              }
              else
                System.Diagnostics.Trace.Assert(false);
            }
          }
          break;
        case "GPCompositeDomain":
          if (domain.items == null) break;
          {
            ProcessGPCompositeDomain(realDomain, realDomains, domain, contentFilePath);
          }
          break;
        case "GPSceneServiceLayerDomain":
          // servicelayertype contains the domain value
          break;
        case "GPFileDomain":
          // domain.filetypes contain file extensions
          break;
        case "GPNumericDomain":
          if (!string.IsNullOrEmpty(domain.allowempty))
            System.Diagnostics.Trace.Assert(domain.allowempty == "true" || domain.allowempty == "false");
          if ((object)domain.low != null)
          {
            string itemJson = domain.low.ToString(); // suppose `dynamicObject` is your input
            Dictionary<string, string> itemDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(itemJson);
            domain.RealLow = new LowHigh();
            if (itemDict.Remove("val", out string value)) domain.RealLow.Value = value;
            if (itemDict.Remove("inclusive", out string inclusive)) domain.RealLow.Inclusive = inclusive;
            if (itemDict.Remove("allow", out string allow)) domain.RealLow.Allow = allow;
            if (itemDict.Count > 0)
            {
              System.Diagnostics.Trace.Assert(false);
            }
          }
          if ((object)domain.high != null)
          {
            string itemJson = domain.high.ToString(); // suppose `dynamicObject` is your input
            Dictionary<string, string> itemDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(itemJson);
            domain.RealHigh = new LowHigh();
            if (itemDict.Remove("val", out string value)) domain.RealHigh.Value = value;
            if (itemDict.Remove("inclusive", out string inclusive)) domain.RealHigh.Inclusive = inclusive;
            if (itemDict.Remove("allow", out string allow)) domain.RealHigh.Allow = allow;
            if (itemDict.Count > 0)
            {
              System.Diagnostics.Trace.Assert(false);
            }
          }
          break;
        case "GPFieldDomain":
          // fieldtype contains the field data types domain
          break;
        case "GPRangeDomain":
          // min/max contain the range domain values
          break;
        case "GPSAGeoDataDomain":
          // bool: checkfield, singleband, List<string>: datatype, fieldtype, geometrytype
          {
            if (domain.checkfield != null) realDomain.CheckField = Convert.ToBoolean(domain.checkfield);
            if (domain.singleband != null) realDomain.SingleBand = Convert.ToBoolean(domain.singleband);
            // already loaded
            // if (domain.geometrytype != null) realDomain.GeometryTypes = new List<string>(domain.geometrytype);
            if (domain.datatype != null) realDomain.DataTypes = new List<string>(domain.datatype);
            if (domain.fieldtype != null) realDomain.FieldTypes = new List<string>(domain.fieldtype);
          }
          break;
        case "GPSARemapDomain":
          // remaptype contains the domain values
          break;
        case "GPDatasetDomain":
          // datasettype contains the domain values
          break;
        case "GPTablesDomain":
          // hidejoinedlayers, showonlystandalonetables, portaltype contain the domain values
          break;
        case "GPWorkspaceDomain":
          // workspacetype contains the domain values
          break;
        case "GPLayerDomain":
          // geometrytype contain the domain values
          break;
        case "GPBrowseFiltersDomain":
          // filters contain the domain values
          if (realDomain.BrowseFiltersDomains == null) realDomain.BrowseFiltersDomains = [];
          if (domain.filters != null) realDomain.BrowseFiltersDomains.AddRange (new List<string>(domain.filters));
          break;
        case "GPUnitDomain":
          // realDomain.GPUnitDomainValues contains the domain value
          if (domain.items == null) break;
          if (realDomain.GPUnitDomainValues == null) realDomain.GPUnitDomainValues = [];
          if (domain.items != null)
          {
            foreach (var theItem in domain.items)
            {
              realDomain.GPUnitDomainValues.Add(theItem.ToString());
            }
          }
          {
            if (domain.range != null)
            {
              var rangeDict = domain.range;
              if (rangeDict != null)
              {
                foreach (var rangeKey in rangeDict.Keys)
                {
                  if (rangeKey == "min") realDomain.Min = rangeDict[rangeKey];
                  if (rangeKey == "max") realDomain.Max = rangeDict[rangeKey];
                  if (rangeKey == "unit_type") realDomain.UnitType = rangeDict[rangeKey];
                }
              }
            }
          }
          break;
        case "GPLayersAndTablesDomain":
          // musthavejoins, hidesubtypelayers, hidesubtypegrouplayers contains the domain value
          
          break;
        case "xmlserialize":
          // xml contains the domain value
          break;
        case "GPMapDomain":
          // maptype contains the domain value
          if (domain.filetypes == null) break;
          break;
        case "GPLocatorsDomain":
          // has no domain values
          break;
        case "GPGASearchNeighborhoodDomain":
          // chordaldistance, neighbourtype contain the domain values
          break;
        case "GPSANeighborhoodDomain":
          // neighbourtype contain the domain values
          break;
        case "GPFeatureClassDomain":
          // featuretype, portaltype, has_z, include_z, geometrytype contain the domain values
          break;
        default:
          if (domain.filetypes == null) break;
          throw new Exception($"Unknown domain type: {domain.type}");
      }
      realDomains.Insert(0, realDomain);
      return realDomains;
    }

    private static void ProcessGPCompositeDomain(RealDomain realDomain, List<RealDomain> realDomains, Domain domain, string contentFilePath)
    {
      foreach (var theItem in domain.items)
      {
        var itemDict = ((JsonElement)theItem).Deserialize<Dictionary<string, object>>();
        if (itemDict == null)
        {
          System.Diagnostics.Trace.Assert(false);
          continue;
        }
        var sType = string.Empty;
        if (itemDict.Remove("type", out object oType)) sType = oType.ToString();
        switch (sType)
        {
          case "GPSceneServiceLayerDomain":
            if (itemDict.Remove("servicelayertype", out object oServiceLayerType))
            {
              var thisRealDomain = new RealDomain();
              thisRealDomain.DomainType = sType;
              thisRealDomain.ServiceLayerTypes = ((JsonElement)oServiceLayerType).Deserialize<List<string>>();
              realDomains.Add(thisRealDomain);
            }
            break;
          case "GPFileDomain":
            if (itemDict.Remove("filetypes", out object ofiletypes))
            {
              var thisRealDomain = new RealDomain();
              thisRealDomain.DomainType = sType;
              thisRealDomain.FileTypes = ((JsonElement)ofiletypes).Deserialize<List<string>>();
              realDomains.Add(thisRealDomain);
            }
            break;
          case "GPWorkspaceDomain":
            if (itemDict.Remove("workspacetype", out object oworkspacetypes))
            {
              var thisRealDomain = new RealDomain();
              thisRealDomain.DomainType = sType;
              thisRealDomain.WorkspaceType = ((JsonElement)oworkspacetypes).Deserialize<List<string>>();
              realDomains.Add(thisRealDomain);
            }
            break;
          case "GPLayersAndTablesDomain":
            {
              var thisRealDomain = new RealDomain();
              thisRealDomain.DomainType = sType;
              realDomains.Add(thisRealDomain);
            }
            break;
          case "GPFeatureClassDomain":
            {
              if (itemDict.Remove("geometrytype", out object ogeometrytypes))
              {
                var thisRealDomain = new RealDomain();
                thisRealDomain.DomainType = sType;
                thisRealDomain.GeometryTypes = ((JsonElement)ogeometrytypes).Deserialize<List<string>>();
                realDomains.Add(thisRealDomain);
              }
              if (itemDict.Remove("has_z", out object ohas_z))
              {
                var thisRealDomain = new RealDomain();
                thisRealDomain.DomainType = sType;
                thisRealDomain.HasZ = ohas_z.ToString();
                realDomains.Add(thisRealDomain);
              }
              if (itemDict.Remove("include_z", out object oinclude_z))
              {
                var thisRealDomain = new RealDomain();
                thisRealDomain.DomainType = sType;
                thisRealDomain.IncludeZ = oinclude_z.ToString();
                realDomains.Add(thisRealDomain);
              }
            }
            break;
          case "null":
            {
              var thisRealDomain = new RealDomain();
              thisRealDomain.DomainType = sType;
              realDomains.Add(thisRealDomain);
            }
            break;
          case "GPDatasetDomain":
            if (itemDict.Remove("datasettype", out object odatasettypes))
            {
              var thisRealDomain = new RealDomain();
              thisRealDomain.DomainType = sType;
              thisRealDomain.DatasetTypes = ((JsonElement)odatasettypes).Deserialize<List<string>>();
              realDomains.Add(thisRealDomain);
            }
            break;
          case "GPBrowseFiltersDomain":
            // filters contain the domain values
            if (realDomain.BrowseFiltersDomains == null) realDomain.BrowseFiltersDomains = [];
            if (domain.filters != null) realDomain.BrowseFiltersDomains.AddRange(new List<string>(domain.filters));
            break;
          case "GPFieldDomain":
            {
              if (itemDict.Remove("fieldtype", out object ofieldtypes))
              {
                var thisRealDomain = new RealDomain();
                thisRealDomain.DomainType = sType;
                thisRealDomain.FieldTypes = ((JsonElement)ofieldtypes).Deserialize<List<string>>();
                realDomains.Add(thisRealDomain);
              }
              if (itemDict.Remove("exclude.field", out object oexcludefields))
              {
                var thisRealDomain = new RealDomain();
                thisRealDomain.DomainType = sType;
                thisRealDomain.ExcludeFields = ((JsonElement)oexcludefields).Deserialize<List<string>>();
                realDomains.Add(thisRealDomain);
              }
            }
            break;
          case "GPCodedValueDomain":
            realDomain.DomainCodedValues = [];
            if (!itemDict.ContainsKey("items"))
            {
              //System.Diagnostics.Trace.Assert(false);
              break;
            }
            var valueDicts = ((JsonElement)itemDict["items"]).Deserialize<Dictionary<string, string>[]>();
            foreach (var valueDict in valueDicts)
            {
              var codedValue = new CodedValue();
              if (valueDict.Remove("code", out string code)) codedValue.Code = code;
              if (valueDict.Remove("value", out string value)) codedValue.Value = value;
              if (valueDict.Remove("type", out string type)) codedValue.Type = type;
              realDomain.DomainCodedValues.Add(codedValue);
              if (valueDict.Count > 0)
              {
                System.Diagnostics.Trace.Assert(false);
              }
            }
            itemDict.Remove("items");
            break;
          case "GPUnitDomain":
            // realDomain.GPUnitDomainValues contains the domain value
            {
              var thisRealDomain = new RealDomain
              {
                DomainType = sType
              };
              if (realDomain.GPUnitDomainValues == null) realDomain.GPUnitDomainValues = [];
              if (itemDict.Remove("items", out object oitems))
              {
                realDomain.GPUnitDomainValues.Add(theItem.ToString());
              }
              realDomains.Add(thisRealDomain);
            }
            break;
          case "GPNumericDomain":
            // allowempty, low, high, etc.
            {
              var thisRealDomain = new RealDomain
              {
                DomainType = sType
              };
              if (itemDict.Remove("allowempty", out object oallowempty))
              {
                thisRealDomain.AllowEmpty = oallowempty.ToString();
              }
              if (itemDict.Remove("low", out object olow))
              {
                string json = olow.ToString(); // suppose `dynamicObject` is your input
                Dictionary<string, string> lowDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                domain.RealLow = new LowHigh();
                if (lowDict.Remove("val", out string lowvalue)) domain.RealLow.Value = lowvalue;
                if (lowDict.Remove("inclusive", out string lowinclusive)) domain.RealLow.Inclusive = lowinclusive;
                if (lowDict.Remove("allow", out string lowallow)) domain.RealLow.Allow = lowallow;
                if (lowDict.Count > 0)
                {
                  System.Diagnostics.Trace.Assert(false);
                }
              }
              if (itemDict.Remove("high", out object ohigh))
              {
                string json = ohigh.ToString(); // suppose `dynamicObject` is your input
                Dictionary<string, string> highDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                domain.RealLow = new LowHigh();
                if (highDict.Remove("val", out string highvalue)) domain.RealLow.Value = highvalue;
                if (highDict.Remove("inclusive", out string inclusive)) domain.RealLow.Inclusive = inclusive;
                if (highDict.Remove("allow", out string allow)) domain.RealLow.Allow = allow;
                if (highDict.Count > 0)
                {
                  System.Diagnostics.Trace.Assert(false);
                }
              }
              realDomains.Add(thisRealDomain);
              if (itemDict.Count > 0)
              {
                System.Diagnostics.Trace.Assert(false);
              }
            }
            break;
          case "GPRangeDomain":
            // min/max contain the range domain values
            {
              var thisRealDomain = new RealDomain
              {
                DomainType = sType
              };
              if (itemDict.Remove("max", out object omax))
              {
                thisRealDomain.Max = omax.ToString();
              }
              if (itemDict.Remove("min", out object omin))
              {
                thisRealDomain.Min = omin.ToString();
              }
              realDomains.Add(thisRealDomain);
            }
            break;
          case "GPLayerDomain":
            // geometrytype contain the domain values
            {
              var thisRealDomain = new RealDomain
              {
                DomainType = sType
              };
              if (itemDict.Remove("geometrytype", out object ogeometrytype))
              {
                thisRealDomain.GeometryTypes = ((JsonElement)ogeometrytype).Deserialize<List<string>>();
              }
              realDomains.Add(thisRealDomain);
            }
            break;
          case "GPCompositeDomain":
            // should always be empty
            if (itemDict.Count > 0)
            {
              // check if the item is empty
              if (itemDict.ContainsKey("items"))
              {
                var items = itemDict["items"];
                var dict = ((JsonElement)items).Deserialize<Dictionary<string, object>[]>();
                if (dict.Length > 1)
                {
                  System.Diagnostics.Trace.Assert(false);
                }
              }
              else
              {
                System.Diagnostics.Trace.Assert(false);
              }
            }
            break;
          case "GPSceneServiceSubLayerDomain":
            break;
          case "GPTablesDomain":
            break;
          case "xmlserialize":
            break;
          case "GPSAGeoDataDomain":
            break;
          default:
            System.Diagnostics.Trace.WriteLine($@"Unknown type in items: {sType} {contentFilePath}");
            System.Diagnostics.Trace.Assert(false);
            break;
        }
        if (itemDict.Count > 0)
        {
          //System.Diagnostics.Trace.Assert(false);
        }
        //System.Diagnostics.Trace.WriteLine(itemDict.ToString());
        //  string itemJson = theItem.ToString(); // suppose `dynamicObject` is your input
        //  Dictionary<string, string> itemDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(itemJson);
        //  var codedValue = new CodedValue();
        //  if (itemDict.Remove("code", out string code)) codedValue.code = code;
        //  if (itemDict.Remove("value", out string value)) codedValue.value = value;
        //  if (itemDict.Remove("type", out string type)) codedValue.type = type;
        //  domain.CodedValueItems.Append(codedValue);
        //  if (itemDict.Count > 0)
        //  {
        //    System.Diagnostics.Trace.Assert(false);
        //  }
      }
    }
  }


}
