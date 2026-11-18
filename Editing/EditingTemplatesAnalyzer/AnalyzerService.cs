
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
using ArcGIS.Desktop.Editing.Templates;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EditingTemplatesAnalyzer
{
	internal static class AnalyzerService
	{
		// Get broken group templates by cause
		public static (List<EditingTemplate> missingPrimaryTemplates, List<EditingTemplate> missingSecondaryTemplates,
					List<EditingTemplate> missingBuilderTemplates, List<EditingTemplate> templatesFailingDefinitionQuery, List<EditingTemplate> presetMissingFeatures,
					List<EditingTemplate> missingTags, List<EditingTemplate> missingDescription) GetAllBrokenGroupTemplates(Map map)
		{
			if (map == null)
				return (new List<EditingTemplate>(), new List<EditingTemplate>(), new List<EditingTemplate>(),
					new List<EditingTemplate>(), new List<EditingTemplate>(), new List<EditingTemplate>(), new List<EditingTemplate>());

			var groupTemplates = map.GetLayersAsFlattenedList()
				.SelectMany(l => l.GetTemplates() ?? Enumerable.Empty<EditingTemplate>())
				.OfType<EditingGroupTemplate>();

			var missingPrimaryTemplates = new List<EditingTemplate>();
			var missingSecondaryTemplates = new List<EditingTemplate>();
			var missingPresetFeatures = new List<EditingTemplate>();
			var missingBuilderTemplates = new List<EditingTemplate>();
			var templatesFailingDefinitionQuery = new List<EditingTemplate>();
			var missingTags = new List<EditingTemplate>();
			var missingDescription = new List<EditingTemplate>();

			static void AddOnce(List<EditingTemplate> list, EditingTemplate? tpl)
			{
				if (tpl == null) return;
				if (!list.Contains(tpl)) list.Add(tpl);
			}

			static bool IsPresetGroup(EditingGroupTemplate gt)
			{
				return gt.Children?.Any(p =>
					string.Equals(p.Builder?.Name, "preset", StringComparison.OrdinalIgnoreCase)
					|| (p.Template?.Name?.Contains("__preset", StringComparison.OrdinalIgnoreCase) == true)) == true;
			}

			void AnalyzeNonPreset(EditingGroupTemplate group)
			{
				var children = group.Children;
				if (children == null) return;

				if (!children.Any())
				{
					if (group.BaseTemplate == null) AddOnce(missingPrimaryTemplates, group);
					if (children.Count == 0) AddOnce(missingSecondaryTemplates, group);
					if (group.BaseTemplate?.PassesDefinitionQuery == false) AddOnce(templatesFailingDefinitionQuery, group);
					if (group.Tags == null || !group.Tags.Any()) AddOnce(missingTags, group);
					if (string.IsNullOrEmpty(group.Description)) AddOnce(missingDescription, group);
					return;
				}

				foreach (var part in children)
				{
					if (part == null) continue;
					if (part.Builder == null) AddOnce(missingBuilderTemplates, group);
					if (part.Template == null) AddOnce(missingSecondaryTemplates, group);
					if (part.Parent?.BaseTemplate == null) AddOnce(missingPrimaryTemplates, group);
					if (part.Parent?.BaseTemplate?.PassesDefinitionQuery == false || part.Template?.PassesDefinitionQuery == false)
						AddOnce(templatesFailingDefinitionQuery, group);
				}

				if (group.Tags == null || !group.Tags.Any()) AddOnce(missingTags, group);
				if (string.IsNullOrEmpty(group.Description)) AddOnce(missingDescription, group);
			}

			void AnalyzePreset(EditingGroupTemplate group)
			{
				var children = group.Children;
				if (children == null) return;

				var presetParts = children.Where(p => string.Equals(p.Builder?.Name, "preset", StringComparison.OrdinalIgnoreCase)
					|| (p.Template?.Name?.Contains("__preset", StringComparison.OrdinalIgnoreCase) == true)).ToList();
				if (!presetParts.Any()) return;

				var malformed = presetParts.FirstOrDefault(p => string.IsNullOrEmpty(p.Template?.Name) || !p.Template.Name.Contains("__preset", StringComparison.OrdinalIgnoreCase));
				if (malformed != null) AddOnce(missingPresetFeatures, group);

				if (children.Any(p => p.Builder == null)) AddOnce(missingBuilderTemplates, group);
				if (group.Tags == null || !group.Tags.Any()) AddOnce(missingTags, group);
				if (string.IsNullOrEmpty(group.Description)) AddOnce(missingDescription, group);
			}

			foreach (var group in groupTemplates)
			{
				if (IsPresetGroup(group)) AnalyzePreset(group);
				else AnalyzeNonPreset(group);
			}

			return (missingPrimaryTemplates, missingSecondaryTemplates, missingBuilderTemplates,
						templatesFailingDefinitionQuery, missingPresetFeatures, missingTags, missingDescription);
		}


		/// <summary>
		/// Find broken group templates to remove; return layer/template pairs for removal actions.
		/// </summary>
		public static List<(FeatureLayer layer, EditingTemplate template)> FindBrokenGroupTemplateCandidates(Map map)
		{
			var candidates = new List<(FeatureLayer layer, EditingTemplate template)>();
			if (map == null) return candidates;

			var seen = new HashSet<(FeatureLayer layer, EditingTemplate template)>();
			foreach (var layer in map.GetLayersAsFlattenedList().OfType<FeatureLayer>())
			{
				foreach (var template in layer.GetTemplates() ?? Enumerable.Empty<EditingTemplate>())
				{
					if (template is not EditingGroupTemplate groupTemplate) continue;
					if (groupTemplate.Children == null) continue;
					if (!GroupTemplateIsBroken(groupTemplate, template)) continue;

					var pair = (layer, template);
					if (seen.Add(pair)) candidates.Add(pair);
				}
			}
			return candidates;
		}

		private static bool GroupTemplateIsBroken(EditingGroupTemplate groupTemplate, EditingTemplate template)
		{
			var children = groupTemplate.Children!;
			if (children.Count == 0)
				return groupTemplate.BaseTemplate == null
					|| groupTemplate.BaseTemplate?.PassesDefinitionQuery == false
					|| template.Tags == null || !template.Tags.Any()
					|| string.IsNullOrEmpty(template.Description);

			foreach (var part in children)
			{
				if (part == null) continue;
				if (part.Builder == null) return true;
				if (part.Parent?.BaseTemplate == null) return true;
				if (part.Template == null) return true;
				if (part.Parent?.BaseTemplate?.PassesDefinitionQuery == false || part.Template?.PassesDefinitionQuery == false) return true;
				if (template.Tags == null || !template.Tags.Any()) return true;
				if (string.IsNullOrEmpty(template.Description)) return true;
			}
			return false;
		}

		// Remove specified templates and return removed names 
		public static (List<string> removedNames, string? errorMessage) RemoveTemplates(List<(FeatureLayer layer, EditingTemplate template)> candidates)
		{
			var removed = new List<string>();
			if (candidates == null || candidates.Count == 0) return (removed, null);

			var failedDisplays = new List<string>();
			foreach (var candidate in candidates)
			{
				var (layer, template) = candidate;
				var display = $"{template?.Name ?? "<no-name>"} ({layer?.Name ?? "<no-layer>"})";
				try
				{
					layer.RemoveTemplate(template);			
					if (!removed.Contains(display)) removed.Add(display);
				}
				catch (Exception)
				{
					if (!failedDisplays.Contains(display)) failedDisplays.Add(display);
				}
			}

			if (failedDisplays.Count == 0)
				return (removed, null);

			var singleMessage = failedDisplays.Count == 1
					? $"Failed to remove template: {failedDisplays[0]}"
					: $"Failed to remove {failedDisplays.Count} templates: {string.Join(", ", failedDisplays)}";

			return (removed, singleMessage);
		}
	}
}
