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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessProjectFiles
{
  internal static class EventLog
  {
    /// <summary>
    /// Writes the given line to ArcGIS Pro event logs using Warning status
    /// </summary>
    /// <param name="line"></param>
    internal static void WriteWarning(string line) => ArcGIS.Desktop.Framework.Utilities.EventLog.Write
          (ArcGIS.Desktop.Framework.Utilities.EventLog.EventType.Warning,
          $@"{System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',')[0]}: {line}");

    /// <summary>
    /// Writes the given line to ArcGIS Pro event logs using Debug status
    /// </summary>
    /// <param name="line"></param>
    internal static void WriteDebug(string line) => ArcGIS.Desktop.Framework.Utilities.EventLog.Write
          (ArcGIS.Desktop.Framework.Utilities.EventLog.EventType.Debug,
          $@"{System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',')[0]}: {line}");

    /// <summary>
    /// Writes the given line to ArcGIS Pro event logs using Information status
    /// </summary>
    /// <param name="line"></param>
    internal static void WriteInfo(string line) => ArcGIS.Desktop.Framework.Utilities.EventLog.Write
          (ArcGIS.Desktop.Framework.Utilities.EventLog.EventType.Information,
          $@"{System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',')[0]}: {line}");


    /// <summary>
    /// Writes the given line to ArcGIS Pro event logs using Error status
    /// </summary>
    /// <param name="line"></param>
    internal static void WriteError(string line) => ArcGIS.Desktop.Framework.Utilities.EventLog.Write
          (ArcGIS.Desktop.Framework.Utilities.EventLog.EventType.Error,
          $@"{System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',')[0]}: {line}");
  }
}
