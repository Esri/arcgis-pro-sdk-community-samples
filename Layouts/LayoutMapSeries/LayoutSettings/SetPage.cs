/*

   Copyright 2017 Esri

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

using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace LayoutMapSeries.LayoutSettings
{
	[DataContract]
	internal class SetPage
	{
		[DataMember(Name = "Name")]
		public string Name { get; set; }
		[DataMember(Name = "LayoutName")]
		public string LayoutName { get; set; }
		[DataMember(Name = "MapFrameName")]
		public string MapFrameName { get; set; }
		[DataMember(Name = "Width")]
		internal double Width { get; set; }
		[DataMember(Name = "Height")]
		internal double Height { get; set; }
		[DataMember(Name = "LinearUnit")]
		internal LinearUnit LinearUnit { get; set; }

    // computed fields:

    internal double HeightPartsMarginalia => Height / 4;
    internal double XWidthMapMarginalia => Width / 5;
    internal double MarginLayout => Width / 25;
    internal double XOffsetMapMarginalia => Width - (XWidthMapMarginalia + MarginLayout);
    internal double WidthMap => Width - MarginLayout - XWidthMapMarginalia - MarginLayout/2;
    internal double WidthLegend => XWidthMapMarginalia - MarginLayout;
    internal double YOffsetSymbol => 0.5;
    
    /// <summary>
    /// Get a SetPage object from a json string
    /// </summary>
    /// <param name="json"></param>
    /// <returns>SetPage object</returns>
    internal static SetPage FromJson(string json)
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return (SetPage)serializer.Deserialize(json, typeof(SetPage));
		}
		/// <summary>
		/// Convert object to json string
		/// </summary>
		/// <returns>json string of object</returns>
		internal string ToJson()
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Serialize(this);
		}
	}

	[DataContract]
	internal class SetPages
	{
		[DataMember(Name = "SetPageList")]
		internal IList<SetPage> SetPageList { get; set; }

		internal SetPages()
		{
			SetPageList = new List<SetPage>
			{
				new SetPage() { Name = "Railroad - A4 - Portrait", MapFrameName = "Railroad Map Frame", LayoutName = "Railroad Map Series", Width = 29.7, Height = 21.0, LinearUnit = LinearUnit.Centimeters },
				new SetPage() { Name = "Railroad - A3 - Portrait", MapFrameName = "Railroad Map Frame", LayoutName = "Railroad Map Series", Width = 42, Height = 29.7, LinearUnit = LinearUnit.Centimeters },
				new SetPage() { Name = "Railroad - A2 - Portrait", MapFrameName = "Railroad Map Frame", LayoutName = "Railroad Map Series", Width = 59.4, Height = 42, LinearUnit = LinearUnit.Centimeters }
			};

		}
		/// <summary>
		/// Get a SetPage object from a json string
		/// </summary>
		/// <param name="json"></param>
		/// <returns>SetPage object</returns>
		internal static SetPage FromJson(string json)
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return (SetPage)serializer.Deserialize(json, typeof(SetPage));
		}
		/// <summary>
		/// Convert object to json string
		/// </summary>
		/// <returns>json string of object</returns>
		internal string ToJson()
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			return serializer.Serialize(this);
		}
	}
}
