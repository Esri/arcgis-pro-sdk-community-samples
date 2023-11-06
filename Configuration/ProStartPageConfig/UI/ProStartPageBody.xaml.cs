/*

   Copyright 2023 Esri

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
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ProStartPageConfig.Helpers;
using ProStartPageConfig.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProStartPageConfig.UI
{
  /// <summary>
  /// Interaction logic for ProStartPageBody.xaml
  /// </summary>
  public partial class ProStartPageBody : UserControl
  {

    public event PropertyChangedEventHandler PropertyChanged = delegate { };

    private List<ProTemplateItem> _proTemplates = null;
    private List<ProTemplateItem> _proTemplates2 = null;
    public ProStartPageBody()
    {
      InitializeComponent();
      (this.BodyContent as FrameworkElement).DataContext = this;

      _proTemplates = new List<ProTemplateItem>();
      _proTemplates2 = new List<ProTemplateItem>();

      _proTemplates.Add(new ProTemplateItem
      {
        Name = "Map",
        TemplateType = TemplateType.Map
      });
      _proTemplates.Add(new ProTemplateItem
      {
        Name = "Catalog",
        TemplateType = TemplateType.Catalog
      });
      _proTemplates.Add(new ProTemplateItem
      {
        Name = "Global Scene",
        TemplateType = TemplateType.GlobalScene
      });
      _proTemplates.Add(new ProTemplateItem
      {
        Name = "Local Scene",
        TemplateType = TemplateType.LocalScene
      });

      _proTemplates2.Add(new ProTemplateItem
      {
        Name = "Start without a template",
        TemplateType = TemplateType.Untitled
      });
    }

    private ICommand _resourcesCmd = null;

    public ICommand ResourcesCommand
    {
      get
      {
        if (_resourcesCmd == null)
        {
          _resourcesCmd = new RelayCommand(
            (param) => Module1.Current.ProStartPageViewModel.Navigate(param.ToString()),
            () => true);
        }
        return _resourcesCmd;
      }
    }

    public List<ProTemplateItem> Templates => _proTemplates;

    public List<ProTemplateItem> Templates2 => _proTemplates2;

    private ProTemplateItem _selectedTemplateItem = null;
    private ProTemplateItem _selectedTemplateItem2 = null;

    public ProTemplateItem SelectedTemplateItem
    {
      get
      {
        return _selectedTemplateItem;
      }
      set
      {
        if (value != _selectedTemplateItem)
        {
          _selectedTemplateItem = value;
          _selectedTemplateItem?.TemplateAction.Execute(null);
        }
      }
    }

    public ProTemplateItem SelectedTemplateItem2
    {
      get
      {
        return _selectedTemplateItem2;
      }
      set
      {
        if (value != _selectedTemplateItem2)
        {
          _selectedTemplateItem2 = value;
          _selectedTemplateItem2?.TemplateAction.Execute(null);
        }
      }
    }


    private ImageSource _imgSource = null;

    public ImageSource ResourcesButtonImage
    {
      get
      {
        if (_imgSource == null)
        {
          var url = @"pack://application:,,,/ProStartPageConfig;component/Images/LearningResources.png";
          //var uri = 
          if (FrameworkApplication.ApplicationTheme == ApplicationTheme.Dark ||
          FrameworkApplication.ApplicationTheme == ApplicationTheme.HighContrast)
            url = @"pack://application:,,,/ProStartPageConfig;component/Images/LearningResourcesDark.png";
          var bmp = new BitmapImage();
          bmp.BeginInit();
          bmp.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
          bmp.EndInit();
          _imgSource = bmp;
        }
        return (_imgSource);
      }
    }

    public ImageSource OpenFolderImage => ProImageProvider.Instance["FolderOpenState16"];

    private bool IsDesignMode
    {
      //http://stackoverflow.com/questions/12917566/detecting-design-mode-using-wpf-in-a-static-method
      get { return DesignerProperties.GetIsInDesignMode(this); }
    }

    protected virtual void NotifyPropertyChanged([CallerMemberName] string propName = "")
    {
      PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }


    // this command is called when a project is selected from the RecentProjectsControl on the ProStartPageBody pange   
    private RelayCommand _projectChosenCmd;
    public ICommand ProjectChosenCmd
    {
      get
      {
        if (_projectChosenCmd == null)
          _projectChosenCmd = new RelayCommand((param) => OnProjectChosen(param), (param) => true);

        return _projectChosenCmd;
      }
    }

    private void OnProjectChosen(object param)
    {
      var path = param as string;
      if (string.IsNullOrEmpty(path))
        return;

      // open the project
      Project.OpenAsync(path);
    }


    // this command is called when a template is selected from the RecentTemplatesControl on the ProStartPageBody pange   
    private RelayCommand _templateChosenCmd;
    public ICommand TemplateChosenCmd
    {
      get
      {
        if (_templateChosenCmd == null)
          _templateChosenCmd = new RelayCommand((param) => OnTemplateChosen(param), (param) => true);

        return _templateChosenCmd;
      }
    }

    private void OnTemplateChosen(object param)
    {
      var path = param as string;
      if (string.IsNullOrEmpty(path))
        return;

      // create project with the template
      ProStartPageHelper.CreateWithTemplate(path);
    }
  }
}
