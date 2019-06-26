/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Internal.Core;
using ESRI.ArcGIS.ItemIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;

namespace ProjectCustomItemEarthQuake.Items
{
	/// <summary>
	/// Example custom project item. A custom project item is a custom item which
	/// can be persisted in a project file
	/// </summary>
	/// <remarks>
	/// As a <i>project</i> item, QuakeProjectItems can save state into the aprx. Conversely,
	/// when a project is opened that contains a persisted QuakeProjectItem, QuakeProjectItem
	/// is asked to re-hydrate itself (based on the name, catalogpath, and type that was
	/// saved in the project)</remarks>
	internal class QuakeProjectItem : ArcGIS.Desktop.Core.CustomProjectItemBase
	{
		// private static int event_count;
		protected QuakeProjectItem() : base()
		{
			this._pathSaveRelative = true;
		}
		protected QuakeProjectItem(ItemInfoValue iiv) : base(FlipBrowseDialogOnly(iiv))
		{
			_pathSaveRelative = true;
		}

		/// <summary>
		/// This constructor is called if the project item was saved into the project.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="catalogPath"></param>
		/// <param name="typeID"></param>
		/// <param name="containerTypeID"></param>
		/// <remarks>Custom project items cannot <b>not</b> be saved into the project if
		/// the user clicks (or executes) save</remarks>
		public QuakeProjectItem(string name, string catalogPath, string typeID, string containerTypeID) :
		  base(name, catalogPath, typeID, containerTypeID)
		{
			_pathSaveRelative = true;
		}

		#region Icon override

		/// <summary>
		/// Gets whether the project item can contain child items
		/// </summary>
		public override bool IsContainer => true;

		public override ImageSource LargeImage
		{
			get
			{
				return System.Windows.Application.Current.Resources["BexDog32"] as ImageSource;
			}
		}

		public override Task<ImageSource> SmallImage
		{
			get
			{
				ImageSource smallImage = System.Windows.Application.Current.Resources["BexDog16"] as ImageSource;
				if (smallImage == null) throw new ArgumentException("SmallImage for CustomProjectItem doesn't exist");
				return Task.FromResult(smallImage as ImageSource);
			}
		}

		#endregion Icon override

		#region Rename override code

		protected override bool CanRename => true;

		protected override bool OnRename(string newName)
		{
			// have to do some work to actually change the name so if they call refresh it'll be there
			// whether it's a file on disk or node in the xml
			var new_ext = System.IO.Path.GetExtension(newName);
			if (string.IsNullOrEmpty(new_ext))
			{
				new_ext = System.IO.Path.GetExtension(this.Path);
				newName = System.IO.Path.ChangeExtension(newName, new_ext);
			}

			var new_file_path = System.IO.Path.Combine(
			  System.IO.Path.GetDirectoryName(this.Path), newName);
			System.IO.File.Move(this.Path, new_file_path);
			this.Path = new_file_path;
			return base.OnRename(newName);
		}

		#endregion Rename override code

		#region Fetch override to provide 'children'

		/// <summary>
		/// Fetch is called if <b>IsContainer</b> = <b>true</b> and the project item is being
		/// expanded in the Catalog pane for the first time.
		/// </summary>
		/// <remarks>The project item should instantiate items for each of its children and
		/// add them to its child collection (see <b>AddRangeToChildren</b>)</remarks>
		public override void Fetch()
		{
			//This is where the quake item is located
			string filePath = this.Path;
			//Quake is an xml document, we parse it's content to provide the list of children
			XDocument doc = XDocument.Load(filePath);

			XNamespace aw = "http://quakeml.org/xmlns/bed/1.2";
			IEnumerable<XElement> earthquakeEvents = from el in doc.Root.Descendants(aw + "event") select el;

			List<QuakeEventCustomItem> events = new List<QuakeEventCustomItem>();
			var existingChildren = this.GetChildren().ToList();
			int event_count = 1;
			//Parse out the child quake events
			foreach (XElement earthquakeElement in earthquakeEvents)
			{
				var path = filePath + $"[{event_count}]";
				XElement desc = earthquakeElement.Element(aw + "description");
				XElement name = desc.Element(aw + "text");
				string fullName = name.Value;
				if (existingChildren.Any(i => i.Path == path)) continue;

				XElement origin = earthquakeElement.Element(aw + "origin");
				XElement time = origin.Element(aw + "time");
				XElement value = time.Element(aw + "value");
				string date = value.Value;
				DateTime timestamp = Convert.ToDateTime(date);

				//Make an "event" item for each child read from the quake file
				QuakeEventCustomItem item = new QuakeEventCustomItem(
				  fullName, path, "acme_quake_event", timestamp.ToString());
				//if (events.Any(s => s.Name == fullName))
				//    continue;
				events.Add(item);
				event_count++;
			}
			//Add the event "child" items to the children collection
			this.AddRangeToChildren(events);
		}

		#endregion Fetch override to provide 'children'

		#region Uniquely identify each custom project item

		public override ProjectItemInfo OnGetInfo()
		{
			var projectItemInfo = new ProjectItemInfo
			{
				Name = this.Name,
				Path = this.Path,
				Type = QuakeProjectItemContainer.ContainerName
			};
			return projectItemInfo;
		}

		#endregion Uniquely identify each custom project item

		#region Private Helpers

		private static ItemInfoValue FlipBrowseDialogOnly(ItemInfoValue iiv)
		{
			iiv.browseDialogOnly = "FALSE";
			return iiv;
		}

		#endregion Helpers
	}

	/// <summary>
	/// Quake event items. These are children of a QuakeProjectItem
	/// </summary>
	/// <remarks>QuakeEventCustomItems are, themselves, custom items</remarks>
	internal class QuakeEventCustomItem : CustomItemBase
	{

		public QuakeEventCustomItem(string name, string path, string type, string lastModifiedTime) : base(name, path, type, lastModifiedTime)
		{
			this.DisplayType = "QuakeEvent";
			//this.ContextMenuID = "QuakeProjectItem_ContextMenu";
		}

		public void SetNewName(string newName)
		{
			this.Name = newName;
			NotifyPropertyChanged("Name");
			this.Title = newName;
			NotifyPropertyChanged("Title");
			this._itemInfoValue.name = newName;
			this._itemInfoValue.description = newName;			
		}

		public override ImageSource LargeImage
		{
			get
			{
				return System.Windows.Application.Current.Resources["T-Rex32"] as ImageSource;
			}
		}

		public override Task<ImageSource> SmallImage
		{
			get
			{
				ImageSource img = System.Windows.Application.Current.Resources["T-Rex16"] as ImageSource;
				return Task.FromResult(img);
			}
		}
	}
}
