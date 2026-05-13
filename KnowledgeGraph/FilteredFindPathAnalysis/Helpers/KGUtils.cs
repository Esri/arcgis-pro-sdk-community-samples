/*

   Copyright 2026 Esri

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
using ArcGIS.Core.Data.Knowledge;
using ArcGIS.Core.Data.Knowledge.Analytics;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilteredFindPathAnalysis.Helpers
{

	public enum FFP_Analysis_Type
	{
		FFP,
		Expand,
		FilteredExpand,
		Connect,
		FindBetween,
	}

	public enum OIDCombineMethod
	{
		UnionWithExisting,
		IntersectWithExisting,
		SubtractExisting
	};

	internal class KGUtils
	{
		//Singleton
		public static readonly KGUtils Instance = new KGUtils();

		private KGUtils() { }

		internal KnowledgeGraph InitKnowledgeGraph(string url)
		{
			if (MapView.Active?.Map == null)
			{
				System.Diagnostics.Debug.WriteLine("\r\n**MapView is null**");
			}

			var kg_layer = MapView.Active?.Map?.GetLayersAsFlattenedList()?
											.OfType<ArcGIS.Desktop.Mapping.KnowledgeGraphLayer>()?
											.FirstOrDefault();

			KnowledgeGraph kg = null;
			var fl = kg_layer?.GetLayersAsFlattenedList()?
				.OfType<FeatureLayer>()?.FirstOrDefault();
			if (fl != null)
			{
				using (var fc = fl.GetFeatureClass())
					kg = fc.GetDatastore() as KnowledgeGraph;
			}
			else
			{
				var stbl = kg_layer?.GetStandaloneTablesAsFlattenedList().FirstOrDefault();
				if (stbl != null)
				{
					using (var table = stbl.GetTable())
						kg = table.GetDatastore() as KnowledgeGraph;
				}
			}

			if (kg == null)
			{
				var kg_connect = new KnowledgeGraphConnectionProperties(
														 new Uri(url));
				kg = new KnowledgeGraph(kg_connect);
			}
			return kg;
		}

		public string FormatID(string id)
		{
			id = id.ToUpperInvariant();
			if (!id.StartsWith('{'))
				id = '{' + id;
			if (!id.EndsWith('}'))
				id += '}';
			return id;
		}

		public Dictionary<string, List<long>> CombineOIDDicts(
			Dictionary<string, List<long>> dictNewOIDs,
			Dictionary<string, List<long>> dictExistingOIDs,
			OIDCombineMethod method)
		{
			var combined = new Dictionary<string, List<long>>();

			switch (method)
			{
				case OIDCombineMethod.IntersectWithExisting:
					foreach (var kvp in dictNewOIDs)
					{
						if (dictExistingOIDs.ContainsKey(kvp.Key))
						{
							var intersected = kvp.Value.Intersect(dictExistingOIDs[kvp.Key]).ToList();
							if (intersected.Count > 0)
								combined[kvp.Key] = intersected;
						}
					}
					break;
				case OIDCombineMethod.SubtractExisting:
					foreach (var kvp in dictNewOIDs)
					{
						if (dictExistingOIDs.ContainsKey(kvp.Key))
						{
							var subtracted = kvp.Value.Except(dictExistingOIDs[kvp.Key]).ToList();
							if (subtracted.Count > 0)
								combined[kvp.Key] = subtracted;
						}
						else
						{
							combined[kvp.Key] = kvp.Value;
						}
					}
					break;
				case OIDCombineMethod.UnionWithExisting:
					foreach (var kvp in dictNewOIDs)
					{
						var values = kvp.Value;
						if (dictExistingOIDs.ContainsKey(kvp.Key))
							values = values.Union(dictExistingOIDs[kvp.Key]).ToList();
						combined[kvp.Key] = values;
					}
					break;
			}
			return combined;
		}

		internal List<string> GetEntityTypeNames(KnowledgeGraph kg)
		{
			using (var kg_dm = kg.GetDataModel())
			{
				return kg_dm.GetEntityTypes().Keys.ToList();
			}
		}

		#region FFP

		internal CIMFilteredFindPathsConfiguration GetConfiguration(
			Map linkChart, FFP_Analysis_Type ffp_type)
		{
			switch (ffp_type)
			{
				case FFP_Analysis_Type.FFP:
					return FFPConfig(linkChart);
				case FFP_Analysis_Type.Expand:
					return ExpandConfig(linkChart);
				case FFP_Analysis_Type.FilteredExpand:
					return FilteredExpandConfig(linkChart);
				case FFP_Analysis_Type.Connect:
					return ConnectConfig(linkChart);
				case FFP_Analysis_Type.FindBetween:
					return FindBetweenConfig(linkChart);
			}
			return null;

			CIMFilteredFindPathsConfiguration FFPConfig(Map linkChart)
			{
				var ffp_config = new CIMFilteredFindPathsConfiguration();
				ffp_config.Name = "FFP";

				//Configure Origin and Destination Entities
				var originEntities = new List<CIMFilteredFindPathsEntity>();
				var destEntities = new List<CIMFilteredFindPathsEntity>();

				var entity_types =  //Hardcoded for sample data, change as needed
						new List<string> { "Species", "User", "Observation" };

				foreach (var entity_type in entity_types)
				{
					var ffp_entity = new CIMFilteredFindPathsEntity();
					ffp_entity.EntityTypeName = entity_type;
					ffp_entity.PropertyFilterPredicate = "";
					originEntities.Add(ffp_entity);
					destEntities.Add(ffp_entity);
				}

				ffp_config.OriginEntities = originEntities.ToArray();
				ffp_config.DestinationEntities = destEntities.ToArray();

				//Path filters
				ffp_config.PathFilters = [];
				//Traversal
				ffp_config.TraversalDirections = [];

				//Calculations and costs - default values
				ffp_config.RelationshipCostProperty = "";
				ffp_config.DefaultRelationshipCost = 1.0;
				ffp_config.DefaultTraversalDirectionType = KGTraversalDirectionType.Any;
				ffp_config.EntityUsage = FilteredFindPathsEntityUsage.AnyOriginAnyDestination;
				ffp_config.PathMode = KGPathMode.Shortest;
				ffp_config.MinPathLength = 1;
				ffp_config.MaxPathLength = 8;
				ffp_config.MaxCountPaths = (int)100000;//Change as needed
				ffp_config.ClosedPathPolicy = KGClosedPathPolicy.Forbid;
				ffp_config.TimeFilter = null;

				return ffp_config;
			}

			CIMFilteredFindPathsConfiguration ExpandConfig(Map linkChart)
			{
				var ffp_config = new CIMFilteredFindPathsConfiguration();
				ffp_config.Name = "Expand";

				//Get the ids of the selected features
				var map_sel = linkChart.GetSelection();
				var kg_id_set = map_sel.ToKnowledgeGraphIDSet();

				var dict_uids_by_type = kg_id_set.ToUIDDictionary();
				var all_type_names = kg_id_set.GetEntityRelationshipTypeNames();

				//Configure Origin and Destination Entities
				var originEntities = new List<CIMFilteredFindPathsEntity>();
				var destEntities = new List<CIMFilteredFindPathsEntity>();

				//Add in origins and destinations
				foreach (var entity_type_name in all_type_names.entityTypeNames)
				{
					var uids = dict_uids_by_type[entity_type_name];
					foreach (var uid in uids)//Something has to be selected
					{
						//Add in the selected entities as the origins
						var sel_entity = new CIMFilteredFindPathsEntity();
						sel_entity.EntityTypeName = entity_type_name;
						sel_entity.ID = KGUtils.Instance.FormatID(uid.ToString());

						originEntities.Add(sel_entity);
					}
				}

				var kg_layer = linkChart.GetLayersAsFlattenedList()
					.OfType<KnowledgeGraphLayer>().First();

				//Add all entity types in as destination types
				var dest_entities = GetEntityTypeNames(kg_layer.GetDatastore());
				foreach(var dest_entity in dest_entities)
				{
					//Add all entity types in as destination types
					var dest_entity_ffp = new CIMFilteredFindPathsEntity();
					dest_entity_ffp.EntityTypeName = dest_entity;
					destEntities.Add(dest_entity_ffp);
				}

				ffp_config.OriginEntities = originEntities.ToArray();
				ffp_config.DestinationEntities = destEntities.ToArray();

				//Path filters
				ffp_config.PathFilters = [];
				//Traversal
				ffp_config.TraversalDirections = [];

				ffp_config.RelationshipCostProperty = "";
				ffp_config.DefaultRelationshipCost = 1.0;
				ffp_config.DefaultTraversalDirectionType = KGTraversalDirectionType.Any;
				ffp_config.EntityUsage = FilteredFindPathsEntityUsage.AllOriginsAnyDestination;//One shortest path per origin
				ffp_config.PathMode = KGPathMode.Shortest;
				ffp_config.MinPathLength = (int)1;
				ffp_config.MaxPathLength = (int)1;//Max length is also only 1 for expand
				ffp_config.MaxCountPaths = (int)100000;//Change as needed
				ffp_config.ClosedPathPolicy = KGClosedPathPolicy.Allow;//Cycles are allowed
				ffp_config.TimeFilter = null;

				return ffp_config;
			}

			CIMFilteredFindPathsConfiguration FilteredExpandConfig(Map linkChart)
			{
				var ffp_config = ExpandConfig(linkChart);
				ffp_config.Name = "Filtered Expand";

				//Path filters
				var pathFilters = new List<CIMFilteredFindPathsPathFilter>();
				//For filtered expand we consider only some of the
				//relationship types - the "rest" are considered to be 
				//"filtered out"
				var path_filter = new CIMFilteredFindPathsPathFilter();
				path_filter.ItemTypeName = "ObservedIn";
				path_filter.ItemType = KGPathFilterItemType.Relationship;
				path_filter.FilterType = KGPathFilterType.IncludeOnly;
				pathFilters.Add(path_filter);
				ffp_config.PathFilters = pathFilters.ToArray();

				return ffp_config;
			}

			CIMFilteredFindPathsConfiguration ConnectConfig(Map linkChart)
			{
				var ffp_config = new CIMFilteredFindPathsConfiguration();
				ffp_config.Name = "Connect";

				//Configure Origin and Destination Entities
				var originEntities = new List<CIMFilteredFindPathsEntity>();
				var destEntities = new List<CIMFilteredFindPathsEntity>();

				//Get the ids of the selected features
				var map_sel = linkChart.GetSelection();
				//Get the KG layer in the link chart
				var kg_layer = linkChart.GetLayersAsFlattenedList()
					.OfType<KnowledgeGraphLayer>().First();

				Dictionary<string, List<object>> dict_uids = null;
				IReadOnlyList<string> entityTypeNames = null;

				if (map_sel.Count > 0)
				{
					//Get what is selected in the link chart
					var kg_id_set = map_sel.ToKnowledgeGraphIDSet();
					dict_uids = kg_id_set.ToUIDDictionary();
					entityTypeNames = kg_id_set.GetEntityRelationshipTypeNames().entityTypeNames;
					foreach (var entity_type_name in entityTypeNames)
					{
						//Add the selected entities to the list
						var uids = dict_uids[entity_type_name];
						foreach (var uid in uids)
						{
							var sel_entity = new CIMFilteredFindPathsEntity();
							sel_entity.EntityTypeName = entity_type_name;
							sel_entity.ID = KGUtils.Instance.FormatID(uid.ToString());
							originEntities.Add(sel_entity);
						}
					}
				}

				//Get everything currently in the link chart for the destinations
				var dest_entities = GetEntityTypeNames(kg_layer.GetDatastore());
				foreach (var dest_entity in dest_entities)
				{
					//Add all entity types in as destination types
					var dest_entity_ffp = new CIMFilteredFindPathsEntity();
					dest_entity_ffp.EntityTypeName = dest_entity;
					destEntities.Add(dest_entity_ffp);
				}

				//var kg_id_set_all = kg_layer.GetIDSet();
				//dict_uids = kg_id_set_all.ToUIDDictionary();
				//entityTypeNames = kg_id_set_all.GetEntityRelationshipTypeNames().entityTypeNames;

				//foreach (var entity_type_name in entityTypeNames)
				//{
				//	var uids = dict_uids[entity_type_name];
				//	foreach (var uid in uids)
				//	{
				//		var sel_entity = new CIMFilteredFindPathsEntity();
				//		sel_entity.EntityTypeName = entity_type_name;
				//		sel_entity.ID = KGUtils.Instance.FormatID(uid.ToString());
				//		destEntities.Add(sel_entity);
				//	}
				//}

				//Was anything selected? If not, set origins = destinations
				if (map_sel.Count == 0)
					originEntities = destEntities.ToList();

				ffp_config.OriginEntities = originEntities.ToArray();
				ffp_config.DestinationEntities = destEntities.ToArray();

				//Path filters
				ffp_config.PathFilters = [];
				//Traversal
				ffp_config.TraversalDirections = [];

				ffp_config.RelationshipCostProperty = "";
				ffp_config.DefaultRelationshipCost = 1.0;
				ffp_config.DefaultTraversalDirectionType = KGTraversalDirectionType.Any;
				ffp_config.EntityUsage = FilteredFindPathsEntityUsage.AnyOriginAnyDestination;
				ffp_config.PathMode = KGPathMode.Shortest;
				ffp_config.MinPathLength = (int)1;
				ffp_config.MaxPathLength = (int)1;//Max length is also only 1 for connect
				ffp_config.MaxCountPaths = (int)100000;//Change as needed
				ffp_config.ClosedPathPolicy = KGClosedPathPolicy.Allow;//Cycles are allowed
				ffp_config.TimeFilter = null;

				return ffp_config;
			}

			CIMFilteredFindPathsConfiguration FindBetweenConfig(Map linkChart)
			{
				var ffp_config = new CIMFilteredFindPathsConfiguration();
				ffp_config.Name = "Find Between";

				//Get the ids of the selected features
				var map_sel = linkChart.GetSelection();
				var kg_id_set = map_sel.ToKnowledgeGraphIDSet();

				var dict_uids_by_type = kg_id_set.ToUIDDictionary();
				var all_type_names = kg_id_set.GetEntityRelationshipTypeNames();

				//Configure Origin and Destination Entities
				var originEntities = new List<CIMFilteredFindPathsEntity>();
				var destEntities = new List<CIMFilteredFindPathsEntity>();

				//Add in origins and destinations
				foreach (var entity_type_name in all_type_names.entityTypeNames)
				{
					var uids = dict_uids_by_type[entity_type_name];
					foreach (var uid in uids)//Something has to be selected
					{
						//Add in the selected entities as the origins and destinations
						var sel_entity = new CIMFilteredFindPathsEntity();
						sel_entity.EntityTypeName = entity_type_name;
						sel_entity.ID = KGUtils.Instance.FormatID(uid.ToString());

						originEntities.Add(sel_entity);
						destEntities.Add(sel_entity);
					}
				}

				ffp_config.OriginEntities = originEntities.ToArray();
				ffp_config.DestinationEntities = destEntities.ToArray();

				//Path filters
				ffp_config.PathFilters = [];
				//Traversal
				ffp_config.TraversalDirections = [];

				ffp_config.RelationshipCostProperty = "";
				ffp_config.DefaultRelationshipCost = 1.0;
				ffp_config.DefaultTraversalDirectionType = KGTraversalDirectionType.Any;
				ffp_config.EntityUsage = FilteredFindPathsEntityUsage.AnyOriginAnyDestination;
				ffp_config.PathMode = KGPathMode.Shortest;
				ffp_config.MinPathLength = (int)2;//At least 2 for between
				ffp_config.MaxPathLength = (int)8;
				ffp_config.MaxCountPaths = (int)100000;//Change as needed
				ffp_config.ClosedPathPolicy = KGClosedPathPolicy.Allow;//Cycles allowed
				ffp_config.TimeFilter = null;

				return ffp_config;
			}
		}

		internal (bool isValid, string errorMsg) CanRunFFPAnalysisType(
			Map linkChart,
			FFP_Analysis_Type ffp_type)
		{
			var errorMsg = "";

			switch (ffp_type)
			{
				case FFP_Analysis_Type.FFP:
					break;
				case FFP_Analysis_Type.Expand:
				case FFP_Analysis_Type.FilteredExpand:
					{
						var map_sel = linkChart.GetSelection();
						if (map_sel.Count == 0)
						{
							errorMsg = "At least one entity must be selected to expand from";
							return (false, errorMsg);
						}
					}
					break;
				case FFP_Analysis_Type.Connect:
					break;
				case FFP_Analysis_Type.FindBetween:
					{
						//Get the ids of the selected features
						var map_sel = linkChart.GetSelection();
						var kg_id_set = map_sel.ToKnowledgeGraphIDSet();

						var dict_uids_by_type = kg_id_set.ToUIDDictionary();
						var entityTypeNames =
							kg_id_set.GetEntityRelationshipTypeNames().entityTypeNames;

						//At least 2 and no more than 10 entities -must- be selected
						int count_selected = 0;
						foreach (var entity_type_name in entityTypeNames)
						{
							var uids = dict_uids_by_type[entity_type_name];
							foreach (var uid in uids)//Something has to be selected
							{
								count_selected++;
								if (count_selected > 10)
								{
									errorMsg = "At least 2 entities and no more than 10 entities " +
														 "can be selected for Find Between";
									return (false, errorMsg);
								}
							}
						}
						if (count_selected < 2)
						{
							errorMsg = "At least 2 entities and no more than 10 entities " +
														 "can be selected for Find Between";
							return (false, errorMsg);
						}
					}
					break;
			}
			return (true, "");
		}

		#endregion

		#region FFP Results

		public void UpdateLinkChartWithResults(
			Map linkChart, KnowledgeGraphFilteredFindPathsResults results,
			FFP_Analysis_Type analysisType)
		{
			//Create a KG layer id set from the results
			var pathsEntitiesAndRelationships = results.ExtractPathsEntitiesAndRelationships(null);
			var expandResultIdSet = KnowledgeGraphLayerIDSet.FromKnowledgeGraphIDSet(
				pathsEntitiesAndRelationships.ToKnowledgeGraphIDSet(
					KGResultContentFromFFP.EntitiesAndRelationships));

			if (!linkChart.CanAppendToLinkChart(expandResultIdSet))
				return;//not compatible

			var kg_layer = linkChart.GetLayersAsFlattenedList().OfType<KnowledgeGraphLayer>().First();
			var oidDictExists = kg_layer.GetIDSet().ToOIDDictionary();

			var oidDict = expandResultIdSet.ToOIDDictionary();
			//Append the results to the link chart
			linkChart.AppendToLinkChart(expandResultIdSet);

			KnowledgeGraphIDSet kg_id_set_for_select = null;

			switch (analysisType)
			{
				case FFP_Analysis_Type.FFP:
					//get everything
					kg_id_set_for_select = kg_layer.GetIDSet().IDSet;
					//set root nodes if the algo is hierarchical top to bottom
					var lcView = MapView.Active;
					var algo = lcView.GetLinkChartLayout();
					if (algo == KnowledgeLinkChartLayoutAlgorithm.Hierarchical_TopToBottom
						|| algo == KnowledgeLinkChartLayoutAlgorithm.Hierarchical_BottomToTop)
					{
						//set root nodes
						var mmIdSet = MapMemberIDSet.FromKnowledgeGraphIDSet(
																				linkChart, pathsEntitiesAndRelationships.ToKnowledgeGraphIDSet(
																											KGResultContentFromFFP.OnlyPathsOriginEntities));
						lcView.SetRootNodes(mmIdSet);
					}
					break;
				case FFP_Analysis_Type.Expand:
				case FFP_Analysis_Type.FilteredExpand:
				case FFP_Analysis_Type.Connect:
					//For Expand and Connect we want just the newly added oids
					//to be selected
					var result_oid_dict = KGUtils.Instance.CombineOIDDicts(
						oidDict, oidDictExists, OIDCombineMethod.SubtractExisting);
					kg_id_set_for_select = KnowledgeGraphIDSet.FromDictionary(
															kg_layer.GetDatastore(), result_oid_dict);

					break;
				case FFP_Analysis_Type.FindBetween:
					//For Between we want (just) the "between" entity oids
					//no relationships
					var between_uid_dict = ExtractBetweenEntities(pathsEntitiesAndRelationships);
					kg_id_set_for_select = KnowledgeGraphIDSet.FromDictionary(
																		kg_layer.GetDatastore(), between_uid_dict);

					break;
			}

			var mm_id_set = MapMemberIDSet.FromKnowledgeGraphIDSet(
												linkChart, kg_id_set_for_select);

			SelectionSet selSet = SelectionSet.FromMapMemberIDSet(mm_id_set);
			linkChart.SetSelection(selSet);
		}

		public async Task CreateLinkChartWithResultsAsync(
			KnowledgeGraph kg,
			KnowledgeGraphFilteredFindPathsResults results)
		{

			var pathsEntitiesAndRelationships = results.ExtractPathsEntitiesAndRelationships(null);

			//Create a KG layer id set
			var kgLayerIdSet = KnowledgeGraphLayerIDSet.FromKnowledgeGraphIDSet(
				pathsEntitiesAndRelationships.ToKnowledgeGraphIDSet(
					KGResultContentFromFFP.EntitiesAndRelationships));

			//Create a brand new link chart with the results and show it
			var linkChart = MapFactory.Instance.CreateLinkChart(
												"FFP Sample", kg, kgLayerIdSet);

			var mapPane = await FrameworkApplication.Panes.CreateMapPaneAsync(linkChart);
			var linkChartView = mapPane.MapView;
			var map = linkChartView.Map;

			//Change layout algo
			await linkChartView.SetLinkChartLayoutAsync(
				KnowledgeLinkChartLayoutAlgorithm.Hierarchical_TopToBottom);

			//Set root nodes
			var mmIdSet = MapMemberIDSet.FromKnowledgeGraphIDSet(
																	map, pathsEntitiesAndRelationships.ToKnowledgeGraphIDSet(
																								KGResultContentFromFFP.OnlyPathsOriginEntities));
			linkChartView.SetRootNodes(mmIdSet);
		}

		private Dictionary<string, List<object>> ExtractBetweenEntities(
			PathsEntitiesAndRelationships per)
		{

			var origin_dest_uids = new List<string>();
			var between_uids = new Dictionary<string, List<object>>();

			//Origin Entities
			foreach (var idx in per.PathsOriginEntitiesUIDsIndexes)
			{
				var uid = KGUtils.Instance.FormatID(per.EntitiesUIDs[idx].ToString());
				origin_dest_uids.Add(uid);
			}
			//Destination Entities
			foreach (var idx in per.PathsDestinationEntitiesUIDsIndexes)
			{
				var uid = KGUtils.Instance.FormatID(per.EntitiesUIDs[idx].ToString());
				origin_dest_uids.Add(uid);
			}
			//Between Entities
			var idx2 = 0;
			foreach (var raw_uid in per.EntitiesUIDs)
			{
				var uid = KGUtils.Instance.FormatID(raw_uid.ToString());
				if (!origin_dest_uids.Contains(uid))
				{
					var entity_type_name = per.EntityTypeNames[per.EntityTypes[idx2]];
					if (!between_uids.ContainsKey(entity_type_name))
						between_uids[entity_type_name] = new List<object>();
					between_uids[entity_type_name].Add(raw_uid);
				}
				idx2++;
			}
			return between_uids;
		}

		#endregion
	}
}
