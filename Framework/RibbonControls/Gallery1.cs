// Copyright 2017 Esri 
//
// 
//   Licensed under the Apache License, Version 2.0 (the "License"); 
//   you may not use this file except in compliance with the License. 
//   You may obtain a copy of the License at 
//
//       http://www.apache.org/licenses/LICENSE-2.0 
//
//   Unless required by applicable law or agreed to in writing, software 
//   distributed under the License is distributed on an "AS IS" BASIS, 
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//   See the License for the specific language governing permissions and 
//   limitations under the License. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace RibbonControls
{
  internal class Gallery1 : Gallery
  {
    private bool _isInitialized;

    protected override void OnDropDownOpened()
    {
      Initialize();
    }

    private void Initialize()
    {
      if (_isInitialized)
        return;

      //Add 6 items to the gallery
      for (int i = 0; i < 6; i++)
      {
        string name = string.Format("Item {0}", i);
        Add(new GalleryItem(name, this.LargeImage != null ? ((ImageSource)this.LargeImage).Clone() : null, name));
      }
      _isInitialized = true;

    }

    protected override void OnClick(GalleryItem item)
    {
      //TODO - insert your code to manipulate the clicked gallery item here
      System.Diagnostics.Debug.WriteLine("Remove this line after adding your custom behavior.");
      base.OnClick(item);
    }
  }
}
