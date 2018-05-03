using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentFileExplorer.Helpers
{
	public class GdbItem : FileItemBase
	{
		private FolderItem _parentFolderItem = null;
		
		public GdbItem(FolderItem parentFolderItem, string theFeactureClass)
		{
			_parentFolderItem = parentFolderItem;
			FeatureClassName = theFeactureClass;
		}
		
		public override bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				SetProperty(ref _isSelected, value, () => IsSelected);
				if (_isSelected)
				{
					//MessageBox.Show($@"Open layer: {_parentFolderItem.Folder} / {FeatureClassName}");
					AddToMap();
				}
			}
		}

		private string _featureClassName = null;

		public string FeatureClassName
		{
			get { return _featureClassName; }
			set
			{
				SetProperty(ref _featureClassName, value, () => FeatureClassName);
				base.Name = Path.GetFileName(_featureClassName);
			}
		}

		private Task AddToMap()
		{
			if (MapView.Active?.Map == null) return null;
			return QueuedTask.Run(() =>
			{
				try
				{
					// first we create an 'Item' using itemfactory
					string uri = $@"{_parentFolderItem.Folder}\{FeatureClassName}";
					Item currentItem = ItemFactory.Instance.Create(uri);
					// Finally add the feature service to the map
					// if we have an item that can be turned into a layer
					// add it to the map
					if (LayerFactory.Instance.CanCreateLayerFrom(currentItem))
						LayerFactory.Instance.CreateLayer(currentItem, MapView.Active.Map);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message, $@"Error trying to add layer: {_parentFolderItem.Folder} / {FeatureClassName}");
				}
			});
		}
	}
}
