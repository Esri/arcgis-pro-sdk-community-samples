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
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Desktop.Framework.Contracts;
using Dialogs = ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping.Events;
using System.Collections.Generic;
using static BringUpSubnetworkNamesOnDiagramEdges.BringUpSubnetworkNamesOnDiagramEdgesModule;
using static BringUpSubnetworkNamesOnDiagramEdges.CommonTools;

namespace BringUpSubnetworkNamesOnDiagramEdges
{
  internal class TemplateGallery : Gallery
  {
    private bool _isInitialized;

    public TemplateGallery()
    {
      ActiveMapViewChangedEvent.Subscribe(OnActiveMapViewChanged);
    }

    protected override void OnDropDownOpened()
    {
      if (!_isInitialized)
      {
        QueuedTask.Run(() =>
        {
          if (GlobalDiagramManager == null)
          {
            GlobalUtilityNetwork = GetUtilityNetworkFromActiveMap();
            if (GlobalUtilityNetwork == null)
            {
              Dialogs.MessageBox.Show("This tool only works with utility networks, not with trace networks");
              return;
            }
            GlobalDiagramManager = GlobalUtilityNetwork.GetDiagramManager();
          }

          using UtilityNetworkDefinition unDef = GlobalUtilityNetwork.GetDefinition();
          List<string> sysTemplate = [];
          foreach (var dn in unDef.GetDomainNetworks())
          {
            foreach (var t in dn.Tiers)
            {
              foreach (string tempName in t.GetDiagramTemplateNames())
              {
                if (!sysTemplate.Contains(tempName))
                  sysTemplate.Add(tempName);
              }
            }
          }

          foreach (DiagramTemplate template in GlobalDiagramManager.GetDiagramTemplates())
          {
            string name = template.Name;
            if (!sysTemplate.Contains(name))
              Add(new GalleryItem(name, null, name));
          }

          _isInitialized = true;
        });
      }
      else
        SelectedItem = _ActiveTemplate;
    }

    protected override void Uninitialize()
    {
      ActiveMapViewChangedEvent.Unsubscribe(OnActiveMapViewChanged);

      base.Uninitialize();
    }

    private void OnActiveMapViewChanged(ActiveMapViewChangedEventArgs obj)
    {
      if (obj == null || obj.IncomingView == null)
        return;

      QueuedTask.Run(() =>
      {
        if (GlobalUtilityNetwork == null || GlobalUtilityNetwork.GetName() != GetUtilityNetworkFromActiveMap(obj.IncomingView.Map).GetName())
        {
          _isInitialized = false;
          Clear();
        }
      });
    }


    protected override void OnClick(GalleryItem item)
    {
      base.OnClick(item);

      RunNewDiagram(item?.ToString());

      if (!string.IsNullOrEmpty(status))
        Dialogs.MessageBox.Show(status);
    }
  }
}
