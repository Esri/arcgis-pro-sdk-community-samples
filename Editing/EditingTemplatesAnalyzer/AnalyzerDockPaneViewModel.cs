
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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace EditingTemplatesAnalyzer
{
	internal class AnalyzerDockPaneViewModel : DockPane
	{
		private const string _dockPaneID = "EditingTemplatesAnalyzer_AnalyzerDockPane";

		// Manage undo/redo
		public override OperationManager OperationManager
		{
			get
			{
				return MapView.Active?.Map?.OperationManager ?? LayoutView.Active?.Layout?.OperationManager;
			}
		}

		private ObservableCollection<string> _brokenTemplates = new();
		public ObservableCollection<string> BrokenTemplates
		{
			get => _brokenTemplates;
			private set => SetProperty(ref _brokenTemplates, value);
		}

		private ObservableCollection<string> _missingPrimaryTemplates = new();
		public ObservableCollection<string> MissingPrimaryTemplates
		{
			get => _missingPrimaryTemplates;
			private set => SetProperty(ref _missingPrimaryTemplates, value);
		}

		private ObservableCollection<string> _missingSecondaryTemplates = new();
		public ObservableCollection<string> MissingSecondaryTemplates
		{
			get => _missingSecondaryTemplates;
			private set => SetProperty(ref _missingSecondaryTemplates, value);
		}

		private ObservableCollection<string> _missingPresetFeatures = new();
		public ObservableCollection<string> MissingPresetFeatures
		{
			get => _missingPresetFeatures;
			private set => SetProperty(ref _missingPresetFeatures, value);
		}

		private ObservableCollection<string> _missingBuilderTemplates = new();
		public ObservableCollection<string> MissingBuilderTemplates
		{
			get => _missingBuilderTemplates;
			private set => SetProperty(ref _missingBuilderTemplates, value);
		}


		private ObservableCollection<string> _templatesFailingDefinitionQuery = new();
		public ObservableCollection<string> TemplatesFailingDefinitionQuery
		{
			get => _templatesFailingDefinitionQuery;
			private set => SetProperty(ref _templatesFailingDefinitionQuery, value);
		}

		private ObservableCollection<string> _missingTagsTemplates = new();
		public ObservableCollection<string> MissingTagsTemplates
		{
			get => _missingTagsTemplates;
			private set => SetProperty(ref _missingTagsTemplates, value);
		}

		private ObservableCollection<string> _missingDescriptionTemplates = new();
		public ObservableCollection<string> MissingDescriptionTemplates
		{
			get => _missingDescriptionTemplates;
			private set => SetProperty(ref _missingDescriptionTemplates, value);
		}

		private ObservableCollection<string> _displayedTemplates = new();
		public ObservableCollection<string> DisplayedTemplates
		{
			get => _displayedTemplates;
			private set
			{
				if (_displayedTemplates == value)
					return;
				if (_displayedTemplates != null)
					_displayedTemplates.CollectionChanged -= DisplayedTemplates_CollectionChanged;

				SetProperty(ref _displayedTemplates, value);

				if (_displayedTemplates != null)
					_displayedTemplates.CollectionChanged += DisplayedTemplates_CollectionChanged;

				UpdateNoResults();
			}
		}


		private bool _analysisRun = false;
		private bool _noResults = false;
		public bool NoResults
		{
			get => _noResults;
			private set => SetProperty(ref _noResults, value);
		}

		private void DisplayedTemplates_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateNoResults();

		private void UpdateNoResults()
		{
			var has = _displayedTemplates != null && _displayedTemplates.Any();
			NoResults = _analysisRun && !has;
		}

		private string? _selectedCategory = null;
		public string? SelectedCategory
		{
			get => _selectedCategory;
			set
			{
				if (SetProperty(ref _selectedCategory, value))
				{
					UpdateDisplayedTemplates();
				}
			}
		}

		private ObservableCollection<string> _categories = new(new[] {
				"<All>",
			"Missing primary template",
			"Missing secondary template",
			"Missing feature from preset",
			"Failed definition query",
			"Missing feature builder",
			"Missing description",
			"Missing tags",
		});

		public ObservableCollection<string> Categories
		{
			get => _categories;
			private set => SetProperty(ref _categories, value);
		}

		private bool _showCategoryFilter = true;
		public bool ShowCategoryFilter
		{
			get => _showCategoryFilter;
			private set => SetProperty(ref _showCategoryFilter, value);
		}

		private void UpdateDisplayedTemplates()
		{
			// If the "<All>" category is selected (or nothing), show the union of all broken templates.
			if (string.IsNullOrEmpty(SelectedCategory) || SelectedCategory == "<All>")
			{
				DisplayedTemplates = BrokenTemplates ?? new ObservableCollection<string>();
				return;
			}

			switch (SelectedCategory)
			{
				case "Missing primary template":
					DisplayedTemplates = MissingPrimaryTemplates ?? new ObservableCollection<string>();
					break;
				case "Missing secondary template":
					DisplayedTemplates = MissingSecondaryTemplates ?? new ObservableCollection<string>();
					break;
				case "Missing preset features":
					DisplayedTemplates = MissingPresetFeatures ?? new ObservableCollection<string>();
					break;
				case "Failed definition query":
					DisplayedTemplates = TemplatesFailingDefinitionQuery ?? new ObservableCollection<string>();
					break;
				case "Missing template builder":
					DisplayedTemplates = MissingBuilderTemplates ?? new ObservableCollection<string>();
					break;
				case "Missing tags":
					DisplayedTemplates = MissingTagsTemplates ?? new ObservableCollection<string>();
					break;
				case "Missing description":
					DisplayedTemplates = MissingDescriptionTemplates ?? new ObservableCollection<string>();
					break;
				default:
					DisplayedTemplates = new ObservableCollection<string>();
					break;
			}
		}

		public ICommand BrokenGroupTemplatesCommand { get; }

		public ICommand DeleteSelectedCommand { get; private set; }

		private ObservableCollection<string> _selectedTemplates = new();
		public ObservableCollection<string> SelectedTemplates
		{
			get => _selectedTemplates;
			set
			{
				if (_selectedTemplates == value)
					return;
				if (_selectedTemplates != null)
					_selectedTemplates.CollectionChanged -= SelectedTemplates_CollectionChanged;
				SetProperty(ref _selectedTemplates, value);
				if (_selectedTemplates != null)
					_selectedTemplates.CollectionChanged += SelectedTemplates_CollectionChanged;
				CommandManager.InvalidateRequerySuggested();
			}
		}

		private void SelectedTemplates_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			CommandManager.InvalidateRequerySuggested();
		}

		public ICommand ClearResultsCommand { get; }

		protected AnalyzerDockPaneViewModel()
		{
			SelectedTemplates = new ObservableCollection<string>();

			BrokenGroupTemplatesCommand = new RelayCommand(async () => await LoadBrokenGroupTemplates());
			ClearResultsCommand = new RelayCommand(async () => { ClearResults(); await Task.CompletedTask; });
			DeleteSelectedCommand = new RelayCommand(async () => await DeleteSelected(), () => SelectedTemplates != null && SelectedTemplates.Any());
		}

		/// <summary>
		/// Show the DockPane.
		/// </summary>
		internal static void Show()
		{
			DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
			if (pane == null)
				return;

			pane.Activate();
		}

		private async Task DeleteTemplate(string? templateName)
		{
			if (string.IsNullOrEmpty(templateName))
				return;

			// remove template name from collections
			void RemoveFromCollection(ObservableCollection<string>? collection)
			{
				if (collection == null) return;
				if (collection.Contains(templateName))
					collection.Remove(templateName);
			}
			RemoveFromCollection(BrokenTemplates);
			RemoveFromCollection(MissingPrimaryTemplates);
			RemoveFromCollection(MissingSecondaryTemplates);
			RemoveFromCollection(MissingPresetFeatures);
			RemoveFromCollection(MissingBuilderTemplates);
			RemoveFromCollection(TemplatesFailingDefinitionQuery);
			RemoveFromCollection(MissingTagsTemplates);
			RemoveFromCollection(MissingDescriptionTemplates);
			RemoveFromCollection(DisplayedTemplates);

			await Task.CompletedTask;
		}

		private async Task DeleteSelected()
		{
			var map = MapView.Active?.Map;
			if (map == null)
			{
				MessageBox.Show("No active map.");
				return;
			}

			// Find broken templates on the map 
			List<(FeatureLayer layer, EditingTemplate template)> candidates = null;
			try
			{
				candidates = await QueuedTask.Run(() => AnalyzerService.FindBrokenGroupTemplateCandidates(map));
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to analyze templates: {ex.Message}");
				return;
			}

			// Filter candidates down to only those that match the user selection
			var selectedNames = new HashSet<string>(SelectedTemplates);
			var filtered = candidates.Where(c => c.template != null && selectedNames.Contains(BuildDisplay(c))).ToList();

			// build confirmation message
			var sb = new StringBuilder();
			sb.AppendLine("Templates selected to be deleted:\n");
			foreach (var (layer, template) in filtered)
			{
				sb.AppendLine($" - {template?.Name} ({layer?.Name})");
			}
			sb.AppendLine();
			sb.AppendLine("Do you want to continue?");

			var confirm = MessageBox.Show(sb.ToString(),
				"Confirm Delete", MessageBoxButton.YesNo);
			if (confirm != MessageBoxResult.Yes)
				return;

			// delete on selected templates
			var result = await QueuedTask.Run(() => AnalyzerService.RemoveTemplates(filtered));

			var removedNames = result.removedNames ?? new List<string>();

			// Remove the template names from UI collections
			foreach (var name in removedNames)
			{
				await DeleteTemplate(name);
			}
		}

		private async Task LoadBrokenGroupTemplates()
		{
			var map = MapView.Active?.Map;
			if (map == null)
			{
				MessageBox.Show("No active map.");
				return;
			}

			// Get analysis result and candidate layer/template pairs so we can build
			var result = await QueuedTask.Run(() => AnalyzerService.GetAllBrokenGroupTemplates(map));
			var candidates = await QueuedTask.Run(() => AnalyzerService.FindBrokenGroupTemplateCandidates(map));

			// Create per-category display name collections using the candidates to
			// include layer context.
			var primaryNodes = BuildNodes(result.missingPrimaryTemplates, candidates);
			var secondaryNodes = BuildNodes(result.missingSecondaryTemplates, candidates);
			var builderNodes = BuildNodes(result.missingBuilderTemplates, candidates);
			var presetNodes = BuildNodes(result.presetMissingFeatures, candidates);
			var primaryDefNodes = BuildNodes(result.templatesFailingDefinitionQuery, candidates);
			var tagsNodes = BuildNodes(result.missingTags, candidates);
			var descriptionNodes = BuildNodes(result.missingDescription, candidates);

			// expose per-category collections
			MissingPrimaryTemplates = primaryNodes;
			MissingSecondaryTemplates = secondaryNodes;
			MissingBuilderTemplates = builderNodes;
			TemplatesFailingDefinitionQuery = primaryDefNodes;
			MissingPresetFeatures = presetNodes;
			MissingTagsTemplates = tagsNodes;
			MissingDescriptionTemplates = descriptionNodes;

			// Build union view using the display strings and ensure distinct entries
			var distinctNames = primaryNodes
				.Concat(secondaryNodes)
				.Concat(builderNodes)
				.Concat(presetNodes)
				.Concat(primaryDefNodes)
				.Concat(tagsNodes)
				.Concat(descriptionNodes)
				.Distinct()
				.ToList();

			BrokenTemplates = new ObservableCollection<string>(distinctNames);

			// default category to "<All>" if unset
			if (string.IsNullOrEmpty(SelectedCategory))
				SelectedCategory = "<All>";
			else
				UpdateDisplayedTemplates();

			_analysisRun = true;
			UpdateNoResults();
		}

		private void ClearResults()
		{
			ResetAllCollections();
			SelectedCategory = "<All>";
			_analysisRun = false;
			NoResults = false;
		}

		private void ResetAllCollections()
		{
			MissingPrimaryTemplates = new ObservableCollection<string>();
			MissingSecondaryTemplates = new ObservableCollection<string>();
			MissingBuilderTemplates = new ObservableCollection<string>();
			TemplatesFailingDefinitionQuery = new ObservableCollection<string>();
			MissingTagsTemplates = new ObservableCollection<string>();
			MissingDescriptionTemplates = new ObservableCollection<string>();
			MissingPresetFeatures = new ObservableCollection<string>();
			DisplayedTemplates = new ObservableCollection<string>();
			BrokenTemplates = new ObservableCollection<string>();
		}

		// build display string for a layer/template pair 
		private static string BuildDisplay((FeatureLayer layer, EditingTemplate template) pair)
		{
			var layerName = pair.layer?.Name ?? "<no-layer>";
			var tplName = pair.template?.Name ?? pair.template?.GetType().Name ?? "<no-name>";
			return $"{tplName} ({layerName})";
		}

		// build observable nodes for a set of templates given candidate layer/template pairs
		private static ObservableCollection<string> BuildNodes(IEnumerable<EditingTemplate>? templates, IEnumerable<(FeatureLayer layer, EditingTemplate template)>? candidates)
		{
			var nodes = new ObservableCollection<string>();
			if (templates == null) return nodes;
			foreach (var t in templates.Distinct().OfType<EditingTemplate>())
			{
				var matches = candidates?.Where(c => c.template == t).ToList();
				if (matches != null && matches.Any())
				{
					foreach (var m in matches)
						nodes.Add(BuildDisplay(m));
				}
				else
				{
					nodes.Add(t.Name ?? t.GetType().Name);
				}
			}
			return nodes;
		}
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class AnalyzerDockPane_ShowButton : Button
	{
		protected override void OnClick()
		{
			AnalyzerDockPaneViewModel.Show();
		}
	}
}
