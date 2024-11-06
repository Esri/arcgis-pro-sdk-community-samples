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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunCoreHostApp
{
  internal class DotNetHelper
  {
    internal static string ProgFileFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

    internal static string DotNetRuntimePath = string.Empty;

    internal static string DotNetExePath = string.Empty;

    internal static List<string> DotNetVersions = new();

    internal static void InitializeDotNetVars()
    {
      // check if this is a .NET file.. only look at the 'newest' .NET framework version
      DotNetRuntimePath = Path.Combine(ProgFileFolder, "dotnet", "shared", "Microsoft.NETCore.App");
      DotNetExePath = Path.Combine(ProgFileFolder, "dotnet", "dotnet.exe");
      // get the newest version of DotNET
      List<string> dotNetDirs = Directory.GetDirectories(DotNetRuntimePath).ToList();
      dotNetDirs.Sort();
      foreach (var dir in dotNetDirs)
      {
        DotNetVersions.Add(Path.GetFileName (dir));
      }
    }

  }
}
