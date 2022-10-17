using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExportDiagramToFeatureClasses
{
  /// <summary>
  /// Custom field used by the view model
  /// </summary>
  internal class CustomField
  {
    private readonly int _length;
    private readonly Type _keyType;
    private string _NewModelName;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="FieldToAdd">Field to customize</param>
    public CustomField(Field FieldToAdd)
    {
      OriginName = FieldToAdd.Name;
      OriginType = FieldToAdd.FieldType;
      NewType = OriginType;
      NewName = OriginName;

      Domain domain = FieldToAdd.GetDomain();

      if (domain != null && domain is CodedValueDomain valueDomain)
      {
        Dictionary<object, string> values = new();
        foreach (var v in valueDomain.GetCodedValuePairs())
        {
          if (!values.TryGetValue(v.Key, out _))
          {
            values.Add(v.Key, v.Value);
          }
        }

        FieldDomain = values;
        KeyValuePair<object, string> pair = FieldDomain.First();
        _keyType = pair.Key.GetType();
        HasDomain = true;
      }
      else
      {
        FieldDomain = null;
        HasDomain = false;
      }

      if (OriginName.Contains("UN_", StringComparison.OrdinalIgnoreCase) || OriginName.Contains("TN_", StringComparison.OrdinalIgnoreCase))
      {
        NewName = OriginName[(OriginName.LastIndexOf('.') + 1)..];
        if (string.Compare(NewName, "STATUS", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(NewName, "ROTATION", StringComparison.OrdinalIgnoreCase) == 0)
        {
          if (OriginName.Contains("ASSOCIATION", StringComparison.OrdinalIgnoreCase))
            NewName = "A_" + NewName;
          else
          {
            NewName = "D_" + NewName;
          }
        }
        else if (NewName.Contains("SOURCEID", StringComparison.OrdinalIgnoreCase))
        {
          NewName = NewName.Replace("SOURCEID", "SourceName", StringComparison.OrdinalIgnoreCase);
          NewType = FieldType.String;
        }
        else if (NewName.Contains("SRCID", StringComparison.OrdinalIgnoreCase))
        {
          NewName = NewName.Replace("SRCID", "SourceName", StringComparison.OrdinalIgnoreCase);
          NewType = FieldType.String;
        }
        else if (string.Compare(NewName, "ASSOCIATIONTYPE", StringComparison.OrdinalIgnoreCase) == 0)
        {
          NewType = FieldType.String;
        }
        else if (string.Compare(NewName, "OBJECTID", StringComparison.OrdinalIgnoreCase) == 0)
        {
          if (OriginName.Contains("_Junctions", StringComparison.OrdinalIgnoreCase) ||
              OriginName.Contains("_Edges", StringComparison.OrdinalIgnoreCase) ||
              OriginName.Contains("_Containers", StringComparison.OrdinalIgnoreCase) ||
              OriginName.Contains("_TmpJunctions", StringComparison.OrdinalIgnoreCase) ||
              OriginName.Contains("_TmpEdges", StringComparison.OrdinalIgnoreCase) ||
              OriginName.Contains("_TmpContainers", StringComparison.OrdinalIgnoreCase))
          {
            NewName = "D_OBJECTID";
            NewType = FieldType.Integer;
          }
          else
          {
            NewName = "S_OBJECTID";
            NewType = FieldType.Integer;
          }
        }
      }
      else if (NewName.Contains("OBJECTID", StringComparison.OrdinalIgnoreCase))
      {
        NewName = "S_OBJECTID";
        NewType = FieldType.Integer;
      }
      else if (OriginName.Contains('.'))
      {
        NewName = OriginName[(OriginName.LastIndexOf('.') + 1)..];        //NewName = qualifiedName;
      }
      else
      {
        NewName = OriginName;
      }

      if (NewName.Contains("SOURCEID", StringComparison.OrdinalIgnoreCase))
      {
        NewName = NewName.Replace("SOURCEID", "SourceName", StringComparison.OrdinalIgnoreCase);
        NewType = FieldType.String;
        _length = 50;
      }
      else if (NewName.Contains("SRCID", StringComparison.OrdinalIgnoreCase))
      {
        NewName = NewName.Replace("SRCID", "SourceName", StringComparison.OrdinalIgnoreCase);
        NewType = FieldType.String;
        _length = 50;
      }

      if (CompareWithNewName("ASSETTYPE") || CompareWithNewName("ASSETGROUP"))
      {
        NewType = FieldType.String;
        _length = 50;
      }
      else if (FieldDomain != null)
      {
        NewType = FieldType.String;
        _length = 255;
      }
      else if (NewType == FieldType.OID)
      {
        NewType = FieldType.Integer;
      }
      else
      {
        _length = FieldToAdd.Length;
      }

      Precision = FieldToAdd.Precision;
      AliasName = NewName == "S_OBJECTID" ? "Source ObjectId" : FieldToAdd.AliasName;
      Scale = FieldToAdd.Scale;
      HasDefaultValue = FieldToAdd.HasDefaultValue;
      if (HasDefaultValue)
      {
        // Sometimes it appears that the field type changed, even if the default value is still in the old type
        // In this case, there is an error at the feature class creation
        switch (NewType)
        {
          case FieldType.String:
            DefaultValue = FieldToAdd.GetDefaultValue().ToString();
            break;
          case FieldType.Integer:
            DefaultValue = Convert.ToInt32(FieldToAdd.GetDefaultValue());
            break;
          case FieldType.SmallInteger:
            DefaultValue = Convert.ToInt16(FieldToAdd.GetDefaultValue());
            break;
          case FieldType.Single:
            DefaultValue = Convert.ToSingle(FieldToAdd.GetDefaultValue());
            break;
          case FieldType.Double:
            DefaultValue = Convert.ToDouble(FieldToAdd.GetDefaultValue());
            break;
          case FieldType.Date:
            DefaultValue = Convert.ToDateTime(FieldToAdd.GetDefaultValue());
            break;
        }
      }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="FieldName">New field name</param>
    /// <param name="TypeField">Field type</param>
    public CustomField(string FieldName, FieldType TypeField)
    {
      OriginName = FieldName;
      OriginType = TypeField;
      NewType = OriginType;
      NewName = OriginName;

    }

    /// <summary>
    /// Field name in the source feature class
    /// </summary>
    public string OriginName { get; }

    /// <summary>
    /// New name, without qualification, used in the new geodatabase
    /// </summary>
    public string NewName { get; }

    /// <summary>
    /// Get the source field type
    /// This can be changed, if it is a domain field
    /// </summary>
    public FieldType OriginType { get; }

    /// <summary>
    /// New field type
    /// </summary>
    public FieldType NewType { get; }

    /// <summary>
    /// Use to change the domain value into domain name
    /// </summary>
    public Dictionary<object, string> FieldDomain { get; }

    /// <summary>
    /// Get the length of the new field
    /// </summary>
    public int Length
    {
      get
      {
        if (NewType == FieldType.String && OriginType != NewType)
        {
          return 255;
        }
        return _length;
      }
    }

    /// <summary>
    /// Get the precision of the new field
    /// </summary>
    public int Precision { get; }

    /// <summary>
    /// Get the alias name of the new field
    /// </summary>
    public string AliasName { get; }

    /// <summary>
    /// Get the new field name with the qualified name if needed
    /// </summary>
    public string NewModelName
    {
      get
      {
        if (string.IsNullOrEmpty(_NewModelName))
          return NewName;

        return _NewModelName;
      }
      set { _NewModelName = value; }
    }

    /// <summary>
    /// Flag to indicate if the field has a default value
    /// </summary>
    public bool HasDefaultValue { get; }

    /// <summary>
    /// Default Value
    /// </summary>
    public object DefaultValue { get; }

    /// <summary>
    /// Scale
    /// </summary>
    public int Scale { get; }

    /// <summary>
    /// Flag to indicate if the field has a domain name
    /// </summary>
    public bool HasDomain { get; }

    /// <summary>
    /// Indicate if the field has the search name
    /// </summary>
    /// <param name="FieldName">Search name</param>
    /// <returns>True if same name</returns>
    public bool CompareWithNewName(string FieldName)
    {
      return string.Compare(FieldName, NewName, StringComparison.OrdinalIgnoreCase) == 0;
    }

    /// <summary>
    /// Get the domain name
    /// </summary>
    /// <param name="Index">Domain index</param>
    /// <returns>String</returns>
    public string GetDomainValue(object Index)
    {
      string key = "";
      if (typeof(short) == _keyType)
      {
        FieldDomain.TryGetValue(Convert.ToInt16(Index), out key);
      }
      else if (typeof(int) == _keyType)
      {
        FieldDomain.TryGetValue(Convert.ToInt32(Index), out key);
      }
      else if (typeof(long) == _keyType)
      {
        FieldDomain.TryGetValue(Convert.ToInt64(Index), out key);
      }

      if (string.IsNullOrEmpty(key))
      {
        return Index.ToString();
      }

      return key;
    }

    /// <summary>
    /// Flag to indicate whether the field name changed from SourceId To SourceName
    /// </summary>
    public bool IsSourceName
    {
      get { return NewName.Contains("SourceName", StringComparison.OrdinalIgnoreCase); }
    }

    /// <summary>
    /// Flag to indicate whether the field is the network source class Asset Group field
    /// </summary>
    public bool IsAssetGroup
    {
      get { return NewName.Contains("AssetGroup", StringComparison.OrdinalIgnoreCase); }
    }

    /// <summary>
    /// Flag to indicate whether the field is the network source class Asset Type field
    /// </summary>
    public bool IsAssetType
    {
      get { return NewName.Contains("AssetType", StringComparison.OrdinalIgnoreCase); }
    }

    /// <summary>
    /// Flag to indicate whether the field defines an association type
    /// </summary>
    public bool IsAssociationType
    {
      get { return NewName.Contains("AssociationType", StringComparison.OrdinalIgnoreCase) || string.Compare(NewName, "D_ASSOCIATION", true) == 0; }
    }

    /// <summary>
    /// List of the Asset Group for the feature Class, if the field is an Asset Group field
    /// </summary>
    public IReadOnlyList<AssetGroup> AssetGroups { get; internal set; }

    /// <summary>
    /// Get the Asset Group from its code
    /// </summary>
    /// <param name="Code">Asset Group code</param>
    /// <returns>Asset Group</returns>
    public AssetGroup GetAssetGroup(int Code)
    {
      if (AssetGroups == null)
        return null;

      return AssetGroups.FirstOrDefault(a => a.Code == Code);
    }

    /// <summary>
    /// Get the Asset Group from its name
    /// </summary>
    /// <param name="Name">Asset Group name</param>
    /// <returns>Asset Group</returns>
    public AssetGroup GetAssetGroup(string Name)
    {
      if (AssetGroups == null)
        return null;

      AssetGroup result = AssetGroups.FirstOrDefault(a => string.Compare(a.Name, Name, StringComparison.OrdinalIgnoreCase) == 0);
      if (result == null)
      {
        List<AssetGroup> resultList = AssetGroups.Where(a => Name.Contains(a.Name, StringComparison.OrdinalIgnoreCase)).ToList();
        if (resultList.Count == 1)
        {
          return resultList[0];
        }

        int maxLenght = 0;
        foreach (var v in resultList)
        {
          if (v.Name.Length > maxLenght)
          {
            maxLenght = v.Name.Length;
            result = v;
          }
        }

      }
      return result;
    }
  }
}