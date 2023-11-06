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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TableViewerTest.Helper
{
  internal static class DiagnosticHelper
  {
    internal static string CurrentMemberName([CallerMemberName] string caller = null) => caller;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static string GetMethodName()
    {
      var st = new StackTrace(new StackFrame(1));
      var fullName = st.GetFrame(0).GetMethod().ReflectedType.FullName;
      var parts = fullName.Split('+');
      if (parts.Length > 1 && parts[1].Contains ('>') && parts[1].Contains('<'))
      {
        var idxStart = parts[1].IndexOf('<')+1;
        var idxEnd = parts[1].LastIndexOf('>');
        var name = parts[1].Substring(idxStart, idxEnd-idxStart);
        return $@"{parts[0]}.{name}()";
      }
      else return $@"{st.GetFrame(0).GetMethod().Name}";
    }

    internal static void Start ([CallerMemberName] string caller = null) => System.Diagnostics.Trace.WriteLine ($@"Called: {caller}");

    internal static void WriteWarning (string line) => ArcGIS.Desktop.Framework.Utilities.EventLog.Write
          (ArcGIS.Desktop.Framework.Utilities.EventLog.EventType.Warning, $@"{System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',')[0]}: {line}");
  }
}
