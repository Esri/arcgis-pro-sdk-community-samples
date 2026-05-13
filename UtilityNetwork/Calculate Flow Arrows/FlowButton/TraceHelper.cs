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
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Mapping;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using ArcGIS.Core.Data.UtilityNetwork.Trace;

namespace Calculate_Flow_Arrows.FlowButton
{
    internal class TraceHelper
    {

        internal static (string fieldName, long statusValue) GetNetworkStatusAttribute(Tier tier)
        {
            string statusAttributeName = string.Empty;
            long statusValue = 1;
            var traceConfiguration = tier.GetTraceConfiguration();
            if (traceConfiguration != null && traceConfiguration.Traversability != null)
            {
                var traversability = traceConfiguration.Traversability;

                if (traceConfiguration.Propagators != null &&
                traceConfiguration.Propagators.Count > 0)
                    Debug.WriteLine("Flow arrows do not reflect phasing and propagation.");

                var barriers = new Stack<ArcGIS.Core.Data.UtilityNetwork.Trace.Condition>();
                barriers.Push(traversability.Barriers);

                while (barriers.Count > 0)
                {
                    var barrier = barriers.Pop();

                    #region Handle expressions

                    if (barrier is NetworkAttributeComparison networkAttributeComparison)
                    {
                        // Check for a condition barrier with the word Status or Enabled in it, that isn't lifecycle or construction related
                        if ((networkAttributeComparison.NetworkAttribute.Name.Contains("Status", StringComparison.InvariantCultureIgnoreCase)||
                             networkAttributeComparison.NetworkAttribute.Name.Contains("Enabled", StringComparison.InvariantCultureIgnoreCase))
                            &&
                            !networkAttributeComparison.NetworkAttribute.Name.Contains("Lifecycle", StringComparison.InvariantCultureIgnoreCase)
                            &&
                            !networkAttributeComparison.NetworkAttribute.Name.Contains("Construction", StringComparison.InvariantCultureIgnoreCase))
                        {
                            statusAttributeName = networkAttributeComparison.NetworkAttribute.Name;

                            if (networkAttributeComparison.Operator is Operator.Equal)
                                statusValue = Convert.ToInt64(networkAttributeComparison.Value);
                            break;
                        }
                    }

                    #region Or Expression

                    if (barrier is Or orExpression)
                    {
                        // Check the right hand expression
                        if (orExpression.LeftExpression is Or)
                            barriers.Push(orExpression.LeftExpression);
                        else if (orExpression.LeftExpression is And)
                            barriers.Push(orExpression.LeftExpression);
                        else if (orExpression.LeftExpression is NetworkAttributeComparison attributeComparison)
                        {
                            if ((attributeComparison.NetworkAttribute.Name.Contains("Status", StringComparison.InvariantCultureIgnoreCase)||
                                 attributeComparison.NetworkAttribute.Name.Contains("Enabled", StringComparison.InvariantCultureIgnoreCase))
                                &&
                                !attributeComparison.NetworkAttribute.Name.Contains("Lifecycle", StringComparison.InvariantCultureIgnoreCase)
                                &&
                                !attributeComparison.NetworkAttribute.Name.Contains("Construction", StringComparison.InvariantCultureIgnoreCase))
                            {
                                statusAttributeName = attributeComparison.NetworkAttribute.Name;

                                if (attributeComparison.Operator is Operator.Equal)
                                    statusValue = Convert.ToInt64(attributeComparison.Value);
                                break;
                            }
                        }

                        // Check the right hand expression
                        if (orExpression.RightExpression is Or)
                            barriers.Push(orExpression.RightExpression);
                        else if (orExpression.RightExpression is And)
                            barriers.Push(orExpression.RightExpression);
                        else if (orExpression.RightExpression is NetworkAttributeComparison attributeComparison)
                        {
                            if ((attributeComparison.NetworkAttribute.Name.Contains("Status", StringComparison.InvariantCultureIgnoreCase)||
                                 attributeComparison.NetworkAttribute.Name.Contains("Enabled", StringComparison.InvariantCultureIgnoreCase))
                                &&
                                !attributeComparison.NetworkAttribute.Name.Contains("Lifecycle", StringComparison.InvariantCultureIgnoreCase)
                                &&
                                !attributeComparison.NetworkAttribute.Name.Contains("Construction", StringComparison.InvariantCultureIgnoreCase))
                            {
                                statusAttributeName = attributeComparison.NetworkAttribute.Name;

                                if (attributeComparison.Operator is Operator.Equal)
                                    statusValue = Convert.ToInt64(attributeComparison.Value);
                                break;
                            }
                        }
                    }

                    #endregion

                    #region And Expression

                    if (barrier is And andExpression)
                    {
                        // Check the right hand expression
                        if (andExpression.LeftExpression is Or)
                            barriers.Push(andExpression.LeftExpression);
                        else if (andExpression.LeftExpression is And)
                            barriers.Push(andExpression.LeftExpression);
                        else if (andExpression.LeftExpression is NetworkAttributeComparison attributeComparison)
                        {
                            if ((attributeComparison.NetworkAttribute.Name.Contains("Status", StringComparison.InvariantCultureIgnoreCase)||
                                 attributeComparison.NetworkAttribute.Name.Contains("Enabled", StringComparison.InvariantCultureIgnoreCase))
                                &&
                                !attributeComparison.NetworkAttribute.Name.Contains("Lifecycle", StringComparison.InvariantCultureIgnoreCase)
                                &&
                                !attributeComparison.NetworkAttribute.Name.Contains("Construction", StringComparison.InvariantCultureIgnoreCase))
                            {
                                statusAttributeName = attributeComparison.NetworkAttribute.Name;

                                if (attributeComparison.Operator is Operator.Equal)
                                    statusValue = Convert.ToInt64(attributeComparison.Value);
                                break;
                            }
                        }

                        // Check the right hand expression
                        if (andExpression.RightExpression is Or)
                            barriers.Push(andExpression.RightExpression);
                        else if (andExpression.RightExpression is And)
                            barriers.Push(andExpression.RightExpression);
                        else if (andExpression.RightExpression is NetworkAttributeComparison attributeComparison)
                        {
                            if ((attributeComparison.NetworkAttribute.Name.Contains("Status", StringComparison.InvariantCultureIgnoreCase)||
                                 attributeComparison.NetworkAttribute.Name.Contains("Enabled", StringComparison.InvariantCultureIgnoreCase))
                                &&
                                !attributeComparison.NetworkAttribute.Name.Contains("Lifecycle", StringComparison.InvariantCultureIgnoreCase)
                                &&
                                !attributeComparison.NetworkAttribute.Name.Contains("Construction", StringComparison.InvariantCultureIgnoreCase))
                            {
                                statusAttributeName = attributeComparison.NetworkAttribute.Name;

                                if (attributeComparison.Operator is Operator.Equal)
                                    statusValue = Convert.ToInt64(attributeComparison.Value);
                                break;
                            }
                        }
                    }

                    #endregion

                    #endregion

                }
            }

            return (statusAttributeName, statusValue);
        }

        internal static List<string> ExportTier(Map activeMap, string tierName, IEnumerable<string> networkAttributeNames = null, string outputDirectory = null)
        {
            using var utilityNetwork = Helper.GetFirstUtilityNetworkFromMap(activeMap);
            if (utilityNetwork == null)
                throw new Exception("No utility network found in map.");

            var utilityNetworkDefinition = utilityNetwork.GetDefinition();
            Tier targetTier = null;
            foreach (var domainNetwork in utilityNetworkDefinition.GetDomainNetworks())
                foreach (var tier in domainNetwork.Tiers)
                {
                    if (tier.Name.Equals(tierName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        targetTier = tier;
                        break;
                    }
                }

            if (targetTier == null)
                throw new Exception("Unable to find desired tier: " + tierName);

            if(networkAttributeNames == null)
            {
                var statusInfo = GetNetworkStatusAttribute(targetTier);
                if (!string.IsNullOrEmpty(statusInfo.fieldName))
                    networkAttributeNames = [statusInfo.fieldName];
            }

            if (string.IsNullOrEmpty(outputDirectory))
            {
                var saveFileDialog = new OpenFolderDialog
                {
                    Title = "JSON Export Folder"
                };
                if (!saveFileDialog.ShowDialog().Value)
                    return null;
                outputDirectory = saveFileDialog.FolderName;
            }

            return ExportTier(utilityNetwork, targetTier, networkAttributeNames, outputDirectory);
        }

        internal static List<string> ExportTier(UtilityNetwork utilityNetwork, Tier tier, IEnumerable<string> networkAttributeNames = null, string outputDirectory = null)
        {
            var utilityNetworkDefinition = utilityNetwork.GetDefinition();

            var networkAttributes = networkAttributeNames == null
                ? null
                : utilityNetworkDefinition.GetNetworkAttributes()
                    .Where(networkAttribute => networkAttributeNames.Contains(networkAttribute.Name));

            var outputFiles = new List<string>();

            var subnetworkManager = utilityNetwork.GetSubnetworkManager();
            var subnetworkExportOptions = new SubnetworkExportOptions
            {
                // Note::We should be able to set these parameters at the result type level
                // There is no reason to inlclude geometry in BOTH the features and the connectivity
                // Only include geometry in the features section, then refer back to that when parsing connectivity (if you need to)
                IncludeDomainDescriptions = true,
                IncludeGeometry = true,
                SubnetworkExportResultTypes = new List<SubnetworkExportResultType>
                    {
                        SubnetworkExportResultType.Features,
                        SubnetworkExportResultType.Connectivity,
                        SubnetworkExportResultType.ContainmentAndAttachment
                    },
            };

            if (networkAttributeNames != null)
                subnetworkExportOptions.ResultNetworkAttributes = networkAttributes.ToList();

            var cleanSubnetworks = subnetworkManager.GetSubnetworks(tier, SubnetworkStates.Clean);
            foreach (var subnetwork in cleanSubnetworks.OrderBy(subnetwork => subnetwork.Name))
            {
                var subnetworkName = subnetwork.Name;
                var outputFilePath = Path.Combine(outputDirectory, subnetworkName + ".json");
                if (ExportSubnetwork(subnetworkExportOptions, subnetwork, outputFilePath))
                    outputFiles.Add(outputFilePath);
            }

            return outputFiles;
        }

        #region Export Dirty Subnetworks
        /// <summary>
        /// Exports a dirty subnetwork using the Trace export functionality. This should only be used on tiers that don't manage state.
        /// </summary>
        /// <param name="utilityNetwork">Utility Network</param>
        /// <param name="subnetwork">Subnetwork</param>
        /// <param name="outputFilePath">Output file path</param>
        /// <param name="networkAttributeNames">Network attribute names to export</param>
        /// <returns></returns>
        internal static bool ExportDirtySubnetwork(UtilityNetwork utilityNetwork, Subnetwork subnetwork, string outputFilePath, IEnumerable<string> networkAttributeNames = null)
        {
            var utilityNetworkDefinition = utilityNetwork.GetDefinition();

            var networkAttributes = networkAttributeNames == null
                ? null
                : utilityNetworkDefinition.GetNetworkAttributes()
                    .Where(networkAttribute => networkAttributeNames.Contains(networkAttribute.Name))
                    .Select(networkAttribute => networkAttribute.Name);

            var traceArguments = new TraceArgument(subnetwork);

            traceArguments.ResultOptions = new ResultOptions
            {
                IncludeGeometry = true,
                NetworkAttributes = networkAttributes.ToList(),
            };
            traceArguments.ResultTypes = new List<ResultType>
            {
                ResultType.Feature,
                ResultType.Connectivity,
            };

            // The TraceArgument must be set with a TraceConfiguration object with a valid DomainNetwork for the subnetwork-based SubnetworkTracer.
            traceArguments.Configuration = subnetwork.Tier.GetTraceConfiguration();

            var traceExportOptions = new TraceExportOptions()
            {
                IncludeDomainDescriptions = false,
            };

            return ExportDirtySubnetwork(utilityNetwork, traceArguments, traceExportOptions, subnetwork, outputFilePath);
        }

        internal static bool ExportDirtySubnetwork(UtilityNetwork utilityNetwork, TraceArgument traceArguments, TraceExportOptions traceExportOptions, Subnetwork subnetwork, string outputFilePath)
        {
            using var traceManager = utilityNetwork.GetTraceManager();
            var subnetworkTracer = traceManager.GetTracer<SubnetworkTracer>();

            var subnetworkName = subnetwork.Name;

            var start = DateTime.Now;
            subnetworkTracer.Export(new Uri(outputFilePath), traceArguments, traceExportOptions);
            var end = DateTime.Now;

            Debug.WriteLine(end.ToString() + " - " + subnetworkName + ": " + (end - start).TotalSeconds + "(s) elapsed");

            return true;
        }

        #endregion

        #region Export clean subnetworks

        /// <summary>
        /// Export a clean subnetwork using the subnetwork manager.
        /// </summary>
        /// <param name="utilityNetwork">Utility Network</param>
        /// <param name="subnetwork">Subnetwork</param>
        /// <param name="outputFilePath">Output file path</param>
        /// <param name="networkAttributeNames">Network attribute names to export</param>
        /// <returns></returns>
        internal static bool ExportSubnetwork(UtilityNetwork utilityNetwork, Subnetwork subnetwork, string outputFilePath, IEnumerable<string> networkAttributeNames = null)
        {
            var utilityNetworkDefinition = utilityNetwork.GetDefinition();

            var networkAttributes = networkAttributeNames == null
                ? null
                : utilityNetworkDefinition.GetNetworkAttributes()
                    .Where(networkAttribute => networkAttributeNames.Contains(networkAttribute.Name));

            var subnetworkExportOptions = new SubnetworkExportOptions
            {
                // Note::We should be able to set these parameters at the result type level
                // There is no reason to inlclude geometry in BOTH the features and the connectivity
                // Only include geometry in the features section, then refer back to that when parsing connectivity (if you need to)
                IncludeDomainDescriptions = true,
                IncludeGeometry = true,
                SubnetworkExportResultTypes = new List<SubnetworkExportResultType>
                    {
                        SubnetworkExportResultType.Features,
                        SubnetworkExportResultType.Connectivity,
                    },
            };

            if (networkAttributeNames != null)
                subnetworkExportOptions.ResultNetworkAttributes = networkAttributes.ToList();

            return ExportSubnetwork(subnetworkExportOptions, subnetwork, outputFilePath);
        }
        
        internal static bool ExportSubnetwork(SubnetworkExportOptions subnetworkExportOptions, Subnetwork subnetwork, string outputFilePath)
        {
            var subnetworkName = subnetwork.Name;

            var subnetworkState = subnetwork.GetState();
            if (subnetworkState == SubnetworkStates.Dirty)
            {
                Debug.WriteLine(subnetworkName + " - Subnetwork is dirty, cannot export. Use trace instead.");
                return false;
            }

            var start = DateTime.Now;
            subnetwork.Export(new Uri(outputFilePath), subnetworkExportOptions);
            var end = DateTime.Now;
            Debug.WriteLine(end.ToString() + " - " + subnetworkName + ": " + (end - start).TotalSeconds + "(s) elapsed");
            
            return true;
        }

        #endregion
    }
}
