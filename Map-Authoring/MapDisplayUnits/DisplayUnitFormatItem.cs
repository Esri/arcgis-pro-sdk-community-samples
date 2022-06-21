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
using ArcGIS.Desktop.Core.UnitFormats;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MapUnits
{
  public class DisplayUnitFormatItem : PropertyChangedBase
  {
    private string _name;
    private string _code;
    private string _unitCode;
    private string _plural;
    private string _abbreviation;
    private string _format;
    private string _nameAbbreviation;
    private string _projectDefault;
    private bool _isDefaultFormat;
    private DisplayUnitFormat _displayUnitFormat; //radians, feet, meters, etc
    private UnitFormatType _unitFormatType; //Distance, Location, Angular....
    public DisplayUnitFormatItem(UnitFormatType unitFormatType, DisplayUnitFormat displayUnitFormat, bool isDefaultUnitFormat)
    {
      _displayUnitFormat = displayUnitFormat;
      _unitFormatType = unitFormatType;
      _name = displayUnitFormat.DisplayName;
      _code = displayUnitFormat.MeasurementUnit.ToString();
      _unitCode = displayUnitFormat.UnitCode.ToString();
      _plural = displayUnitFormat.DisplayNamePlural;
      _abbreviation = displayUnitFormat.Abbreviation;
      _format = displayUnitFormat.FormatValue(12345.12);
      _isDefaultFormat = isDefaultUnitFormat;
      _nameAbbreviation = $"{_name} ({_abbreviation})";
      _projectDefault = _isDefaultFormat ? "Project Default" : string.Empty;
    }

    public void SetDefaultDisplayUnit()
    {
      DisplayUnitFormats.Instance.SetDefaultProjectUnitFormat(this._displayUnitFormat);
    }
    private ICommand _makeDefaultCommand;
    public ICommand MakeDefaultCommand
    {
      get
      {
        _makeDefaultCommand = new RelayCommand(() => MakeDisplayUnitAsDefault(), () => { return !_isDefaultFormat; });
        return _makeDefaultCommand;
      }
    }    

   private void MakeDisplayUnitAsDefault()
    {
      MessageBox.Show("Make Default");
      QueuedTask.Run( () => { DisplayUnitFormats.Instance.SetDefaultProjectUnitFormat(this._displayUnitFormat); });      
    }
    public string Name
    {
      get { return _name; }
      set
      {
        SetProperty(ref _name, value, () => Name);
      }
    }
    public string Code
    {
      get { return _code; }
      set
      {
        SetProperty(ref _code, value, () => Code);
      }
    }
    public string UnitCode
    {
      get { return _unitCode; }
      set
      {
        SetProperty(ref _unitCode, value, () => UnitCode);
      }
    }
    public string Plural
    {
      get { return _plural; }
      set
      {
        SetProperty(ref _plural, value, () => Plural);
      }
    }

    public string Abbreviation
    {
      get { return _abbreviation; }
      set
      {
        SetProperty(ref _abbreviation, value, () => Abbreviation);
      }
    }
    public string Format
    {
      get { return _format; }
      set
      {
        SetProperty(ref _format, value, () => Format);
      }
    }
    public string NameAbbreviation
    {
      get { return _nameAbbreviation; }
      set
      {
        SetProperty(ref _nameAbbreviation, value, () => NameAbbreviation);
      }
    }

    public string ProjectDefault
    {
      get { return _projectDefault; }
      set
      {
        SetProperty(ref _projectDefault, value, () => ProjectDefault);
      }
    }
    public bool IsDefaultFormat
    {
      get { return _isDefaultFormat; }
      set
      {
        SetProperty(ref _isDefaultFormat, value, () => IsDefaultFormat);
      }
    }
  }
}
