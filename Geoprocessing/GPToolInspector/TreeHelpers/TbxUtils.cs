/*

   Copyright 2025 Esri

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

namespace GPToolInspector.TreeHelpers
{
	internal class TbxUtils
	{
		public static string GetSystemToolPath()
		{
			var helperPath = MSIHelper.GetInstallDirAndVersion();
			if (!helperPath.HasValue) return string.Empty;
			return System.IO.Path.Combine(helperPath.Value.Folder, @"Resources\ArcToolBox\Toolboxes");
		}

		public static System.Text.Json.JsonSerializerOptions JsonOpt => new()
		{
			NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
		};

		public static IDictionary<string, string> GetToolContentRcMap(string toolPath)
		{
			if (string.IsNullOrEmpty(toolPath))
				return null;
			IDictionary<string, string> map;
			try
			{
				var rcFilePath = System.IO.Path.Combine(toolPath, TbxReader.ToolContentRcFileName);
				map = TbxReader.ReadContentRcFile(rcFilePath);
			}
			catch (Exception e)
			{
				var msg = $"Error reading description:'{e.Message}";
				System.Diagnostics.Debug.Assert(false, msg);
				return null;
			}
			return map;
		}

		public static IDictionary<string, string> GetToolBoxContentRcMap(string toolPath)
		{
			if (string.IsNullOrEmpty(toolPath))
				return null;
			IDictionary<string, string> map;
			try
			{
				var rcFilePath = System.IO.Path.Combine(toolPath, TbxReader.ToolBoxContentRcFileName);
				map = TbxReader.ReadContentRcFile(rcFilePath);
			}
			catch (Exception e)
			{
				var msg = $"Error reading description:'{e.Message}";
				System.Diagnostics.Debug.Assert(false, msg);
				return null;
			}
			return map;
		}
	}
}
