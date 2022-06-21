/*

   Copyright 2019 Esri

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
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ESRI.ArcGIS.ItemIndex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragAndDrop
{
	public class GDBBaseItem : PropertyChangedBase
	{
		private string _name;
		private string _path;
		public string Name
		{
			get { return _name; }
			set
			{
				SetProperty(ref _name, value, () => Name);
			}
		}

		public string Path
		{
			get { return _path; }
			set
			{
				SetProperty(ref _path, value, () => Path);
			}
		}

		public virtual ItemInfoValue GetItemInfoValue()
		{
			var uri = _path + @"\" + _name;
			var gdb_item = ItemFactory.Instance.Create(uri);
			if (gdb_item == null)
			{
				MessageBox.Show($@"Unable to locate: {uri} - Feature datasets are not supported.");
			}
			return new ItemInfoValue()
			{
				name = gdb_item?.Name,
				title = gdb_item?.Name,
				catalogPath = gdb_item?.Path,
				type = gdb_item?.Type,
				typeID = gdb_item?.TypeID,
				isContainer = "false"
			};
		}

		private bool _isExpanded;
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				SetProperty(ref _isExpanded, value, () => IsExpanded);
			}
		}
		internal bool _isSelected;
		public virtual bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				SetProperty(ref _isSelected, value, () => IsSelected);
			}
		}

		private List<GDBBaseItem> _children = new List<GDBBaseItem>();
		public List<GDBBaseItem> Children
		{
			get { return _children; }
			set
			{
				SetProperty(ref _children, value, () => Children);
			}
		}


	}
	public class DatabaseGDBItem : GDBBaseItem
	{
		private string _dbName;
		public string DBName
		{
			get { return _dbName; }
			set
			{
				SetProperty(ref _dbName, value, () => DBName);
			}
		}

		public override ItemInfoValue GetItemInfoValue()
		{
			var uri = Name;
			var gdb_item = ItemFactory.Instance.Create(uri);
			return new ItemInfoValue()
			{
				name = gdb_item.Name,
				title = gdb_item.Name,
				catalogPath = gdb_item.Path,
				type = gdb_item.Type,
				typeID = gdb_item.TypeID,
				isContainer = "true"
			};
		}

	}

	public class TableGDBItem : GDBBaseItem
	{


	}
	public class PointFCGDBItem : GDBBaseItem
	{


	}
	public class LineFCGDBItem : GDBBaseItem
	{


	}
	public class PolygonFCGDBItem : GDBBaseItem
	{


	}


}
