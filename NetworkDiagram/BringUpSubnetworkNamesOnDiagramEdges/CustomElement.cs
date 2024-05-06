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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BringUpSubnetworkNamesOnDiagramEdges
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
    protected string _subnetworkName;
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
    /// <summary>
    /// Subnetwork Name
    /// </summary>
    public string SubnetworkName
    {
      get => _subnetworkName;
      set
      {
        _subnetworkName = value;
      }
    }
    #endregion
  }

  /// <summary>
  /// Class to manage Diagram Element
  /// </summary>
  internal class CustomElement : CustomAggregation
  {
    protected readonly DiagramElement _element;
    protected readonly string _infoOriginal;
    private readonly List<CustomAggregation> _aggregations = [];
    protected string _info;

    public CustomElement(DiagramElement Element, string Info)
    : base(Element.ID, Element.AssociatedGlobalID, Element.AssociatedSourceID)
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
      get => _info;
      set { _info = value; }
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

    /// <summary>
    /// Get the aggregation list by SourceId
    /// </summary>
    /// <param name="SourceIds">Searching SourceId</param>
    /// <returns>CustomAggregation List</returns>
    public List<CustomAggregation> AggregationsBySourceId(int[] SourceIds)
    {
      List<CustomAggregation> myList = [];
      foreach (int i in SourceIds)
      {
        myList.AddRange(_aggregations.Where(a => a.SourceID == i));
      }

      return myList;
    }

    /// <summary>
    /// Get the Element Subnetwork Name when it exists
    /// If not, get the list of the subnetworks for the aggregated elements
    /// </summary>
    public string AggregatedSubnetworkName
    {
      get
      {
        if (!string.IsNullOrEmpty(_subnetworkName) && !_subnetworkName.Equals(BringUpSubnetworkNamesOnDiagramEdgesModule.cUnknown, StringComparison.OrdinalIgnoreCase))
          return _subnetworkName;
        if (_aggregations.Count == 0)
          return _subnetworkName;

        List<string> subnetworks = [];
        bool containtUnknown = false;
        foreach (var aggregation in _aggregations)
        {
          if (!string.IsNullOrEmpty(aggregation.SubnetworkName) && !subnetworks.Contains(aggregation.SubnetworkName))
          {
            if (aggregation.SubnetworkName.Equals(BringUpSubnetworkNamesOnDiagramEdgesModule.cUnknown, StringComparison.OrdinalIgnoreCase))
              containtUnknown = true;
            else
              subnetworks.Add(aggregation.SubnetworkName);
          }
        }

        if (subnetworks.Count == 0)
        {
          if (containtUnknown)
            return BringUpSubnetworkNamesOnDiagramEdgesModule.cUnknown;
          return _subnetworkName;
        }

        StringBuilder sb = new();
        sb.Clear();
        foreach (var subnetwork in subnetworks)
        {
          sb.AppendFormat($"{subnetwork}::");
        }

        return sb.ToString()[..^2];
      }
    }
  }

  /// <summary>
  /// Class to manage the CustomElement list and clarify the code
  /// </summary>
  internal class ManageCustomElement
  {
    private readonly List<CustomElement> _elements = [];

    /// <summary>
    /// Add Element to the intrnal list
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
    {
      CustomElement cust = _elements.FirstOrDefault(a => a.DEID == DEID);
      return cust;
    }

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
