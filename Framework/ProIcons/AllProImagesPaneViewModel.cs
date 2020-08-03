/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ProIcons
{
  internal class ProImage
  {
    public ImageSource Source {get; set;}
    public string Name { get; set; }
  }

  internal class AllProImagesPaneViewModel : Pane
  {
    private const string _viewPaneID = "ProIcons_AllProImagesPane";
    private ResourceDictionary _resournceXaml;
    private List<ProImage> _images = new List<ProImage>();
    private ICommand _searchCmd;
    private ICommand _clearCmd;
    private string _searchText;
    private bool _showingSearch;

    public AllProImagesPaneViewModel() : base()
    {
      if (FrameworkApplication.ApplicationTheme == ApplicationTheme.Dark)
        _resournceXaml = new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/DarkXamlImages.xaml") };
      else
        _resournceXaml = new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/XamlImages.xaml") };

      foreach (string key in _resournceXaml.Keys)
      {
        ImageSource imgSource = _resournceXaml[key] as ImageSource;
        if (imgSource != null)
        {
          ProImage proImage = new ProImage();
          proImage.Name = key;
          proImage.Source = imgSource;
          _images.Add(proImage);
        }
      }
      _images = _images.OrderBy(o => o.Name).ToList();

    }

    public List<ProImage> ProImages
    {
      get
      {
        return _images;
      }
    }

    /// <summary>
    /// Create a new instance of the pane.
    /// </summary>
    internal static void Create()
    {
      FrameworkApplication.Panes.Create(_viewPaneID);
    }

    #region Pane Overrides

    /// <summary>
    /// Called when the pane is initialized.
    /// </summary>
    protected async override Task InitializeAsync()
    {
      await base.InitializeAsync();
    }

    /// <summary>
    /// Called when the pane is uninitialized.
    /// </summary>
    protected async override Task UninitializeAsync()
    {
      await base.UninitializeAsync();
    }

    #endregion Pane Overrides

    public string SearchText
    {
      get { return _searchText; }
      set
      {
        SetProperty(ref _searchText, value, () => SearchText);
      }
    }

    public ICommand SearchCmd
    {
      get
      {
        if (_searchCmd == null)
          _searchCmd = new RelayCommand(new Action<object>((p) => InvokeSearchCommand()), () => { return true; }, false, false);

        return _searchCmd;
      }
    }

    public ICommand ClearCmd
    {
      get
      {
        if (_clearCmd == null)
          _clearCmd = new RelayCommand(new Action<object>((p) => InvokeClearCommand()), () => { return true; }, false, false);

        return _clearCmd;
      }
    }

    internal void InvokeClearCommand()
    {
      // Clear the text
      SearchText = string.Empty;

      // If we're actually showing results - reset
      if (_showingSearch)
        InvokeSearchCommand();

      _showingSearch = false;
    }

    internal void InvokeSearchCommand()
    {
      _showingSearch = true;
      _images.Clear();

      string iconName = _searchText;

      if (iconName.StartsWith("pack"))
        iconName = System.IO.Path.GetFileNameWithoutExtension(iconName);

      foreach (string key in _resournceXaml.Keys)
      {
        if (string.IsNullOrEmpty(iconName) || Regex.IsMatch(key, Regex.Escape(iconName), RegexOptions.IgnoreCase))
        { 
          ImageSource imgSource = _resournceXaml[key] as ImageSource;
          if (imgSource != null)
          {
            ProImage proImage = new ProImage();
            proImage.Name = key;
            proImage.Source = imgSource;
            _images.Add(proImage);
          }
        }
      }
      
      // Sort by name
      _images = _images.OrderBy(o => o.Name).ToList();
      NotifyPropertyChanged("ProImages");
    }
  }

  /// <summary>
  /// Button implementation to create a new instance of the pane and activate it.
  /// </summary>
  internal class AllProImagesPane_OpenButton : Button
  {
    protected override void OnClick()
    {
      AllProImagesPaneViewModel.Create();
    }
  }
}
