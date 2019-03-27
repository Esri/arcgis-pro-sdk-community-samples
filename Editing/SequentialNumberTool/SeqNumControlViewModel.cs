//   Copyright 2019 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

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
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using ArcGIS.Desktop.Mapping;
using System.Xml.Linq;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data;

namespace SeqNum
{
  internal class SeqNumControlViewModel : EmbeddableControl
  {
    public SeqNumControlViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions)
    {
    }

    public List<String> layerList
    {
      get { return Module1._layerList; }
      set { SetProperty(ref Module1._layerList, value); }
    }

    public List<String> fieldList
    {
      get { return Module1._fieldList; }
      set { SetProperty(ref Module1._fieldList, value); }
    }

    public String targetLayer
    {
      get { return Module1._targetLayer; }
      set
      {
        if (value == Module1._targetLayer) return;

        SetProperty(ref Module1._targetLayer, value);

        //if targetlayer is null for some reason then also null field dropdown
        if (value == null)
        {
          fieldList = null;
          return;
        }

        //get field list for the selected target layer
        QueuedTask.Run(() =>
        {
          var featLayer = MapView.Active.Map.FindLayers(Module1._targetLayer).First() as BasicFeatureLayer;
          var flf = featLayer.GetTable().GetDefinition().GetFields();

          fieldList = flf.Where(f => f.FieldType == FieldType.Integer | f.FieldType == FieldType.SmallInteger |
            f.FieldType == FieldType.String).Select(f => f.Name).ToList();
        });
      }
    }

    public String targetField
    {
      get { return Module1._targetField; }
      set
      {
        if (value == null) return;
        SetProperty(ref Module1._targetField, value);
        //is the targetfield a string (cache this value?)
        QueuedTask.Run(() =>
        {
          var featLayer = MapView.Active.Map.FindLayers(Module1._targetLayer).First() as BasicFeatureLayer;
          var flf = featLayer.GetTable().GetDefinition().GetFields();
          SetProperty(ref Module1._isTargetFieldString, flf.First(f => f.Name == value).FieldType == FieldType.String);
          NotifyPropertyChanged("isStringFormatEnabled");
        });
      }
    }

    public string stringFormat
    {
      get { return Module1._stringFormat; }
      set { SetProperty(ref Module1._stringFormat, value); }
    }


    public string formatToolTip =>
      "The format represents how the sequential number is written to the textfield" + "\n" +
      "Use a # to denote the incrementing sequential number" + "\n" +
      "Multiple # will add leading 0's" + "\n" +
      //"e.g." + "\n" +
      "Format      Text" + "\n" +
      "ABC-#       ABC-1" + "\n" +
      "ABC-###  ABC-001";

    public bool isStringFormatEnabled
    {
      get { return Module1._isTargetFieldString; }
    }

    public String startValue
    {
      get {return Module1._startValue;}
      set { SetProperty(ref Module1._startValue, value); }
    }
    
    public  String incValue
    {
      get { return Module1._incValue; }
      set { SetProperty(ref Module1._incValue, value); }
    }
  }
}
