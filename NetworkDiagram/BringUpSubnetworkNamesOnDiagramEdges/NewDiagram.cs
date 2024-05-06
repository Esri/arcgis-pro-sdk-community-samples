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
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using static BringUpSubnetworkNamesOnDiagramEdges.BringUpSubnetworkNamesOnDiagramEdgesModule;

namespace BringUpSubnetworkNamesOnDiagramEdges
{
  internal class NewDiagram : Button
  {
    protected override void OnClick()
    {
      base.OnClick();
      RunNewDiagram(_ActiveTemplate);

      if (!string.IsNullOrEmpty(status))
        MessageBox.Show(status);
    }
  }

}
