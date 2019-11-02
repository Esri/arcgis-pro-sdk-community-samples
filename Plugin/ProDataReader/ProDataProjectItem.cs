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
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ESRI.ArcGIS.ItemIndex;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor;
using ImageMetadata;
using System.Runtime.InteropServices;

namespace ProDataReader
{

	internal class ProDataProjectItem : CustomProjectItemBase
	{
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		[DllImport("kernel32.dll")]

		internal static extern uint GetCurrentThreadId();

		internal static List<string> FileExtensions = new List<string> { ".mdb", ".gpx", ".jpg" };

		protected ProDataProjectItem() : base()
		{
		}

		protected ProDataProjectItem(ItemInfoValue iiv) : base(FlipBrowseDialogOnly(iiv))
		{

		}

		private static ItemInfoValue FlipBrowseDialogOnly(ItemInfoValue iiv)
		{
			iiv.browseDialogOnly = "FALSE";
			return iiv;
		}

		//TODO: Overload for use in your container create item
		public ProDataProjectItem(string name, string catalogPath, string typeID, string containerTypeID) :
		  base(name, catalogPath, typeID, containerTypeID)
		{

		}

		public ProDataProjectItem Clone() => new ProDataProjectItem(this.Name, this.Path, this.TypeID, this.ContainerType);

		/// <summary>
		/// DTor
		/// </summary>
		~ProDataProjectItem()
		{
		}

		public override ImageSource LargeImage
		{
			get
			{
				var largeImg = new BitmapImage(new Uri(@"pack://application:,,,/ProDataReader;component/Images/ZipDetail32.png")) as ImageSource;
				return largeImg;
			}
		}

		public override Task<ImageSource> SmallImage
		{
			get
			{
				var smallImage = new BitmapImage(new Uri(@"pack://application:,,,/ProDataReader;component/Images/ZipDetail16.png")) as ImageSource;
				if (smallImage == null) throw new ArgumentException("SmallImage for CustomProjectItem doesn't exist");
				return Task.FromResult(smallImage as ImageSource);
			}
		}
		public override ProjectItemInfo OnGetInfo()
		{
			var projectItemInfo = new ProjectItemInfo
			{
				Name = this.Name,
				Path = this.Path,
				Type = ProDataProjectItemContainer.ContainerName
			};

			return projectItemInfo;
		}

		public override bool IsContainer => true;

		

		//TODO: Fetch is required if <b>IsContainer</b> = <b>true</b>
		public override void Fetch()
		{
			// Retrieve your child items
			// child items must also derive from CustomItemBase
			// the CustomDataLink file contains line items with link to other folder where our customdata
			// is stored
			var alreadyProcessedPath = new List<string>();
			var children = new List<ProDataSubItem>();
			var dataFolders = System.IO.File.ReadAllLines(this.Path);
			for (var idx = 0; idx < dataFolders.Length; idx++)
			{
				dataFolders[idx] = dataFolders[idx].TrimEnd(new char [] { '\\', '/' });
			}
			foreach (var ext in FileExtensions)
			{
				foreach (var dataFolder in dataFolders)
				{
					if (string.IsNullOrEmpty(dataFolder)) continue;
					var files = System.IO.Directory.GetFiles(dataFolder, $@"*{ext}", System.IO.SearchOption.AllDirectories);
					foreach (var fullName in files)
					{
						var fileName = System.IO.Path.GetFileNameWithoutExtension(fullName);
						var fileExt = System.IO.Path.GetExtension(fullName).ToLower();
						switch (fileExt)
						{
							case ".gpx":
								{
									var uniquePath = fullName;
									var child = new ProDataSubItem(fileName, uniquePath, this.TypeID,
										null, ProDataSubItem.EnumSubItemType.GpxType);
									children.Add(GetParentFolder(dataFolder, child));
								}
								break;
							case ".jpg":
								{
									var ximgInfo = new XimgInfo(fullName);
									System.Diagnostics.Debug.WriteLine($@"Image {ximgInfo.IsImage}");
									if (!ximgInfo.IsImage || !ximgInfo.IsGpsEnabled) continue;

									//// Get the PropertyItems property from image.
									// the path has to be 'unique' for each entry otherwise the UI
									// will not treat the enumeration as a real enumeration
									// However in this case we only have one single item

									var parentFolder = System.IO.Path.GetDirectoryName(fullName);
									if (dataFolder.Equals(parentFolder))
									{
										System.Diagnostics.Debug.WriteLine($@"Lat: {ximgInfo.Latitude} Lon: {ximgInfo.Longitude} Alt: {ximgInfo.Altitude}");
										var uniquePath = fullName;
										var child = new ProDataSubItem(fileName, uniquePath, this.TypeID, null, ProDataSubItem.EnumSubItemType.ImgType);
										children.Add(GetParentFolder(dataFolder, child));
									}
									else
									{
										// directory full of photos
										if (alreadyProcessedPath.Contains(parentFolder))
										{
											// already processed this parent folder

										}
										else
										{
											alreadyProcessedPath.Add(parentFolder);
											var jpgChildren = new List<ProDataSubItem>();
											var jpgFiles = System.IO.Directory.GetFiles(parentFolder, $@"*{ext}", System.IO.SearchOption.TopDirectoryOnly);
											foreach (var jpgFullName in jpgFiles)
											{
												var xInfo = new XimgInfo(jpgFullName);
												System.Diagnostics.Debug.WriteLine($@"Image {xInfo.IsImage}");
												if (!xInfo.IsImage || !xInfo.IsGpsEnabled) continue;
												jpgChildren.Add(new ProDataSubItem(jpgFullName, jpgFullName, this.TypeID, null, ProDataSubItem.EnumSubItemType.ImgType));
											}
											var uniquePath = System.IO.Path.Combine (parentFolder, $@"{System.IO.Path.GetFileName(parentFolder)} Jpeg Images");
											var name = System.IO.Path.GetFileName(uniquePath);
											var rootNode = new ProDataSubItem(name, uniquePath, this.TypeID, null, ProDataSubItem.EnumSubItemType.ImgDirType, jpgChildren);
											children.Add(GetParentFolder(dataFolder, rootNode));
										}
									}
									//var img = System.Drawing.Image.FromStream (fs);
									//var propItems = img.PropertyItems;
									//foreach (var propItem in propItems)
									//{
									//    System.Diagnostics.Debug.WriteLine($@"iD: 0x{propItem.Id.ToString("x")}");
									//}
									//// see: https://www.exiv2.org/tags.html
									//System.Diagnostics.Debug.WriteLine($@"Image Aspect = {img.Width} x {img.Height}");
									//System.Diagnostics.Debug.WriteLine($@"Make = {ReadImageProperty(img, 0x110)}");
									//System.Diagnostics.Debug.WriteLine($@"Date = {ReadImageProperty(img, 0x0132)}");

									//var gps = ImageMetadataReader.ReadMetadata(fs).OfType<GpsDirectory>().FirstOrDefault();
									//var location = gps.GetGeoLocation();
									//System.Diagnostics.Debug.WriteLine($@"Location: {location.Longitude} {location.Latitude}");
								}
								break;
						}
					}
				}
			}
			this.AddRangeToChildren(children);
		}

		/// <summary>
		/// subItem has a path that needs to be parsed (from the back) in order to build the 
		/// directory tree.  Only the top node of the directory tree (at topNodeName) will be
		/// returned and then added to the root.
		/// </summary>
		/// <param name="topNodeName">top directory where all data is located</param>
		/// <param name="subItem">Item to be inserted at the end of the branch</param>
		/// <returns>top node that needs to be added to the root 'data folder'</returns>
		private ProDataSubItem GetParentFolder(string topNodeName, ProDataSubItem subItem)
		{
			var parts = topNodeName.Split(new char [] { '/','\\'});
			var rootPartsCnt = parts.Length;
			parts = subItem.Path.Split(new char[] { '/', '\\' });
			var topNode = subItem;
			for (int idx = parts.Length - 2; idx >= rootPartsCnt; idx--)
			{
				var completeFolderPath = string.Empty;
				for (int iidx = 0; iidx <= idx; iidx++)
				{
					if (iidx > 0) completeFolderPath += @"\";
					completeFolderPath += parts[iidx];
				}
				var uniquePath = System.IO.Path.Combine(Path, completeFolderPath);
				topNode = new ProDataSubItem(parts[idx], uniquePath, this.TypeID,
					null, ProDataSubItem.EnumSubItemType.DirType, new List<ProDataSubItem> { topNode });
			}
			return topNode;
		}

		private static readonly ASCIIEncoding ASCIIencoding = new ASCIIEncoding();

		private static string ReadImageProperty(System.Drawing.Image image, int ID)
		{
			try
			{
				var pi = image.GetPropertyItem(ID);
				if (pi.Type == 2) //ASCII
				{
					return ASCIIencoding.GetString(pi.Value, 0, pi.Len - 1);
				}
				if (pi.Type == 5) //rational
				{
					byte[] bb = pi.Value;
					uint uNominator = BitConverter.ToUInt32(bb, 0);
					uint uDenominator = BitConverter.ToUInt32(bb, 4);
					if (uDenominator == 1) return uNominator.ToString();
					return uNominator.ToString() + "/" + uDenominator.ToString();
				}
			}
			catch
			{
			}
			return "Not Found";
		}
	}


	internal class ProDataSubItem : CustomItemBase
	{
		public enum EnumSubItemType
		{
			DirType = 0,
			GpxType = 1,
			ImgType = 2,
			GpxDirType = 4,
			ImgDirType = 5
		}

		public ProDataSubItem(string name, string catalogPath,
							 string typeID, object specialization,
							 EnumSubItemType zipSubItemType,
							 List<ProDataSubItem> children = null) :
			base(System.IO.Path.GetFileNameWithoutExtension(name), catalogPath, typeID, DateTime.Now.ToString())
		{
			ComboPath = name;
			SubItemType = zipSubItemType;
			this.DisplayType = "Misc GIS Feature Class";
			this.ContextMenuID = "ProDataSubItem_ContextMenu";
			if (children != null)
			{
				this.AddRangeToChildren(children);
			}
			Specialization = specialization;
		}

		public override bool IsContainer => GetChildren().Count() > 0;

		public string ComboPath { get; set; }

		public object Specialization { get; set; }

		public EnumSubItemType SubItemType { get; set; }

		public override ImageSource LargeImage
		{
			get
			{
				var imgSrc = GetIconImage(false);
				return imgSrc;
			}
		}

		public override Task<ImageSource> SmallImage
		{
			get
			{
				var imgSrc = GetIconImage(true);
				return Task.FromResult(imgSrc);
			}
		}

		private ImageSource GetIconImage(bool bSmall)
		{
			var size = bSmall ? "16" : "32";
			var imgSrc = System.Windows.Application.Current.Resources[$@"T-Rex{size}"] as ImageSource;
			switch (SubItemType)
			{
				case EnumSubItemType.DirType:
					imgSrc = System.Windows.Application.Current.Resources[$@"Folder{size}"] as ImageSource;
					break;
				case EnumSubItemType.GpxType:
					imgSrc = System.Windows.Application.Current.Resources[$@"GeodatabaseFeatureClassLine{size}"] as ImageSource;
					break;
				case EnumSubItemType.GpxDirType:
					break;
				case EnumSubItemType.ImgType:
					imgSrc = System.Windows.Application.Current.Resources[$@"Image{size}"] as ImageSource;
					break;
				case EnumSubItemType.ImgDirType:
					{
						imgSrc = System.Windows.Application.Current.Resources[$@"GeodatabaseMosaicDataset{size}"] as ImageSource;
						var fcType = 0;
						var proGpxInfo = Specialization as XimgInfo;
						if (proGpxInfo != null) fcType = proGpxInfo.GeometryType;
						switch (fcType)
						{
							case 1:
								imgSrc = System.Windows.Application.Current.Resources[$@"GeodatabaseFeatureClassPoint{size}"] as ImageSource;
								break;
						}
					}
					break;
				default:
					imgSrc = System.Windows.Application.Current.Resources[$@"T-Rex{size}"] as ImageSource;
					break;
			}
			if (imgSrc == null) throw new ArgumentException($@"Unable to find small image for ProMdbTable");
			return imgSrc;
		}
	}
}
