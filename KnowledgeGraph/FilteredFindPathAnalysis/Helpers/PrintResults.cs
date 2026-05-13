/*

   Copyright 2026 Esri

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
using ArcGIS.Core.Data.Knowledge.Analytics;
using System.Collections.Generic;
using System.Text;

namespace FilteredFindPathAnalysis.Helpers
{
	public class BuildPathAsString
	{
		public string PathString { get; private set; }

		public BuildPathAsString()
		{
			PathString = "";
		}

		public BuildPathAsString(BuildPathAsString copy)
		{
			PathString = copy.PathString;
		}

		public void Append(Entity first, Entity second, PathRelationship path_relation)
		{

			var A = $"({first.TypeName}:{KGUtils.Instance.FormatID(first.Uid.ToString())})";
			var B = $"({second.TypeName}:{KGUtils.Instance.FormatID(second.Uid.ToString())})";

			if (string.IsNullOrEmpty(PathString))
				PathString = A;

			var rel = path_relation.Relationship;
			var rel_uid = KGUtils.Instance.FormatID(rel.Uid.ToString());
			var rel_string = $"[{rel.TypeName}:{rel_uid}]";

			if (path_relation.SameDirectionAsPath)
				rel_string = $"-{rel_string}->{B}";
			else
				rel_string = $"<-{rel_string}-{B}";

			PathString += rel_string;
		}
	}

	public static class PrintResults
	{

		public static string PrintAsString(
			this KnowledgeGraphFilteredFindPathsResults results,
			FFP_Analysis_Type ffp_type)
		{
			//print out paths by increasing length
			StringBuilder sb = new StringBuilder();
			var path_by_len_indices = results.PathIndicesOrderedByIncreasingPathLength;

			sb.AppendLine($"\r\nResults for {ffp_type.ToString()}:");
			sb.AppendLine($"Paths by length: {path_by_len_indices.Length}\r\n");

			foreach (var path_idx in path_by_len_indices)
			{
				var path = (ResultPath)results.MaterializePath(path_idx.index);
				var path_by_len_str = path.PrintAsString(path_idx.index);
				sb.AppendLine(path_by_len_str);
			}
			return sb.ToString();
		}

		public static string PrintAsString(this ResultPath resultPath, long index = 0)
		{
			var final_paths = new List<BuildPathAsString>();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(
				$"ResultPath[{index}] len: {resultPath.Length}, min: {resultPath.MinCost} max: {resultPath.MaxCost}");

			foreach (var rel_group in resultPath.RelationshipGroups)
			{
				var first_group = final_paths.Count == 0;
				var paths_temp = new List<BuildPathAsString>();

				foreach (var relation in rel_group.Relationships)
				{
					//This is the first group so start the new paths
					if (first_group)
					{
						var new_path = new BuildPathAsString();
						new_path.Append(rel_group.FirstEntity, rel_group.SecondEntity, relation);
						paths_temp.Add(new_path);
					}
					else //Append the relation to each of the existing paths
					{
						foreach (var path in final_paths)
						{
							var new_path = new BuildPathAsString(path);
							new_path.Append(rel_group.FirstEntity, rel_group.SecondEntity, relation);
							paths_temp.Add(new_path);
						}
					}
				}
				final_paths.Clear();
				final_paths.AddRange(paths_temp);//Collect the paths we are building
			}
			//Print them out
			int p = 0;
			foreach (var path in final_paths)
				sb.AppendLine($" Path[{p++}]" + path.PathString);

			return sb.ToString();
		}
	}
}
