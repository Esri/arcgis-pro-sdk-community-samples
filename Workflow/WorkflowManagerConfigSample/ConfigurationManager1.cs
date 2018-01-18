//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Framework;
using System.Xml.Linq;
using ArcGIS.Desktop.Framework.Contracts;
using System.ComponentModel;



namespace WorkflowManagerConfigSample
{
    internal class ConfigurationManager1 : ConfigurationManager
    {
        /// <summary>
        /// Replaces the default ArcGIS Pro application name
        /// </summary>
        protected override string ApplicationName
        {
            get { return "WorkflowManagerConfigSample"; }
        }

        #region Hide/Remove Workflow View and Job View
        protected override void OnUpdateDatabase(XDocument database)
        {
            var nsp = database.Root.Name.Namespace;
            // select all elements that are panes
            var paneElements = from seg in database.Root.Descendants(nsp + "pane") select seg;
            // collect all elements that need to be removed
            var elements = new HashSet<XElement>();
            foreach (var paneElement in paneElements)
            {
                //remove the workflow view and the job view
                var id = paneElement.Attribute("id");
                if (id.Value.Equals("esri_workflow_workflowPane") || id.Value.Equals("esri_workflow_jobView"))//Skip our tabs - "MyConfiguration"
                    elements.Add(paneElement);
            }

            var tabElements = from seg in database.Root.Descendants(nsp + "tab") select seg;
            foreach (var tabElement in tabElements)
            {
                //remove all workflow tabs except the map and define aoi tab
                var id = tabElement.Attribute("id");
                if (id.Value.StartsWith("esri_workflow"))
                {
                    if (id.Value.Equals("esri_workflow_defineAOITab") || id.Value.Equals("esri_workflow_mapPaneTab"))
                        continue;
                    elements.Add(tabElement);
                }
            }

            var toolPaletteElements = from seg in database.Root.Descendants(nsp + "toolPalette") select seg;
            foreach (var toolPaletteElement in toolPaletteElements)
            {
                //remove the tool palette displaying the buttons to open workflow view
                var id = toolPaletteElement.Attribute("id");
                if (id != null && id.Value.Equals("esri_workflow_workflowViewPalette"))
                    elements.Add(toolPaletteElement);
            }
            // remove the elements
            foreach (var element in elements)
            {
                element.Remove();
            }
        }
        #endregion
    }
}
