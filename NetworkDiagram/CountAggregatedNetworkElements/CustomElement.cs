/*

   Copyright 2024 Esri

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
using ArcGIS.Core.Data.NetworkDiagrams;
using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CountAggregatedNetworkElements
{
  /// <summary>
  /// Custom Aggregation, parent of Custom Element
  /// </summary>
  /// <param name="Deid">Diagram Element Id</param>
  /// <param name="GlobalId">Associated Global Id</param>
  /// <param name="SourceID">Associated Source Id</param>
  internal class CustomAggregation(int Deid, Guid GlobalId, int SourceID)
  {
    public enum AggregationTypeEnum
    {
      Junction,
      Edge,
      Container
    }
    #region Necessary fields
    protected readonly int _deid = Deid;
    protected readonly Guid _associatedObjectGuid = GlobalId;
    protected readonly int _sourceID = SourceID;
    private AggregationTypeEnum _customType;

    /// <summary>
    /// Diagram Element ID
    /// </summary>
    public int DEID { get => _deid; }

    /// <summary>
    /// Element GlobalID in the Associated feature class
    /// </summary>
    public Guid AssociatedGlobalID { get => _associatedObjectGuid; }

    /// <summary>
    /// Associated feature class Source ID 
    /// </summary>
    public int SourceID { get => _sourceID; }

    /// <summary>
    /// Element Type
    /// </summary>
    public AggregationTypeEnum CustomType { get => _customType; set => _customType = value; }
    #endregion

    #region Optional fields
    private string _associationStatus;
    private string _assetGroup;
    private string _assetType;

    /// <summary>
    /// Association Status, use to determine whether an element is a container
    /// </summary>
    public string AssociationStatus { get => _associationStatus; set => _associationStatus = value; }

    /// <summary>
    /// Asset Group Label
    /// </summary>
    public string AssetGroup { get => _assetGroup; set => _assetGroup = value; }

    /// <summary>
    /// Asset Type Label
    /// </summary>
    public string AssetType { get => _assetType; set => _assetType = value; }
    #endregion
  }

  /// <summary>
  /// Class to manage Diagram Element
  /// </summary>
  internal class CustomElement : CustomAggregation
  {
    protected readonly DiagramElement _element;
    protected readonly string _infoOriginal;
    protected string _info;
    private readonly List<CustomAggregation> _aggregations = [];

    public CustomElement(DiagramElement Element, string Info) :
      base(Element.ID, Element.AssociatedGlobalID, Element.AssociatedSourceID)
    {
      _element = Element;
      _info = Info ?? "";
      _infoOriginal = _info;

      if (_element is DiagramJunctionElement)
        CustomType = AggregationTypeEnum.Junction;
      else if (_element is DiagramEdgeElement)
        CustomType = AggregationTypeEnum.Edge;
      else
        CustomType = AggregationTypeEnum.Container;
    }

    /// <summary>
    /// Initial Diagram Element
    /// </summary>
    public DiagramElement DiagramElement
    { get => _element; }

    /// <summary>
    /// The associated info
    /// </summary>
    public string Info
    {
      get => _info; set { _info = value; }
    }

    /// <summary>
    /// Return true if the actual Info is not already initialized
    /// </summary>
    public bool InfoHasChanged
    {
      get
      {
        if (string.IsNullOrEmpty(_info) && !string.IsNullOrEmpty(_infoOriginal))
          return true;
        else if (!string.IsNullOrEmpty(_info) && string.IsNullOrEmpty(_infoOriginal))
          return true;
        else if (_info != _infoOriginal)
          return true;

        return false;
      }
    }

    /// <summary>
    /// Aggregation Count
    /// </summary>
    public int NbAggregations { get => _aggregations.Count; }

    /// <summary>
    /// Aggregations List
    /// </summary>
    public List<CustomAggregation> Aggregations { get => _aggregations; }

    public List<CustomAggregation> GetAggregationsBySourceId(int[] IDs)
    {
      List<CustomAggregation> myList = [];

      foreach (int i in IDs)
      {
        myList.AddRange(Aggregations.Where(a => a.SourceID == i));
      }

      return myList;
    }

    /// <summary>
    /// Count the aggregated objects for the specified Network Source, Asset Group and Asset Type
    /// </summary>
    /// <param name="SearchSource">Associated Source ID</param>
    /// <param name="AssetGroupName">Asset Group Name</param>
    /// <param name="AssetTypeName">Asset Type Name</param>
    /// <returns>int</returns>
    public int CountSpecificAssetGroupAssetType(int SearchSource, string AssetGroupName, string AssetTypeName)
    {
      if (string.IsNullOrEmpty(AssetTypeName))
      {
        if (string.IsNullOrEmpty(AssetGroupName))
        {
          return _aggregations.Where(a => a.SourceID == SearchSource).Count();
        }
        else
        {
          return _aggregations.Where(a => a.SourceID == SearchSource && a.AssetGroup == AssetGroupName).Count();
        }
      }
      else
      {
        return _aggregations.Where(a => a.SourceID == SearchSource && a.AssetGroup == AssetGroupName && a.AssetType == AssetTypeName).Count();
      }
    }

    /// <summary>
    /// Count the aggregated objects by type
    /// </summary>
    /// <param name="NbJunctions">Total aggregated junctions</param>
    /// <param name="NbEdges">Total aggregated edges</param>
    /// <param name="NbContainers">Total aggregated containers</param>
    public void GetNbAggregations(out int NbJunctions, out int NbEdges, out int NbContainers)
    {
      NbJunctions = GetAggregationsByCustomType(AggregationTypeEnum.Junction).Count;
      NbEdges = GetAggregationsByCustomType(AggregationTypeEnum.Edge).Count;
      NbContainers = GetAggregationsByCustomType(AggregationTypeEnum.Container).Count;
    }

    /// <summary>
    /// Get Aggregations By CustomType
    /// </summary>
    /// <param name="TypeAggregation">AggregationTypeEnum</param>
    /// <returns>List of CustomAggregation</returns>
    public List<CustomAggregation> GetAggregationsByCustomType(AggregationTypeEnum TypeAggregation)
    {
      return _aggregations.Where(a=> a.CustomType == TypeAggregation).ToList();
    }
  }

  /// <summary>
  /// Class to manage the CustomElement list and clarify the code
  /// </summary>
  internal class ManageCustomElement
  {
    private readonly List<CustomElement> _elements = [];

    /// <summary>
    /// Add Element to the internal list
    /// </summary>
    /// <param name="Element"></param>
    public void Add(CustomElement Element)
    {
      if (Element != null)
        _elements.Add(Element);
    }

    /// <summary>
    /// Element list
    /// </summary>
    public List<CustomElement> Elements
    { get => _elements; }

    /// <summary>
    /// Get all elements with updated Info field
    /// </summary>
    /// <param name="Filter">return the type of all Element in the list</param>
    /// <returns>List of DiagramElementInfo></returns>
    public List<DiagramElementInfo> GetChangedDiagramElementInfo(out DiagramElementFilter Filter)
    {
      Filter = new();
      List<DiagramElementInfo> newList = [];
      var result = _elements.Where(a => a.InfoHasChanged).ToList();

      foreach (var r in result)
      {
        newList.Add(new(r.DEID, r.Info));
        if (r.CustomType == CustomAggregation.AggregationTypeEnum.Junction)
          Filter.BrowseJunctions = true;
        else if (r.CustomType == CustomAggregation.AggregationTypeEnum.Edge)
          Filter.BrowseEdges = true;
        else if (r.CustomType == CustomAggregation.AggregationTypeEnum.Container)
          Filter.BrowseContainers = true;
      }

      return newList;
    }

    /// <summary>
    /// Get the element identified by its Diagram Element ID
    /// </summary>
    /// <param name="DEID">Diagram Element ID</param>
    /// <returns>CustomElement</returns>
    public CustomElement GetElementByDEID(int DEID)
    { return _elements.FirstOrDefault(a => a.DEID == DEID); }

    /// <summary>
    /// Get Elements by Source ID
    /// </summary>
    /// <param name="SourceIDs">Array of Source ID</param>
    /// <returns>List of CustomElement</returns>
    public List<CustomElement> GetElementsBySourcesID(int[] SourceIDs)
    {
      List<CustomElement> newList = [];
      foreach (int i in SourceIDs)
      {
        newList.AddRange([.. _elements.Where(a => a.SourceID == i)]);
      }

      return newList;
    }

    /// <summary>
    /// Get a custom Element list by filtering
    /// </summary>
    /// <param name="Filter"></param>
    /// <returns>List of CustomElement</returns>
    public List<CustomElement> GetFilteredElements(DiagramElementFilter Filter)
    {
      List<CustomElement> newList = [];
      if (Filter.BrowseContainers)
        newList.AddRange([.. _elements.Where(a => a.CustomType == CustomAggregation.AggregationTypeEnum.Container)]);

      if (Filter.BrowseJunctions)
        newList.AddRange([.. _elements.Where(a => a.CustomType == CustomAggregation.AggregationTypeEnum.Junction)]);

      if (Filter.BrowseEdges)
        newList.AddRange([.. _elements.Where(a => a.CustomType == CustomAggregation.AggregationTypeEnum.Edge)]);

      return newList;
    }

    /// <summary>
    /// Get from and to Element by their Diagram Element ID
    /// </summary>
    /// <param name="FromDEID">From Diagram Element ID</param>
    /// <param name="ToDEID">To Diagram Element ID</param>
    /// <returns>Tuple (From Custom Element, To Custom Element) </returns>
    /// <remarks>It is used when an element has no subnetwork name
    /// In this case we determine the subnetwork name from its extremities</remarks>
    public Tuple<CustomElement, CustomElement> GetExtremities(int FromDEID, int ToDEID)
    {
      return Tuple.Create(GetElementByDEID(FromDEID), GetElementByDEID(ToDEID));
    }
  }
}
