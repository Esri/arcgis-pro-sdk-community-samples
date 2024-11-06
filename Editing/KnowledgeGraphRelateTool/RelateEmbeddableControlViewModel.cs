//
// Copyright 2024 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       https://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace KnowledgeGraphRelateTool
{
  /// <summary>
  /// An embeddable control that is displayed by the tool.  Allows users to specify the source and destination
  /// entity types, the relate name and description. 
  /// </summary>
  internal class RelateEmbeddableControlViewModel : EmbeddableControl
  {
    public RelateEmbeddableControlViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }

    /// <summary>
    /// Initialize with a KnowledgeGraphLayer.
    /// </summary>
    /// <param name="kgLayer"></param>
    internal void Init(KnowledgeGraphLayer kgLayer)
    {
      // clear the collections
      Clear();
      if (kgLayer == null)
        return;

      // get the list of entity and relate types from the knowledgegraph layer
      var (entities, relates) = CollectNamedTypes(kgLayer);

      // assign to the properties
      _entityNames = entities.ToList();
      _relateNames = relates.ToList();
      NotifyPropertyChanged(nameof(EntityNames));
      NotifyPropertyChanged(nameof(RelateNames));

      RelateDirection = Directions[0];
    }

    /// <summary>
    /// Clear all the properties.
    /// </summary>
    private void Clear()
    {
      _entityNames = new List<string>();
      _relateNames = new List<string>();
      NotifyPropertyChanged(nameof(EntityNames));
      NotifyPropertyChanged(nameof(RelateNames));

      SourceEntity = "";
      DestinationEntity = "";
      Relate = "";
      RelateDirection = "";
    }

    /// <summary>
    /// Collects the list of entity and relate types from a KnowledgeGraphLayer
    /// </summary>
    private (IEnumerable<string> entities, IEnumerable<string> relates) CollectNamedTypes(KnowledgeGraphLayer kgLayer)
    {
      var entities = kgLayer.GetLayersAsFlattenedList().OfType<LinkChartFeatureLayer>().Where(lcfl => lcfl.IsEntity).Select(lcfl => lcfl.Name).ToList();
      var relates = kgLayer.GetLayersAsFlattenedList().OfType<LinkChartFeatureLayer>().Where(lcfl => lcfl.IsRelationship).Select(lcfl => lcfl.Name).ToList();

      return (entities, relates);
    }

    private List<string> _entityNames;
    public List<string> EntityNames => _entityNames;

    private List<string> _relateNames;
    public List<String> RelateNames => _relateNames;

    private List<string> _directions = new List<string>() { "Forward", "Backward" };
    public List<String> Directions => _directions;

    private string _sourceEntity;
    public string SourceEntity
    {
      get => _sourceEntity;
      set => SetProperty(ref _sourceEntity, value);
    }

    private string _destinationEntity;
    public string DestinationEntity
    {
      get => _destinationEntity;
      set => SetProperty(ref _destinationEntity, value);
    }

    private string _relate;
    public string Relate
    {
      get => _relate;
      set => SetProperty(ref _relate, value);
    }

    private string _relateDirection;
    public string RelateDirection
    {
      get => _relateDirection;
      set => SetProperty(ref _relateDirection, value);
    }

    public bool IsDirectionForward => RelateDirection == Directions[0];
  }
}

