/*

   Copyright 2023 Esri

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
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShowContainment
{
  internal class ShowContainmentButton : Button
  {    
        protected override async void OnClick()
        {
            Utility.CleanModule();
            if (MapView.Active == null || MapView.Active.Map == null)
                return;

            var pd = new ProgressDialog("Generate Diagram - Cancelable", "Canceled", 6, false);

            await CreateDiagram(new CancelableProgressorSource(pd));

            if (!string.IsNullOrEmpty(Utility.messError))
            {
                MessageBox.Show(Utility.messError);
            }
        }

        private async Task<string>  CreateDiagram(CancelableProgressorSource cps)
        {
            string diagramName = string.Empty;
            IList<Guid> listElements = new List<Guid>();
            IList<Guid> listGlobalID = new List<Guid>();
            NetworkDiagram myDiagram = null;
            UtilityNetwork un = null;

            await QueuedTask.Run(() =>
            {
                cps.Progressor.Max = 3;

                cps.Progressor.Value += 0;
                cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
                cps.Progressor.Message = "Step 1 – Get Selected Guids";
              
                if (!Utility.CheckValidation(MapView.Active))
                    return;

                un = Utility.GetUtilityNetworkFromMap(MapView.Active.Map);

                Utility.AddGUIToList(MapView.Active, un, ref listElements, ref listGlobalID);

            }, cps.Progressor);

            if (!string.IsNullOrEmpty(Utility.messError))
            {
                return "";
            }

            await QueuedTask.Run(() =>
            {
                cps.Progressor.Value += 1;
                cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
                cps.Progressor.Message = string.Format("Step 2 – Generate a diagram based on the '{0}' template", Utility.templateExpandContainer);
                //generate a diagram
                un = Utility.GetUtilityNetworkFromMap(MapView.Active.Map);

                DiagramManager diagramManager = un?.GetDiagramManager();
                DiagramTemplate diagramtemplate = null;
                try { diagramtemplate = diagramManager.GetDiagramTemplate(Utility.templateExpandContainer); }
                catch
                {
                    Utility.messError = Utility.messNoTemplateExpand;
                    return;
                }

                try
                {
                    myDiagram = diagramManager.CreateNetworkDiagram(diagramtemplate, listElements);
                }
                catch (Exception ex)
                {
                    Utility.messError = string.Format("Generate diagram\n{0}", ex.Message);
                }
            }, cps.Progressor);

            if (!string.IsNullOrEmpty(Utility.messError))
            {
                return "";
            }

            await QueuedTask.Run(() =>
            {
                cps.Progressor.Value += 1;
                cps.Progressor.Status = (cps.Progressor.Value * 100 / cps.Progressor.Max) + @" % Completed";
                cps.Progressor.Message = string.Format("Step 3 – ShowDiagram");

                diagramName = listElements[0].ToString().ToUpper();
                Utility.ShowDiagram(myDiagram, diagramName);
            });

            return Utility.messError;
        }
    }
}
