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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;

namespace CustomIdentify
{
  /// <summary>
  /// Implementation of a custom popup content class
  /// 
  /// </summary>
  internal class DynamicPopupContent : PopupContent
  {
	private Dictionary<FieldDescription, double> _values = new Dictionary<FieldDescription, double>();
	private long _id;
	private RelateInfo _relateInfo;

	/// <summary>
	/// Constructor initializing the base class with the layer and object id associated with the pop-up content
	/// </summary>
	//public DynamicPopupContent(MapMember mapMember, long id, List<HierarchyRow> hierarchyRows) : base(mapMember, id)
	public DynamicPopupContent(MapMember mapMember, long id) : base(mapMember, id)
	{
	  //Set property indicating the html content will be generated on demand when the content is viewed.
	  IsDynamicContent = true;
	  _id = id;//save our id
	}

	/// <summary>
	/// Called the first time the pop-up content is viewed. This is good practice when you may show a pop-up for multiple items at a time. 
	/// This allows you to delay generating the html content until the item is actually viewed.
	/// </summary>
	protected override Task<string> OnCreateHtmlContent()
	{
	  return QueuedTask.Run(() =>
	  {
		var invalidPopup = "<p>Pop-up content could not be generated for this feature.</p>";
		if (!(MapMember is BasicFeatureLayer layer))
		  return invalidPopup;

		List<HierarchyRow> completeHierarchyRows = new List<HierarchyRow>();
		var gdb = layer.GetTable().GetDatastore() as Geodatabase;
		var fcName = layer.GetTable().GetName();
		if (_relateInfo == null)
		  _relateInfo = new RelateInfo();
		var newRow = _relateInfo.GetRelationshipChildren(layer, gdb, fcName, _id);
		completeHierarchyRows.Add(newRow);

		//Using our html template we construct a new html string that we return through OnCreateHtmlContent
		var sb = new StringBuilder();
		sb.Append(new JavaScriptSerializer().Serialize(completeHierarchyRows));
		string rootType = completeHierarchyRows[0].type;
		sb.Replace(@"""children"":[],", string.Empty);

		//Get the html from the template file on disk that we have packaged with our add-in.
		var htmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "template.html");
		var html = File.ReadAllText(htmlPath);

		//Update the template with our custom html and return it to be displayed in the pop-up window.
		html = html.Replace("insert root layer here", layer.Name);
		html = html.Replace("'insert data here'", sb.ToString());
		html = html.Replace("insert root type field here", rootType);
		return html;
	  });
	}
  }
}
