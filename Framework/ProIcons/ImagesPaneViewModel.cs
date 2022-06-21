/*

   Copyright 2020 Esri

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ProIcons
{
  internal class ImagesPaneViewModel : Pane
  {
    private const string _viewPaneID = "ProIcons_ImagesPane";
    private ICommand _searchCmd;
    private string _searchText;
    ResourceDictionary _resourncePng;
    ResourceDictionary _resourncePngDark;
    ResourceDictionary _resournceXaml;
    ResourceDictionary _resournceXamlDark;

    /// <summary>
    /// Consume the passed in CIMView. Call the base constructor to wire up the CIMView.
    /// </summary>
    public ImagesPaneViewModel() : base()
    {
      _resourncePng = new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/Images.xaml") };
      _resourncePngDark = new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/ImagesDark.xaml") };
      _resournceXaml = new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/XamlImages.xaml") };
      _resournceXamlDark = new ResourceDictionary() { Source = new Uri("pack://application:,,,/ArcGIS.Desktop.Resources;component/DarkXamlImages.xaml") };
    }

    /// <summary>
    /// Create a new instance of the pane.
    /// </summary>
    internal static void Create()
    {
      FrameworkApplication.Panes.Create(_viewPaneID);
    }

    #region Pane Overrides

    public ICommand SearchCmd
    {
      get
      {
        if (_searchCmd == null)
          _searchCmd = new RelayCommand(new Action<object>((p) => InvokeSearchCommand()), () => { return true; }, false, false);

        return _searchCmd;
      }
    }

    internal void InvokeSearchCommand()
    {
      if (string.IsNullOrEmpty(_searchText))
        return;

      ImageSource png = _resourncePng[_searchText] as ImageSource;
      SearchText = Search(_searchText);

      Update();
    }

    private double _scale = 1.0;
    public double Scale
    {
      get { return _scale; }
      set
      {
        _scale = value;
        NotifyPropertyChanged(() => Scale);
      }
    }

    private void Update()
    {
      if (string.IsNullOrEmpty(_searchText))
        return;

      ImageSource png = _resourncePng[_searchText] as ImageSource;
      ImageSource pngDark = _resourncePngDark[_searchText] as ImageSource;
      ImageSource xaml = _resournceXaml[_searchText] as ImageSource;
      ImageSource xamlDark = _resournceXamlDark[_searchText] as ImageSource;

      if (png != null)
        this.Size = png.Height;
      Png = png;
      PngDark = pngDark;
      Xaml = xaml;
      XamlDark = xamlDark;
    }

    private string Search(string iconName)
    {
      _targets.Clear();
      foreach (string key in _resournceXaml.Keys)
      {
        if (string.IsNullOrEmpty(iconName) || Regex.IsMatch(key, Regex.Escape(iconName), RegexOptions.IgnoreCase))
        {
          _targets.Add(key);
        }
      }

      foreach (string key in _resournceXaml.Keys)
      {
        if (string.IsNullOrEmpty(iconName) || Regex.IsMatch(key, Regex.Escape(iconName), RegexOptions.IgnoreCase))
        {
          return key;
        }
      }

      return string.Empty;
    }

    public string SelectedItem
    {
      get { return _searchText; }
      set
      {
        SearchText = value;
        Update();
      }
    }

    private ImageSource _png;
    private ImageSource _pngDark;
    private ImageSource _xaml;
    private ImageSource _xamlDark;

    private double _size;
    public double Size
    {
      get
      {
        return _size;
      }
      set
      {
        _size = value;
        NotifyPropertyChanged(() => Size);
      }
    }

    public ImageSource Png
    {
      get
      {
        return _png;
      }
      set
      {
        _png = value;
        NotifyPropertyChanged(() => Png);
      }
    }

    public ImageSource PngDark
    {
      get
      {
        return _pngDark;
      }
      set
      {
        _pngDark = value;
        NotifyPropertyChanged(() => PngDark);
      }
    }

    public ImageSource Xaml
    {
      get
      {
        return _xaml;
      }
      set
      {
        _xaml = value;
        NotifyPropertyChanged(() => Xaml);
      }
    }

    public ImageSource XamlDark
    {
      get
      {
        return _xamlDark;
      }
      set
      {
        _xamlDark = value;
        NotifyPropertyChanged(() => XamlDark);
      }
    }

    public string SearchText
    {
      get { return _searchText; }
      set
      {
        _searchText = value;
        NotifyPropertyChanged(() => SearchText);
      }
    }

    private ObservableCollection<string> _targets = new ObservableCollection<string>();

    public ObservableCollection<String> Targets
    {
      get 
      { 
        return _targets; 
      }
    }

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
  }

  /// <summary>
  /// Button implementation to create a new instance of the pane and activate it.
  /// </summary>
  internal class ImagesPane_OpenButton : ArcGIS.Desktop.Framework.Contracts.Button
  {
    protected override void OnClick()
    {
      ImagesPaneViewModel.Create();
    }
  }
}
