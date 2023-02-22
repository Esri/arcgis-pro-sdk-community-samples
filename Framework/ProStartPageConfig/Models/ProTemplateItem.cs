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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Dialogs;
using ProStartPageConfig.Helpers;
using ProStartPageConfig.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProStartPageConfig.Models
{

  public class ProTemplateItem
  {

    public static Dictionary<TemplateType, string> ImageUriKeys = new Dictionary<TemplateType, string>();

    static ProTemplateItem()
    {
      ImageUriKeys.Add(TemplateType.Catalog, "CatalogShowTree48");
      ImageUriKeys.Add(TemplateType.GlobalScene, "ArcGlobe48");
      ImageUriKeys.Add(TemplateType.LocalScene, "ArcScene48");
      ImageUriKeys.Add(TemplateType.Map, "Map48");
      ImageUriKeys.Add(TemplateType.Untitled, "Launch48");
    }

    public string Name { get; set; } = "";
    public TemplateType TemplateType { get; set; }

    private ImageSource _imageSource = null;
    public ImageSource ImageSource
    {
      get
      {
        if (_imageSource == null)
          _imageSource = ProImageProvider.Instance[ImageUriKeys[this.TemplateType]];
        return _imageSource;
      }
    }

    private ICommand _templateAction = null;

    public ICommand TemplateAction
    {
      get
      {
        if (this._templateAction == null)
        {
          _templateAction = new RelayCommand((obj) =>
          {
            ProStartPageHelper.CreateWithTemplateType(this.TemplateType);
          }, () => true);
        }
        return _templateAction;
      }
    }

  }
}
