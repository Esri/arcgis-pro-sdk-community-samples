/*

   Copyright 2017 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

   See the License for the specific language governing permissions and
   limitations under the License.

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace CustomEvent
{

  /// <summary>
  /// Toggle the application name and raise the name changed custom event
  /// </summary>
  internal class Publish : Button
  {
    /// <summary>
    /// Execute the button onclick action
    /// </summary>
    protected override void OnClick()
    {
      //Change the name
      string oldName = FrameworkApplication.Name;
      FrameworkApplication.SubTitle = DateTime.Now.ToString("h:mm:ss tt zz");
      //publish our custom event
      NameChangedEvent.Publish(new NameChangedEventArgs(oldName, FrameworkApplication.Name));
    }
  }
}
