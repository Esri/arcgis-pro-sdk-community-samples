//Copyright 2017 Esri

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System.Windows.Input;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;

namespace HookProCommands
{
    internal class MyAddBookmarksButton : Button
    {
        protected override void OnClick()
        {
            // TODO: step 2 enable ArcGIS Pro command functionality 
            var commandId = @"esri_mapping_createBookmark";
            // get the ICommand interface from the ArcGIS Pro Button
            // using command's plug-in wrapper
            var iCommand = FrameworkApplication.GetPlugInWrapper(commandId) as ICommand;
            if (iCommand != null)
            {
                // Let ArcGIS Pro do the work for us
                if (iCommand.CanExecute(null)) iCommand.Execute(null);
            }
        }
    }
}
