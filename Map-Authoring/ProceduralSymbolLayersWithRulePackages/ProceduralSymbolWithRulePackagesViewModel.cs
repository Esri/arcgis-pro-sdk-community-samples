using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Newtonsoft.Json.Linq;

namespace ProceduralSymbolLayersWithRulePackages
{
    internal class ProceduralSymbolWithRulePackagesViewModel : DockPane
    {
        private const string _dockPaneID = "ProceduralSymbolLayersWithRulePackages_ProceduralSymbolWithRulePackages";
        private static readonly string _rulePkgPath = $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"ArcGIS\Projects\RulePackages")}";
        private static readonly string _styleFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\AppData\Roaming\ESRI\ArcGISPro\BuildingStyles.stylx";
        private static string _arcgisOnline = @"http://www.arcgis.com:80/";
        protected ProceduralSymbolWithRulePackagesViewModel()
        {
            //Enable collection mods in the background
            BindingOperations.EnableCollectionSynchronization(RulePackageCollection, _rpkLock);
        }
        protected override async Task InitializeAsync()
        {
           
                DockpaneVisible = Visibility.Collapsed; //Visibility controlled from the module            
                await GetRulePackages();

                if (Module1.BuildingFootprintLayer == null)
                    return;                            
           
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

        #region public properties
        /// <summary>
        /// Text shown near the top of the DockPane.
        /// </summary>
        private string _heading = "Procedural Symbol Layer";
        public string Heading
        {
            get { return _heading; }
            set
            {
                SetProperty(ref _heading, value, () => Heading);
            }
        }

        private readonly object _rpkLock = new object();
        private ObservableCollection<RulePackage> _rulePackageCollection = new ObservableCollection<RulePackage>();

        /// <summary>
        /// Collection of City Engine Rule packages.
        /// </summary>
        public ObservableCollection<RulePackage> RulePackageCollection
        {
            get { return _rulePackageCollection; }
            set { SetProperty(ref _rulePackageCollection, value, () => RulePackageCollection); }
        }

        private RulePackage _selectedRulePackage;

        /// <summary>
        /// Selected Rule Package
        /// </summary>
        public RulePackage SelectedRulePackage
        {
            get { return _selectedRulePackage;}
            set
            {
                
                SetProperty(ref _selectedRulePackage, value, () => SelectedRulePackage);
                ApplyRulePackage();
                
            }
        }
        private static StyleProjectItem _styleProjectItem = null;

        /// <summary>
        /// StyleProjectItem to store the procedural symbol generated.
        /// </summary>
        public static StyleProjectItem BuildingStyleProjectItem
        {
            get
            {
                var styleItemsContainer = Project.Current.GetItems<StyleProjectItem>(); //gets all Style Project Items in the current project
                _styleProjectItem = styleItemsContainer.FirstOrDefault(s => s.Name.Contains("BuildingStyle"));
                return _styleProjectItem;
            }
        }

        private Visibility _docpaneVisible = Visibility.Visible;

        /// <summary>
        /// Controls the visibility of the dockpane elements if the building footprint layer is missing.
        /// </summary>
        public Visibility DockpaneVisible
        {
            get { return _docpaneVisible; }
            set { SetProperty(ref _docpaneVisible, value, () => DockpaneVisible); }
        }

        #endregion


        #region private methods

        private async Task GetRulePackages()
        {
            try
            {
                await QueuedTask.Run(async () =>
                {
                    //building the URL to get the ruel packages.                   
                    UriBuilder searchURL =
                        new UriBuilder(_arcgisOnline)
                        {
                            Path = "sharing/rest/search"
                        };

                    EsriHttpClient httpClient = new EsriHttpClient();

                    //these are the 3 rule packages we will download for this sample
                    string rulePackage =
                        "(type:\"Rule Package\" AND (title:\"Paris Rule package 2014\" OR title:\"Venice Rule package 2014\" OR title:\"Extrude/Color/Rooftype Rule package 2014\"))&f=json";
                    searchURL.Query = string.Format("q={0}&f=json", rulePackage);

                    var searchResponse = httpClient.Get(searchURL.Uri.ToString());

                    //Parsing the JSON retrieved.
                    dynamic resultItems = JObject.Parse(await searchResponse.Content.ReadAsStringAsync());

                    long numberOfTotalItems = resultItems.total.Value;
                    if (numberOfTotalItems == 0)
                        return;

                    List<dynamic> resultItemList = new List<dynamic>();
                    resultItemList.AddRange(resultItems.results);

                    //creating the collection of Rule packages from the parsed JSON.
                    foreach (dynamic item in resultItemList)
                    {
                        var id = item.id.ToString();
                        var title = item.title.ToString();
                        var name = item.name.ToString();
                        var snippet = item.snippet.ToString();
                        var thumbnail = item.thumbnail.ToString();
                        lock (_rpkLock)
                        {
                            RulePackageCollection.Add(new RulePackage(_arcgisOnline, id, title, name, thumbnail, snippet));
                        }
                    }
                });
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        internal async void ApplyRulePackage()
        {
            await DownloadRulePackage(SelectedRulePackage);  //Download the rule package selected.
            await QueuedTask.Run(async () =>
            {
                //Get the build footprint layer's currect renderer.
                CIMSimpleRenderer renderer = (CIMSimpleRenderer)Module1.BuildingFootprintLayer.GetRenderer(); 

                //Get the rule package attributes and mapping to Feature layer from the dictionary
                var attributeExpressionMapping =
                    SelectedRulePackage.RpkAttributeExpressionMapping[SelectedRulePackage.Title];

                //Create the array of CIMPrimitiveOverrides. This is where the field\attribute mapping for the rulepackage is done.
                var primitiveOverrides = attributeExpressionMapping.Select(kvp => new CIMPrimitiveOverride
                    {
                        PrimitiveName = SelectedRulePackage.Title,
                        PropertyName = kvp.Key,
                        Expression = kvp.Value
                    }).ToArray();

                //Full path of the rule package path
                var rulePkgPath =  Path.Combine(_rulePkgPath, SelectedRulePackage.Name); 

                //Creating a procedural symbol using the rulepaackage
                var symbolReference = SymbolFactory.Instance.ConstructProceduralSymbol(rulePkgPath,
                    Module1.BuildingFootprintLayer, primitiveOverrides);

                //CIMPolygonSymbol needed to create a style item.  
                CIMPolygonSymbol polygonSymbol = symbolReference.Symbol as CIMPolygonSymbol;   
                                    
                //Set symbol's real world setting to be the same as that of the feature layer
                polygonSymbol.SetRealWorldUnits(Module1.BuildingFootprintLayer.UsesRealWorldSymbolSizes);

                //Set the current renderer to the new procedural symbol's CIMSymbolReference
                renderer.Symbol = polygonSymbol.MakeSymbolReference();

                //Set the Building footprint layer's render. 
                Module1.BuildingFootprintLayer.SetRenderer(renderer);
                
                //Create a style project item.
                await CreateStyleItem();
                if (BuildingStyleProjectItem != null && !BuildingStyleProjectItem.IsReadOnly)                
                    await AddStyleItemToStyle(BuildingStyleProjectItem, polygonSymbol); //Building footprint's procedural symbol is added to the BuildingStyle   
                                                               
            });
        }

        private async Task DownloadRulePackage(RulePackage rulePackage)
        {
            if (rulePackage == null)
                return;

            var downloadUrl = $"http://www.arcgis.com/sharing/rest/content/items/{rulePackage.ID}/data";
            var fileName = Path.Combine(_rulePkgPath, $"{rulePackage.Name}");
            EsriHttpClient esriHttpClient = new EsriHttpClient();

            await esriHttpClient.GetAsFileAsync(downloadUrl, fileName);
        }

        private Task AddStyleItemToStyle(StyleProjectItem styleProjectItem,  CIMPolygonSymbol cimPolygonSymbol)
        {
            return QueuedTask.Run(() =>
            {
                if (styleProjectItem == null || cimPolygonSymbol == null)
                {
                    throw new System.ArgumentNullException();
                }              

                SymbolStyleItem symbolStyleItem = new SymbolStyleItem()//define the symbol
                {
                    Symbol = cimPolygonSymbol,
                    ItemType = StyleItemType.PolygonSymbol,
                    Category = "BuildingFacade",
                    Name = SelectedRulePackage.Name,
                    Key = SelectedRulePackage.Name,
                    Tags = $"BuildingStyle, {SelectedRulePackage.Title}"                                        
                };

                styleProjectItem.AddItem(symbolStyleItem);
            });
        }

        private static async Task CreateStyleItem()
        {
      if (BuildingStyleProjectItem?.PhysicalPath == null)
      {
        await QueuedTask.Run(() => {
          if (File.Exists(_styleFilePath)) //check if the file is on disc. Add it to the project if it is.
            Project.Current.AddStyle(_styleFilePath);
          else //else create the style item  
          {
            if (BuildingStyleProjectItem != null)
              Project.Current.RemoveStyle(BuildingStyleProjectItem.Name); //remove style from project                           
            Project.Current.CreateStyle($@"{_styleFilePath}");
          }
        });

      }
    }


        #endregion
    }

    /// <summary>
    /// Button implementation to show the DockPane.
    /// </summary>
    internal class ProceduralSymbolWithRulePackages_ShowButton : Button
    {
        protected override void OnClick()
        {
            ProceduralSymbolWithRulePackagesViewModel.Show();
        }
    }
}
