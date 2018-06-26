/*

   Copyright 2018 Esri

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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Controls;
using System.Xml.Linq;
using ArcGIS.Desktop.Core.Sharing;

namespace SOEWithPropertiesTest
{
  internal class SimpleRestSOEWithPropertiesViewModel : EmbeddableControl, IServerObjectExtension
  {
    public SimpleRestSOEWithPropertiesViewModel(XElement options, bool canChangeOptions) : base(options, canChangeOptions) { }

    /// <summary>
    /// Text shown in the control.
    /// </summary>
    private string _text = "Embeddable Control";
    public string Text
    {
      get { return _text; }
      set
      {
        SetProperty(ref _text, value, () => Text);
      }
    }

    private string _layerType = "feature";
    public string LayerType
    {
      get { return _layerType; }
      set { SetProperty(ref _layerType, value, () => LayerType); }
    }

    private int _maxAllowedFeatures = 100;
    public int MaxAllowedFeatures
    {
      get { return _maxAllowedFeatures; }
      set { SetProperty(ref _maxAllowedFeatures, value, () => MaxAllowedFeatures); }
    }

    private bool _isEditable = false;
    public bool IsEditable
    {
      get { return _isEditable; }
      set { SetProperty(ref _isEditable, value, () => IsEditable); }
    }
    public string GetSOETypeName()
    {
      return "NetSimpleRESTSOEWithProperties";
    }
    public string GetSOEDisplayName()
    {
      return ".Net Simple REST SOE With Properties";
    }
    public string GetSOEServiceType()
    {
      return "Map Server";
    }

    public List<Tuple<string, string>> GetSOEProps()
    {
			List<Tuple<string, string>> props = new List<Tuple<string, string>>
			{
				new Tuple<string, string>("layerType", LayerType),
				new Tuple<string, string>("maxNumFeatures", MaxAllowedFeatures.ToString()),
				new Tuple<string, string>("isEditable", IsEditable ? "true" : "false")
			};
			return props;
    }

    public void SetSOEProps(List<Tuple<string, string>> props)
    {
      foreach (var item in props)
      {
        if (item.Item1.ToLower() == "layerType".ToLower())
          LayerType = item.Item2;
        else if (item.Item1.ToLower() == "maxNumFeatures".ToLower())
        {
          int.TryParse(item.Item2, out _maxAllowedFeatures);
          NotifyPropertyChanged(() => MaxAllowedFeatures);
        }
        else if (item.Item1.ToLower() == "isEditable".ToLower())
          IsEditable = item.Item2.ToLower() == "true";
      }
    }

    public List<Tuple<string, string>> GetSOEInfos()
    {
      throw new NotImplementedException();
    }

    public void SetSOEInfos(List<Tuple<string, string>> infos)
    {
      throw new NotImplementedException();
    }

    public string GetSOEWebCapabilities()
    {
      throw new NotImplementedException();
    }

    public void SetSOEWebCapabilities(string capabilities)
    {
      throw new NotImplementedException();
    }
  }
}
