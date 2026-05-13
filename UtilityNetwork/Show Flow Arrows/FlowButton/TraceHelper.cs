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

namespace Show_Flow_Arrows.FlowButton
{
    internal class TraceHelper
    {
        internal static bool SupportsFlowDirectionResults(UtilityNetwork utilityNetwork)
        {
            // It's always supported client/server
            if (!utilityNetwork.AreServerCapabilitiesSupported())
                return true;

            // Otherwise rely on the server to support the capability
            var serverCapabilities = utilityNetwork.GetServerCapabilities();
            return serverCapabilities.SupportsFlowDirectionResults;
        }
        internal static bool SupportsPropagationResults(UtilityNetwork utilityNetwork)
        {
            // It's always supported client/server
            if (!utilityNetwork.AreServerCapabilitiesSupported())
                return true;

            // Otherwise rely on the server to support the capability
            var serverCapabilities = utilityNetwork.GetServerCapabilities();
            return serverCapabilities.SupportsPropagatorResetters;
        }

        #region Export Dirty Subnetworks

        /// <summary>
        /// Exports a dirty subnetwork using the Trace export functionality. This should only be used on tiers that don't manage state.
        /// </summary>
        /// <param name="utilityNetwork">Utility Network</param>
        /// <param name="subnetwork">Subnetwork</param>
        /// <param name="outputFilePath">Output file path</param>
        /// <returns></returns>
        internal static bool ExportDirtySubnetwork(UtilityNetwork utilityNetwork, Subnetwork subnetwork, string outputFilePath)
        {
            var utilityNetworkDefinition = utilityNetwork.GetDefinition();

            var traceArguments = new TraceArgument(subnetwork);

            traceArguments.ResultOptions = new ResultOptions
            {
                IncludeGeometry = true,
            };
            traceArguments.ResultTypes = new List<ResultType>
            {
                ResultType.Feature,
            };

            // The TraceArgument must be set with a TraceConfiguration object with a valid DomainNetwork for the subnetwork-based SubnetworkTracer.
            traceArguments.Configuration = subnetwork.Tier.GetTraceConfiguration();
            traceArguments.Configuration.IncludeContainers = false;
            traceArguments.Configuration.IncludeStructures = false;
            traceArguments.Configuration.IncludeContent = false;

            var traceExportOptions = new TraceExportOptions()
            {
                IncludeDomainDescriptions = false,
            };
            if (TraceHelper.SupportsFlowDirectionResults(utilityNetwork))
                traceExportOptions.IncludeFlowDirections = true;
            if (TraceHelper.SupportsPropagationResults(utilityNetwork))
                traceExportOptions.IncludePropagatedValues = true;

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
        /// <returns></returns>
        internal static bool ExportSubnetwork(UtilityNetwork utilityNetwork, Subnetwork subnetwork, string outputFilePath)
        {
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
                    },
            };

            if (TraceHelper.SupportsFlowDirectionResults(utilityNetwork))
                subnetworkExportOptions.IncludeFlowDirections = true;
            if(TraceHelper.SupportsPropagationResults(utilityNetwork))
                subnetworkExportOptions.IncludePropagatedValues = true;

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
