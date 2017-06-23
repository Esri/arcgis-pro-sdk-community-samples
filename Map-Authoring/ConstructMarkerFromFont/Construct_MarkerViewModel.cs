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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ArcGIS.Core.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;


namespace ConstructMarkerFromFont
{
  /// <summary>
  /// View model for the Construct marker from Fonts dockpane
  /// </summary>
  internal class Construct_MarkerViewModel : DockPane
  {
    private const string _dockPaneID = "ConstructMarkerFromFont_Construct_Marker";
    private const string _menuID = "ConstructMarkerFromFont_Construct_Marker_Menu";
    private static string _styleFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\AppData\Roaming\ESRI\ArcGISPro\FontMarkers.stylx";

    /// <summary>
    /// Constructor
    /// </summary>
    protected Construct_MarkerViewModel()
    {

      //command apply the marker symbol to the point feature
      _applyFontMarkerCmd = new RelayCommand(() => ApplyFontAsMarker(), () => true);
    }

    /// <summary>
    /// Override to implement custom initialization code for this dockpane
    /// </summary>
    /// <returns></returns>
    protected override Task InitializeAsync()
    {
      //initializing
      Size = 12;
      InitializeFontFamilyList();
      InitializeTypefaceList();
      DockpaneVisible = Visibility.Collapsed; //Visibilty is controlled from the Module class.

      return base.InitializeAsync();
    }

    #region Properties
    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "";
    public string Heading
    {
      get
      {
        return _heading;
      }
      set
      {
        SetProperty(ref _heading, value, () => Heading);
      }
    }


    private ICollection<FontFamilyListItem> _familyCollection;          // see FamilyCollection property
                                                                        /// <summary>
                                                                        /// Collection of font families to display in the font family list. By default this is Fonts.SystemFontFamilies,
                                                                        /// but a client could set this to another collection returned by Fonts.GetFontFamilies, e.g., a collection of
                                                                        /// application-defined fonts.
                                                                        /// </summary>
    public ICollection<FontFamilyListItem> FontFamilyCollection
    {
      get
      {
        return _familyCollection;
      }

      set
      {
        SetProperty(ref _familyCollection, value, () => FontFamilyCollection);
      }
    }

    private FontFamilyListItem _selectedFontFamilyListItem;
    /// <summary>
    /// Holds the selected Font
    /// </summary>
    public FontFamilyListItem SelectedFontFamily
    {
      get
      {

        return _selectedFontFamilyListItem;
      }
      set
      {

        SetProperty(ref _selectedFontFamilyListItem, value, () => SelectedFontFamily);
        InitializeTypefaceList();
      }
    }

    private ICollection<TypefaceListItem> _typeFaceCollection;
    /// <summary>
    /// Collection of typefaces to display in the Type face list. 
    /// </summary>
    public ICollection<TypefaceListItem> TypeFaceCollection
    {
      get
      {
        return _typeFaceCollection;
      }

      set
      {

        SetProperty(ref _typeFaceCollection, value, () => TypeFaceCollection);
      }
    }

    private TypefaceListItem _selectedTypeFaceListItem;
    /// <summary>
    /// Holds the selected Style. 
    /// </summary>
    public TypefaceListItem SelectedTypeFace
    {
      get
      {
        if (_selectedCharacter == null)
          _selectedCharacter = new CharacterItem('A');
        return _selectedTypeFaceListItem;
      }
      set
      {

        SetProperty(ref _selectedTypeFaceListItem, value, () => SelectedTypeFace);
      }
    }


    private ICollection<CharacterItem> _characterItem = new List<CharacterItem>();
    /// <summary>
    /// Collection of Characters to display in the listbox.
    /// </summary>
    public ICollection<CharacterItem> CharacterItemCollection
    {
      get
      {
        if (_characterItem.Count == 0)
        {
          for (int iChar = 32; iChar < 128; iChar++) //the ascii of the chars we want to display
          {
            _characterItem.Add(new CharacterItem(((char)iChar))); //adding display of char
          }
        }
        return _characterItem;
      }
      set
      {
        SetProperty(ref _characterItem, value, () => CharacterItemCollection);
      }
    }

    private CharacterItem _selectedCharacter;
    /// <summary>
    /// Holds the selected character
    /// </summary>
    public CharacterItem SelectedCharacter
    {
      get
      {
        return _selectedCharacter;
      }
      set
      {

        SetProperty(ref _selectedCharacter, value, () => SelectedCharacter);

      }
    }

    private int _size;
    /// <summary>
    /// Holds the selected size for the marker
    /// </summary>
    public int Size
    {
      get { return _size; }

      set
      {
        SetProperty(ref _size, value, () => Size);
      }
    }

    private FeatureLayer _pointFeatureLayer;
    /// <summary>
    /// Holds the point feature layer that will be rendered with the selected character.
    /// </summary>
    public FeatureLayer PointFeatureLayer
    {
      get { return _pointFeatureLayer; }
      set
      {
        SetProperty(ref _pointFeatureLayer, value, () => PointFeatureLayer);
      }
    }

    private Visibility _dockpaneVisible = Visibility.Visible;
    /// <summary>
    /// Controls the visible state of the controls on the dockpane
    /// </summary>
    public Visibility DockpaneVisible
    {
      get
      {
        return _dockpaneVisible;
      }
      set { SetProperty(ref _dockpaneVisible, value, () => DockpaneVisible); }
    }

    private bool _isFavorites = false;

    public bool IsFavorites
    {
      get { return _isFavorites; }
      set { SetProperty(ref _isFavorites, value, () => _isFavorites); }
    }

    private static StyleProjectItem _styleProjectItem = null;

    public static StyleProjectItem FontMarkerStyleProjectItem
    {
      get
      {
        var styleItemsContainer = Project.Current.GetItems<StyleProjectItem>(); //gets all Style Project Items in the current project
        _styleProjectItem = styleItemsContainer.FirstOrDefault(s => s.Name.Contains("FontMarkers"));
        return _styleProjectItem;
      }
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Returns the selected point layer in the TOC. 
    /// </summary>
    public void GetSelectedPointFeatureLayer()
    {
      var layers =
          MapView.Active.GetSelectedLayers()?
              .OfType<FeatureLayer>()
              .Where(fl => fl.ShapeType == esriGeometryType.esriGeometryPoint);

      PointFeatureLayer = layers?.FirstOrDefault();
      Heading = PointFeatureLayer?.Name;

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
    /// <summary>
    /// Gets the System Fonts to build the collection.
    /// </summary>
    private void InitializeFontFamilyList()
    {
      ICollection<FontFamily> familyCollection = Fonts.SystemFontFamilies;
      if (familyCollection != null)
      {
        FontFamilyListItem[] items = new FontFamilyListItem[familyCollection.Count];

        int i = 0;

        foreach (FontFamily family in familyCollection)
        {

          items[i++] = new FontFamilyListItem(family);
        }

        Array.Sort<FontFamilyListItem>(items);

        FontFamilyCollection = new Collection<FontFamilyListItem>(items);
        SelectedFontFamily = FontFamilyCollection.FirstOrDefault();
      }
    }
    /// <summary>
    /// Gets the typefaces available for the selected font. 
    /// </summary>
    /// <remarks>BoldSimulated and ObliqueSimulated font styles are filtered out of this collection. ArcGIS Pro does not render these styles.</remarks>
    private void InitializeTypefaceList()
    {
      FontFamily family = new FontFamily();
      if (SelectedFontFamily != null)
        family = SelectedFontFamily.Font;
      if (family != null)
      {
        ICollection<Typeface> faceCollection = family.GetTypefaces(); //family is a FontFamily object

        List<TypefaceListItem> items = new List<TypefaceListItem>();


        foreach (Typeface face in faceCollection)
        {
          if ((face.IsBoldSimulated) || (face.IsObliqueSimulated))
            continue;
          items.Add(new TypefaceListItem(face));
        }

        items.Sort();

        TypeFaceCollection = new Collection<TypefaceListItem>(items);
        SelectedTypeFace = TypeFaceCollection.FirstOrDefault();

      }
    }

    /// <summary>
    /// Method for applying the selected character to the point feature layer
    /// </summary>
    /// <returns></returns>
    private async Task ApplyFontAsMarker()
    {

      var charIndex = 0;
      charIndex = SelectedCharacter.Character;
      var fontName = SelectedFontFamily.ToString();
      var styleName = SelectedTypeFace.ToString();

      if (MapView.Active != null)
        GetSelectedPointFeatureLayer(); //the selected point feature layer in the TOC

      var cimMarker = SymbolFactory.Instance.ConstructMarker(charIndex, fontName,
          styleName, Size); //creating the marker from the Font selected

      var pointSymbolFromMarker = SymbolFactory.Instance.ConstructPointSymbol(cimMarker);
      //create a symbol from the marker            

      await SetFeatureLayerSymbolAsync(PointFeatureLayer, pointSymbolFromMarker);

      if (IsFavorites)
      {
        await CreateStyleItem();
        if (FontMarkerStyleProjectItem != null && pointSymbolFromMarker != null && !FontMarkerStyleProjectItem.IsReadOnly)
          await AddStyleItemToStyle(FontMarkerStyleProjectItem, pointSymbolFromMarker); //selected marker is added to the FontMarker style
      }
    }
    private static async Task CreateStyleItem()
    {
      if (FontMarkerStyleProjectItem?.PhysicalPath == null)
      {
        await QueuedTask.Run(() =>
         {
           if (File.Exists(_styleFilePath)) //check if the file is on disc. Add it to the project if it is.
            Project.Current.AddStyle(_styleFilePath);
           else //else create the style item  
          {
             if (FontMarkerStyleProjectItem != null)
               Project.Current.RemoveStyle(FontMarkerStyleProjectItem.Name); //remove style from project                           
            Project.Current.CreateStyle($@"{_styleFilePath}");
           }
         });
      }
    }
    private Task SetFeatureLayerSymbolAsync(FeatureLayer ftrLayer, CIMSymbol symbolToApply)
    {
      if (ftrLayer == null || symbolToApply == null)
        throw new System.ArgumentNullException();

      return QueuedTask.Run(() =>
      {

        //Get simple renderer from the feature layer
        CIMSimpleRenderer currentRenderer = ftrLayer.GetRenderer() as CIMSimpleRenderer;
        if (currentRenderer == null)
          return;

        //Set symbol's real world setting to be the same as that of the feature layer
        symbolToApply.SetRealWorldUnits(ftrLayer.UsesRealWorldSymbolSizes);

        //Update the symbol of the current simple renderer
        currentRenderer.Symbol = symbolToApply.MakeSymbolReference();
        //Update the feature layer renderer
        ftrLayer.SetRenderer(currentRenderer);
      });
    }

    private Task AddStyleItemToStyle(StyleProjectItem styleProjectItem, CIMPointSymbol cimPointSymbol)
    {
      return QueuedTask.Run(() =>
    {
      if (styleProjectItem == null || cimPointSymbol == null)
      {
        throw new System.ArgumentNullException();
      }
      SymbolStyleItem symbolStyleItem = new SymbolStyleItem() //define the symbol
             {
        Symbol = cimPointSymbol,
        ItemType = StyleItemType.PointSymbol,
        Category = $"{SelectedFontFamily}",
        Name = $"{SelectedCharacter.Character.ToString()}",
        Key = $"{SelectedCharacter.Character.ToString()}_{SelectedFontFamily}_3",
        Tags = $"{SelectedFontFamily};{SelectedCharacter.Character.ToString()};point"
      };

      styleProjectItem.AddItem(symbolStyleItem);
    });
    }

    #endregion

    #region Commands

    ///<summary>
    /// Command to apply the Font marker as the layer's symbol
    /// </summary>
    private ICommand _applyFontMarkerCmd;

    public ICommand ApplyFontMarkerCmd
    {
      get { return _applyFontMarkerCmd; }
    }

    #endregion

  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
  internal class Construct_Marker_ShowButton : Button
  {
    protected override void OnClick()
    {
      Construct_MarkerViewModel.Show();
    }
  }
}
