/*

   Copyright 2019 Esri

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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace GetSymbolSwatch
{
	internal class ShowSymbolSwatchDockpaneViewModel : DockPane
	{
		private const string _dockPaneID = "GetSymbolSwatch_ShowSymbolSwatchDockpane";

    private readonly object _lockCollection = new object();


    protected ShowSymbolSwatchDockpaneViewModel() {
      BindingOperations.EnableCollectionSynchronization(SymbolSwatchInfoList, _lockCollection);
    }

		private ICommand _cmdRefreshSwatches;
		public ICommand CmdRefreshSwatches
		{
			get
			{
				return _cmdRefreshSwatches ?? (_cmdRefreshSwatches = new RelayCommand(() =>
				{
          SymbolSwatchInfoList.Clear();
					if (MapView.Active?.Map == null) return;
					var layers = MapView.Active.Map.GetLayersAsFlattenedList().OfType<FeatureLayer>();
					QueuedTask.Run(() => {
						foreach (FeatureLayer layer in layers)
						{
							CIMRenderer renderer = layer.GetRenderer();
              var types = renderer.GetType().ToString().Split(new char[] { '.' });
              var type = types[types.Length - 1];
              switch (type)
							{
								case "CIMSimpleRenderer":
                  var simpleSi = GetSymbolStyleItem((renderer as CIMSimpleRenderer).Symbol.Symbol);
                  var newSimpleSymbolSwatchInfo = new SymbolSwatchInfo(layer.Name, type, simpleSi.PreviewImage);
                  lock (_lockCollection)
                  {
                    SymbolSwatchInfoList.Add(newSimpleSymbolSwatchInfo);
                  }
                  break;
                case "CIMUniqueValueRenderer":
                  var uniqueValueRenderer = renderer as CIMUniqueValueRenderer;
                  foreach (CIMUniqueValueGroup nextGroup in uniqueValueRenderer.Groups)
                  {
                    foreach (CIMUniqueValueClass nextClass in nextGroup.Classes)
                    {
                      CIMMultiLayerSymbol multiLayerSymbol = nextClass.Symbol.Symbol as CIMMultiLayerSymbol;
                      var mlSi = GetSymbolStyleItem(multiLayerSymbol);
                      var newMlSymbolSwatchInfo = new SymbolSwatchInfo(layer.Name, type, mlSi.PreviewImage)
                      {
                        Note = nextClass.Label
                      };
                      lock (_lockCollection)
                      {
                        SymbolSwatchInfoList.Add(newMlSymbolSwatchInfo);
                      }
                    }
                  }
                  break;
                case "CIMClassBreaksRenderer":
                  var classBreaksRenderer = renderer as CIMClassBreaksRenderer;
                  foreach (CIMClassBreak nextClass in classBreaksRenderer.Breaks)
                  {
                    var classBreakSymbol = nextClass.Symbol.Symbol as CIMSymbol;
                    var mlSi = GetSymbolStyleItem(classBreakSymbol);
                    var newMlSymbolSwatchInfo = new SymbolSwatchInfo(layer.Name, type, mlSi.PreviewImage)
                    {
                      Note = nextClass.Label
                    };
                    lock (_lockCollection)
                    {
                      SymbolSwatchInfoList.Add(newMlSymbolSwatchInfo);
                    }
                  }
                  if (classBreaksRenderer.UseExclusionSymbol)
                  {
                    var classBreakSymbol = classBreaksRenderer.ExclusionSymbol.Symbol as CIMSymbol;
                    var mlSi = GetSymbolStyleItem(classBreakSymbol);
                    var newMlSymbolSwatchInfo = new SymbolSwatchInfo(layer.Name, type, mlSi.PreviewImage)
                    {
                      Note = classBreaksRenderer.ExclusionLabel
                    };
                    lock (_lockCollection)
                    {
                      SymbolSwatchInfoList.Add(newMlSymbolSwatchInfo);
                    }
                  }
                  break;
                default:
                  var defaultSymbolSwatchInfo = new SymbolSwatchInfo(layer.Name, type, null)
                  {
                    Note = "Sample code not implemented"
                  };
                  lock (_lockCollection)
                  {
                    SymbolSwatchInfoList.Add(defaultSymbolSwatchInfo);
                  }
                  break;
							}
            }
          });
				}));
			}
		}

    /// <summary>
    /// Get SymbolStyleItem for a given Symbol ...
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    private SymbolStyleItem GetSymbolStyleItem (CIMSymbol symbol)
    {
      var si = new SymbolStyleItem()
      {
        Symbol = symbol,
        PatchHeight = 32,
        PatchWidth = 32
      };
      return si;
    }

		public ObservableCollection<SymbolSwatchInfo> SymbolSwatchInfoList { get; } = new ObservableCollection<SymbolSwatchInfo>();


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

		/// <summary>
		/// Text shown near the top of the DockPane.
		/// </summary>
		private string _heading = "Show Feature Class Symbols for Active Map View";
		public string Heading
		{
			get { return _heading; }
			set
			{
				SetProperty(ref _heading, value, () => Heading);
			}
		}
	}

	/// <summary>
	/// Button implementation to show the DockPane.
	/// </summary>
	internal class ShowSymbolSwatchDockpane_ShowButton : Button
	{
		protected override void OnClick()
		{
			ShowSymbolSwatchDockpaneViewModel.Show();
		}
	}
}
