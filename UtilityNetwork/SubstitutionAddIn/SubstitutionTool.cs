using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Trace;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Core.Conda;
using ArcGIS.Desktop.Internal.Mapping.TOC;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static ArcGIS.Desktop.Internal.Core.PortalTrafficDataService.PortalDescriptionResponse;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Element = ArcGIS.Core.Data.UtilityNetwork.Element;
using Geometry = ArcGIS.Core.Geometry.Geometry;
using CodedValueDomain = ArcGIS.Core.Data.CodedValueDomain;
using ArcGIS.Core.Internal.CIM;
using NetworkSource = ArcGIS.Core.Data.UtilityNetwork.NetworkSource;
using Field = ArcGIS.Core.Data.Field;
using NetworkAttribute = ArcGIS.Core.Data.UtilityNetwork.NetworkAttribute;
using QueryFilter = ArcGIS.Core.Data.QueryFilter;
using Table = ArcGIS.Core.Data.Table;
using ArcGIS.Desktop.Internal.Framework;
using FrameworkApplication = ArcGIS.Desktop.Framework.FrameworkApplication;

namespace SubstitutionAddIn
{
  //   Copyright 2019 Esri
  //   Licensed under the Apache License, Version 2.0 (the "License");
  //   you may not use this file except in compliance with the License.
  //   You may obtain a copy of the License at

  //       https://www.apache.org/licenses/LICENSE-2.0

  //   Unless required by applicable law or agreed to in writing, software
  //   distributed under the License is distributed on an "AS IS" BASIS,
  //   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  //   See the License for the specific language governing permissions and
  //   limitations under the License. 

  internal class SubstitutionTool : MapTool
  {
    MapView _activeMap = null;
    FeatureLayer _pointFeatureLayer = null;
    Element _pointElement = null;
    string _fieldValue = null;
    string _currentValue = null;
    IList<string> _domainValues = null;
    string _fieldName = null;
    SortedList<object, string> _codeValuePairs = null;
    IList<Tier> _tiersWithResults = null;

    public SubstitutionTool()
    {
      IsSketchTool = true;
      SketchType = SketchGeometryType.Point;
      SketchOutputMode = SketchOutputMode.Map;
    }

    protected override Task OnToolActivateAsync(bool active)
    {
      return base.OnToolActivateAsync(active);
    }

    protected async override Task<bool> OnSketchCompleteAsync(Geometry geometry)
    {
      try
      {
        // user clicks on a feature
        // check that its assettype has the 'Attribute Substitution' category
        // find the network attribute that has a Network Attribute to Substitute set on it
        // find the field for the substitution by checking assignments
        // check if it already has a value
        // show the form
        // List of values(existing value set if there was one) 
        //found from the domain assigned to the field
        // checkbox to make permanent
        // OK button

        // Once the ok button is pressed
        // if the checkbox isn't set, just write the substitution value into the features substitution field
        // If the checkbox is set
        // Loop through tiers and attempt to trace to Find the controller(s)
        // For any controllers found, run update subnetwork
        // get the propagation information to find the burn in field(propagated attribute)
        // run a downstream trace from the selected feature
        // for each feature found
        // set the field for the network attribute to substitute = propagated attribute
        // remove the substitution value from the selected feature

        bool foundFeature = false;
        MapView activeMap = MapView.Active;
        if (activeMap == null)
        {
          // Shouldn't happen
          MessageBox.Show("No active map");
          return false;
        }
        _activeMap = activeMap;

        await QueuedTask.Run(() =>
        {
          try
          {
            SelectionSet featureSelections = activeMap.GetFeaturesEx(geometry);
            Dictionary<MapMember, List<long>> featureLayerToObjectIDMappings = featureSelections.ToDictionary();
            if (featureLayerToObjectIDMappings.Count == 0)
            {
              MessageBox.Show("Select a utility network feature.");
              return;
            }

            foreach (KeyValuePair<MapMember, List<long>> featureLayerToObjectIDMapping in featureLayerToObjectIDMappings)
            {
              if (featureLayerToObjectIDMapping.Key is FeatureLayer featureLayer)
              {
                using (FeatureClass featureClass = featureLayer.GetFeatureClass())
                {
                  // Make sure this is a point feature class
                  if (!Utilities.IsDesiredShapeType(featureClass, GeometryType.Point))
                    continue;

                  using (UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(featureClass))
                  {
                    if (utilityNetwork == null)
                      continue;

                    // Make sure the the feature class is the Device or Junction class in the utility network
                    if (!IsPointNetworkSource(utilityNetwork, featureClass.GetName()))
                      continue;

                    if (featureLayerToObjectIDMapping.Value.Count > 1)
                    {
                      MessageBox.Show("Select only one utility network point feature.");
                      return;
                    }

                    GetPointElement(utilityNetwork, featureClass, featureLayerToObjectIDMapping.Value[0], out Element pointElement);

                    if (!CheckAssetType(pointElement.AssetType) == true)
                      continue;

                    CodedValueDomain codedValueDomain = null;
                    string currentValue = null;

                    GetSubstitionValue(utilityNetwork, pointElement.NetworkSource, pointElement, out codedValueDomain, out currentValue);

                    SortedList<object, string> codeValuePairs = codedValueDomain.GetCodedValuePairs();

                    _codeValuePairs = codeValuePairs;

                    _domainValues = new List<string>();

                    // allow the user to set the value back to null
                    _domainValues.Add("<Null>");
                    foreach (var item in codeValuePairs.Values)
                    {
                      _domainValues.Add(item.ToString());
                    }

                    if (currentValue != null)
                    {
                      foreach (var codeValuePair in codeValuePairs)
                      {
                        if (codeValuePair.Key.ToString() == currentValue)
                        {
                          _currentValue = codeValuePair.Value;
                          break;
                        }
                      }
                    }
                    foundFeature = true;
                    _pointFeatureLayer = featureLayer;
                    _pointElement = pointElement;
                  }
                }
              }
            }
            if (_pointFeatureLayer == null)
            {
              MessageBox.Show("Select a utility network point feature from the Device or Junction class that has the Attribute Substitution category assigned.");
            }

          }
          catch (Exception e)
          {
            MessageBox.Show($"Exception: {e.Message}");
          }
        });

        if (foundFeature == true)
        {
          // Open the dialog so the user can select a substitution value and specify if it is permanent or not
          Substitution substitution = new Substitution(_currentValue, _domainValues);
          {
            substitution.Owner = FrameworkApplication.Current.MainWindow;
          };
          substitution.ShowDialog();


          using (ProgressDialog progressDialog = new ProgressDialog("Performing substitution"))
          {
            string errorMessage = string.Empty;

            progressDialog.Show();

            await QueuedTask.Run(async () =>
            {
              UtilityNetwork utilityNetwork = Utilities.GetUtilityNetwork(_pointFeatureLayer.GetFeatureClass());
              if (substitution.IsPermanent != null && substitution.ChosenSubstitution != null)
              {
                var options = ApplicationOptions.EditingOptions;
                options.AutomaticallySaveEdits = true;

                // get the chosen substitution value and set it on the selected feature
                string newValue = substitution.ChosenSubstitution.FirstOrDefault();
                SetSubstitutionValue(utilityNetwork, _pointElement.NetworkSource, _pointElement, newValue);

                // check if this is going to be a permanent substitution
                bool boolIsPermanent = substitution.IsPermanent.FirstOrDefault();
                if (boolIsPermanent == true)
                {
                  // Loop through tiers and attempt to trace to Find the controller(s)
                  // For any controllers found, run update subnetwork
                  // get the propagation information to find the burn in field(propagated attribute)
                  // run a downstream trace from the selected feature
                  // for each feature found
                  // set the field for the network attribute to substitute = propagated attribute
                  // remove the substitution value from the selected feature
                  ValidateNetwork(utilityNetwork, _activeMap.Extent);

                  RunUpdateSubnetwork(utilityNetwork);

                  if (_tiersWithResults.Count > 0)
                  {
                    UpdateFeatureValues(utilityNetwork);

                    SetSubstitutionValue(utilityNetwork, _pointElement.NetworkSource, _pointElement, null);
                  }
                  else
                  {
                    MessageBox.Show("Error updating subnetworks. Please validate the extent of the subnetwork and try again.");
                  }
                }
              }

            });
          }
        }
        _fieldName = null;
        _activeMap = null;
        _pointFeatureLayer = null;
        _pointElement = null;
        _fieldValue = null;
        _currentValue = null;
        _domainValues = null;
        _codeValuePairs = null;
        _tiersWithResults = null;


      }
      catch (Exception ex)
      {
        MessageBox.Show($@"Error in SubstitutionButton::OnClick: {ex.Message}");
      }

      return true;
    }

    private void UpdateFeatureValues(UtilityNetwork utilityNetwork)
    {
      IReadOnlyList<Result> results = null;
      foreach (Tier tier in _tiersWithResults)
      {
        results = null;
        TraceConfiguration traceConfiguration = tier.GetTraceConfiguration();

        TraceManager traceManager = utilityNetwork.GetTraceManager();
        DownstreamTracer tracer = traceManager.GetTracer<DownstreamTracer>();

        List<Element> startingPointList = new List<Element>();
        startingPointList.Add(_pointElement);

        TraceArgument traceArgument = new TraceArgument(startingPointList);

        traceArgument.Configuration = traceConfiguration;
        try
        {
          results = tracer.Trace(traceArgument);
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.Message);
        }

        if (results != null)
        {
          List<Element> deviceElements = new List<Element>();
          List<Element> junctionElements = new List<Element>();
          List<Element> lineElements = new List<Element>();
          List<Element> junctionObjectElements = new List<Element>();
          List<Element> edgeObjectElements = new List<Element>();

          // loop through each result and update the features
          foreach (Result result in results)
          {
            if (result is ElementResult er)
            {
              IReadOnlyList<Element> elements = er.Elements;
              foreach (Element element2 in elements)
              {
                if (element2.NetworkSource.UsageType == SourceUsageType.Device)
                  deviceElements.Add(element2);
                if (element2.NetworkSource.UsageType == SourceUsageType.Junction)
                  junctionElements.Add(element2);
                if (element2.NetworkSource.UsageType == SourceUsageType.Line)
                  lineElements.Add(element2);
                if (element2.NetworkSource.UsageType == SourceUsageType.JunctionObject)
                  junctionObjectElements.Add(element2);
                if (element2.NetworkSource.UsageType == SourceUsageType.EdgeObject)
                  edgeObjectElements.Add(element2);
              }
            }
          }

          // find the two fields we are dealing with - need to get from propagators
          Field burnInField = null;
          Field propagatedField = null;
          IReadOnlyList<Propagator> propagators = traceConfiguration.Propagators;
          foreach (Propagator propagator in propagators)
          {
            burnInField = propagator.PersistedField;
            NetworkAttribute networkAttribute = propagator.NetworkAttribute;
            IReadOnlyList<NetworkAttributeAssignment> assignments = networkAttribute.Assignments;
            foreach (NetworkAttributeAssignment assignment in assignments)
            {
              propagatedField = assignment.Field;
              break;
            }
          }

          // get the rows and update the propagateField to equal the burnInField
          if (deviceElements.Count > 0)
            UpdateFeatures(utilityNetwork, deviceElements[0].NetworkSource, deviceElements, SourceUsageType.Device, burnInField, propagatedField);
          if (junctionElements.Count > 0)
            UpdateFeatures(utilityNetwork, junctionElements[0].NetworkSource, junctionElements, SourceUsageType.Junction, burnInField, propagatedField);
          if (lineElements.Count > 0)
            UpdateFeatures(utilityNetwork, lineElements[0].NetworkSource, lineElements, SourceUsageType.Line, burnInField, propagatedField);
          if (junctionObjectElements.Count > 0)
            UpdateFeatures(utilityNetwork, junctionObjectElements[0].NetworkSource, junctionObjectElements, SourceUsageType.JunctionObject, burnInField, propagatedField);
          if (edgeObjectElements.Count > 0)
            UpdateFeatures(utilityNetwork, edgeObjectElements[0].NetworkSource, edgeObjectElements, SourceUsageType.EdgeObject, burnInField, propagatedField);

        }
      }

    }

    private async void UpdateFeatures(UtilityNetwork utilityNetwork, NetworkSource networkSource, IList<Element> elements, SourceUsageType usageType, Field burnInField, Field propagatedField)
    {
      // get a list of objectid's
      List<long> queryFilterObjectIDs = new List<long>();
      foreach (Element element in elements)
      {
        queryFilterObjectIDs.Add(element.ObjectID);
      }

      await QueuedTask.Run(() =>
      {
        QueryFilter filter = new QueryFilter()
        {
          ObjectIDs = queryFilterObjectIDs
        };

        Table table = utilityNetwork.GetTable(networkSource);

        using (RowCursor rowCursor = table.Search(filter, false))
        {
          while (rowCursor.MoveNext())
          {
            using (Row row = rowCursor.Current)
            {
              EditOperation editOperation = new EditOperation()
              {
                Name = "set new feature value",
              };
              string errorMessage = "";
              editOperation.Callback((context) =>
                    {
                        try
                        {
                          int intBurnInFieldPosition = row.FindField(burnInField.Name);
                          int intPropagatedFieldPosition = row.FindField(propagatedField.Name);
                          row[intPropagatedFieldPosition] = row[intBurnInFieldPosition];
                          row.Store();
                          context.Invalidate(row);
                        }
                        catch (Exception e)
                        {
                          errorMessage = $"Exception: {e.Message}";
                        }
                      }, _pointFeatureLayer);

              if (editOperation.IsEmpty || !editOperation.Execute())
              {
                //return;
              }
              if (errorMessage != "")
              {
                MessageBox.Show(errorMessage);
              }

            }
          }
        }
        return 1;
      });
    }

    private void ValidateNetwork(UtilityNetwork utilityNetwork, Geometry extent)
    {
      utilityNetwork.ValidateNetworkTopologyInEditOperation(extent);
    }

    private async void RunUpdateSubnetwork(UtilityNetwork utilityNetwork)
    {
      // for each domain network, get tiers, for each tier attempt to do a controller trace            
      List<Tier> tiersWithResults = new List<Tier>();
      IReadOnlyList<Result> results = null;
      UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition();
      IReadOnlyList<DomainNetwork> domainNetworks = utilityNetworkDefinition.GetDomainNetworks();
      foreach (DomainNetwork domainNetwork in domainNetworks)
      {
        IReadOnlyList<Tier> tiers = domainNetwork.Tiers;
        foreach (Tier tier in tiers)
        {
          results = null;
          TraceConfiguration traceConfiguration = tier.GetTraceConfiguration();

          TraceManager traceManager = utilityNetwork.GetTraceManager();
          SubnetworkControllerTracer tracer = traceManager.GetTracer<SubnetworkControllerTracer>();

          List<Element> startingPointList = new List<Element>();
          startingPointList.Add(_pointElement);

          TraceArgument traceArgument = new TraceArgument(startingPointList);

          traceArgument.Configuration = traceConfiguration;
          try
          {
            results = tracer.Trace(traceArgument);
          }
          catch
          {
            // do nothing
          }

          if (results != null)
          {
            if (results.Count > 0)
            {
              foreach (Result result in results)
              {
                if (result is ElementResult er)
                {
                  IReadOnlyList<Element> elements = er.Elements;
                  foreach (Element element2 in elements)
                  {
                    if (element2.NetworkSource.UsageType == SourceUsageType.Device)
                    {
                      // query to get the features subnetwork name(s)
                      QueryFeature(utilityNetwork, element2.NetworkSource, element2.ObjectID, "SubnetworkName");

                      // get the subnetwork
                      SubnetworkManager subnetworkManager = utilityNetwork.GetSubnetworkManager();

                      if (_fieldValue.Contains("::"))
                      {
                        // have to try all subnetworks
                        string[] fieldValues = _fieldValue.Split("::");
                        foreach (string fieldValue in fieldValues)
                        {
                          Subnetwork subnetwork = subnetworkManager.GetSubnetwork(fieldValue);
                          // save any current edits and attempt to update the subnetwork
                          await Project.Current.SaveEditsAsync();
                          if (subnetwork.Tier.Name != tier.Name)
                          {
                            continue;
                          }
                          if (subnetwork.GetState() == SubnetworkStates.Dirty)
                          {
                            try
                            {
                              subnetwork.Update(traceConfiguration);
                              if (!tiersWithResults.Contains(tier))
                              {
                                tiersWithResults.Add(tier);
                              }
                            }
                            catch (Exception ex)
                            {
                              MessageBox.Show(ex.Message);
                            }
                          }
                          else
                          {
                            if (!tiersWithResults.Contains(tier))
                            {
                              tiersWithResults.Add(tier);
                            }
                          }
                        }
                      }
                      else
                      {
                        Subnetwork subnetwork = subnetworkManager.GetSubnetwork(_fieldValue);
                        // save any current edits and attempt to update the subnetwork
                        await Project.Current.SaveEditsAsync();
                        try
                        {
                            subnetwork.Update(traceConfiguration);

                            tiersWithResults.Add(tier);
                        }
                        catch
                        {
                            // subnetwork didn't need to be updated
                            tiersWithResults.Add(tier);
                        }
                      }

                    }
                  }
                }
              }
            }
          }
        }
      }

      _tiersWithResults = tiersWithResults;

    }

    private async void SetSubstitutionValue(UtilityNetwork utilityNetwork, NetworkSource networkSource, Element element, string newValue)
    {
      // set the substitution filed value of clicked feature to the newvalue
      object codeValuePairObject = null;
      foreach (var codeValuePair in _codeValuePairs)
      {
        if (codeValuePair.Value == newValue)
        {
          codeValuePairObject = codeValuePair.Key;
          break;
        }
      }

      await QueuedTask.Run(() =>
      {
        QueryFilter filter = new QueryFilter();
        filter.WhereClause = "OBJECTID = " + element.ObjectID;
        Table table = utilityNetwork.GetTable(networkSource);

        using (RowCursor rowCursor = table.Search(filter, false))
        {
          while (rowCursor.MoveNext())
          {
            using (Row row = rowCursor.Current)
            {
              EditOperation editOperation = new EditOperation()
              {
                Name = "set new feature value",
              };
              string errorMessage = "";
              editOperation.Callback((context) =>
                    {
                        try
                        {
                          int intFieldPosition = row.FindField(_fieldName);
                          row[intFieldPosition] = codeValuePairObject;
                          row.Store();
                          context.Invalidate(row);
                        }
                        catch (Exception e)
                        {
                          errorMessage = $"Exception: {e.Message}";
                        }
                      }, _pointFeatureLayer);

              if (editOperation.IsEmpty || !editOperation.Execute())
              {
                //return;
              }
              if (errorMessage != "")
              {
                MessageBox.Show(errorMessage);
              }

            }
          }
        }
        return 1;
      });
    }

    private void GetSubstitionValue(UtilityNetwork utilityNetwork, NetworkSource networkSource, Element element, out CodedValueDomain codedValueDomain, out string currentValue)
    {
      // find the network attribute that is set with IsSubstitution = true
      // return the current value for the clicked feature as well as the domain values
      codedValueDomain = null;
      currentValue = null;
      UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition();
      IReadOnlyList<NetworkAttribute> networkAttributes = utilityNetworkDefinition.GetNetworkAttributes();
      foreach (NetworkAttribute attribute in networkAttributes)
      {
        if (attribute.IsSubstitution == true)
        {
          IReadOnlyList<NetworkAttributeAssignment> assignments = attribute.Assignments;
          foreach (NetworkAttributeAssignment assignment in assignments)
          {
            if (assignment.NetworkSource.Name == networkSource.Name)
            {
              Field field = assignment.Field;
              codedValueDomain = field.GetDomain() as CodedValueDomain;
              _fieldName = field.Name;

              QueryFeature(utilityNetwork, networkSource, element.ObjectID, field.Name);
              if (_fieldValue != null)
              {
                currentValue = _fieldValue;
                break;
              }
            }
          }
        }
      }
    }


    private async void QueryFeature(UtilityNetwork utilityNetwork, NetworkSource networkSource, long objectID, string fieldName)
    {
      var count = await QueuedTask.Run(() =>
      {
        QueryFilter filter = new QueryFilter();
        filter.WhereClause = "OBJECTID = " + objectID;
        Table table = utilityNetwork.GetTable(networkSource);

        using (RowCursor rowCursor = table.Search(filter, false))
        {
          while (rowCursor.MoveNext())
          {
            using (Row row = rowCursor.Current)
            {
              if (row.FindField(fieldName) != -1)
              {
                if (Convert.ToString(row[fieldName]) != "")
                {
                  _fieldValue = Convert.ToString(row[fieldName]);
                }
              }
            }
          }
        }
        return 1;
      });
    }


    private bool CheckAssetType(AssetType assetType)
    {
      // check to make sure the asset type has the attribute substitution category assigned
      bool foundCategory = false;
      IReadOnlyList<string> assignedCategoryList = assetType.CategoryList;
      foreach (string assignedCategory in assignedCategoryList)
      {
        if (assignedCategory == Utilities._categoryString)
        {
          foundCategory = true;
          break;
        }
      }

      return foundCategory;
    }

    private void GetPointElement(UtilityNetwork utilityNetwork, FeatureClass pointFeatureClass, long pointObjectID, out Element point)
    {
      point = null;
      QueryFilter queryFilter = new QueryFilter()
      {
        ObjectIDs = new List<long>() { pointObjectID }
      };

      using (RowCursor rowCursor = pointFeatureClass.Search(queryFilter, false))
      {
        if (!rowCursor.MoveNext())
        {
          return;
        }

        using (Row row = rowCursor.Current)
        {
          try
          {
            point = utilityNetwork.CreateElement(row);
          }
          catch (Exception e)
          {
            MessageBox.Show(e.Message);
            return;
          }
        }
      }
    }

    private bool IsPointNetworkSource(UtilityNetwork utilityNetwork, string featureClassName)
    {
      try
      {
        // This is inside a try/catch because the feature class may not be a network source (e.g. service territory).

        using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
        using (NetworkSource networkSource = utilityNetworkDefinition.GetNetworkSource(featureClassName))
        {
          if (networkSource.UsageType == SourceUsageType.Device || networkSource.UsageType == SourceUsageType.Junction)
          {
            return true;
          }
          else
          {
            return false;
          }
        }
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message);
        return false;
      }
    }
  }
}
