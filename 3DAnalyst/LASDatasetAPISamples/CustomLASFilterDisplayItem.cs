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
using ArcGIS.Core.Data;
using ArcGIS.Core.Data.Analyst3D;
using ArcGIS.Core.Internal.CIM;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Internal.Mapping.Raster.RasterHistogram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace LASDatasetAPISamples
{
  /// <summary>
  /// Represents the custom item to be displayed in the combo box for Classification Codes, Return Values and Classification Flags of the LAS Dataset.
  /// </summary>
  public class CustomLASFilterDisplayItem : PropertyChangedBase
  {
    /// <summary>
    /// Constructor for Classification Code Custom item for combo box
    /// </summary>
    /// <param name="classCode"></param>
    /// <param name="isChecked"></param>
    public CustomLASFilterDisplayItem(int classCode = 1, bool isChecked = false)
    {
      if (classCode < 23)
        Name = classCode + " - " + _classificationCodeMeaning[classCode];
      if (classCode >= 23 && classCode <= 63)
        Name = classCode + " - " + "Reserved";
      if (classCode >= 64 && classCode <= 255)
        Name = classCode + " - " + "User defined";
      _classCode = classCode;

      ItemType = CustomItemType.ClassCode;
      IsChecked = isChecked;
    }
    /// <summary>
    /// Constructor for Return Type Custom item for combo box
    /// </summary>
    /// <param name="lasReturnType"></param>
    /// <param name="isChecked"></param>
    public CustomLASFilterDisplayItem(LasReturnType lasReturnType, bool isChecked)
    {
      Name = GetReturnTypeFriendlyName(lasReturnType);
      _lasReturnType = lasReturnType;
      ItemType = CustomItemType.ReturnType;
      IsChecked = isChecked;
    }
    /// <summary>
    /// Constructor for Classification Flag Custom item for combo box
    /// </summary>
    /// <param name="flag"></param>
    /// <param name="isChecked"></param>
    public CustomLASFilterDisplayItem(string flag, bool isChecked)
    {
      Name = flag;
      _flag = flag;
      ItemType = CustomItemType.Flags;
      IsChecked = isChecked;
    }

    private int _classCode;
    public int ClassCode
    {
      get => _classCode;
      set => SetProperty(ref _classCode, value, () => ClassCode);
    }
    private LasReturnType _lasReturnType;
    public LasReturnType LasReturnType
    {
      get => _lasReturnType;
      set => SetProperty(ref _lasReturnType, value, () => LasReturnType);
    }

    private string _flag;
    public string Flag
    {
      get => _flag;
      set => SetProperty(ref _flag, value, () => Flag);
    }
    private string _name;
    public string Name
    {
      get
      {
        
        return _name;
      }
      set
      {
        SetProperty(ref _name, value, () => Name);
      }
    }
    private bool _isChecked;
    public bool IsChecked
    {
      get
      {
        return _isChecked;
      }
      set
      {
        SetProperty(ref _isChecked, value, () => IsChecked);
      }

    }

    private CustomItemType _itemType;
    public CustomItemType ItemType
    {
      get => _itemType;
      set => SetProperty(ref _itemType, value, () => ItemType);
    }
    private static Dictionary<int, string> _classificationCodeMeaning = new Dictionary<int, string>
    {
        {0, "Created, Never classified"},
        {1, "Unassigned"},
        {2, "Ground"},
        {3, "Low Vegetation"},
        {4, "Medium Vegetation"},
        {5, "High Vegetation"},
        {6, "Building"},
        {7, "Low Point"},
        {8, "Model Key-Point"},
        {9, "Water"},
        {10, "Rail"},
        {11, "Road Surface"},
        {12, "Reserved"},
        {13, "Wire - Guard (Shield)"},
        {14, "Wire - Conductor (Phase)"},
        {15, "Transmission Tower"},
        {16, "Wire-Structure Connector (Insulator)"},
        {17, "Bridge Deck"},
        {18, "High Noise"},
        {19, "Reserved"},
        {20, "Ignored Ground"},
        {21, "Snow"},
        {22, "Temporal Exclusion"}
    };

    private string GetReturnTypeFriendlyName(LasReturnType lasReturnType)
    {
      switch (lasReturnType)
      {
        case LasReturnType.Return1:
          return "1" ;
        case LasReturnType.Return2:
          return "2";
        case LasReturnType.Return3:
          return "3";
        case LasReturnType.Return4:
          return "4" ;
        case LasReturnType.Return5:
          return "5" ;
        case LasReturnType.Return6:
          return "6" ;
        case LasReturnType.Return7:
          return "7" ;
        case LasReturnType.Return8:
          return "8";
        case LasReturnType.Return9:
          return "9";
        case LasReturnType.Return10:
          return "10";
        case LasReturnType.Return11:
          return "11";
        case LasReturnType.Return12:
          return "12";
        case LasReturnType.Return13:
          return "13";
        case LasReturnType.Return14:
          return "14";
        case LasReturnType.Return15:
          return "15";
        case LasReturnType.ReturnFirstOfMany:
          return "First of Many";
        case LasReturnType.ReturnLastOfMany:
          return "Last of Many";
        case LasReturnType.ReturnLast:
          return "Last";
        case LasReturnType.ReturnSingle:
          return "Single" ;
        default:
          return "Single";
      }
    }


  }
}
