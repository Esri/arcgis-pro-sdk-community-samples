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
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;

namespace AddInInfoManager
{
	/// <summary>
	/// Encapsulated AddIn metadata
	/// </summary>
	public class AddIn
	{
		public string Name { get; set; }
		public string Version { get; set; }
		public string DesktopVersion { get; set; }
		public string AddInPath { get; set; }
		public string Id { get; set; }
		public string Image { get; set; }
		public BitmapImage Thumbnail { get; set; }
		public string AddInDate { get; set; }

		/// <summary>
		/// Get the current add-in module's daml / AddInInfo Id tag (which is the same as the Assembly GUID)
		/// </summary>
		/// <returns></returns>
		public static string GetAddInId()
		{
			// Module.Id is internal, but we can still get the ID from the assembly
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			var attribute = (GuidAttribute)assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
			var fileName = Path.Combine($@"{{{attribute.Value.ToString()}}}", $@"{assembly.FullName.Split(',')[0]}.esriAddInX");
			return fileName;
		}

		/// <summary>
		/// returns a tuple with version and desktopVersion using the given addin file path
		/// </summary>
		/// <param name="fileName">file path (partial) of esriAddinX package</param>
		/// <returns>tuple: version, desktopVersion</returns>
		public static AddIn GetConfigDamlAddInInfo(string fileName)
		{
			var esriAddInX = new AddIn();
			XmlDocument xDoc = new XmlDocument();
			var esriAddInXPath = FindEsriAddInXPath(fileName);
			try
			{
				esriAddInX.AddInPath = esriAddInXPath;
				using (ZipArchive zip = ZipFile.OpenRead(esriAddInXPath))
				{
					ZipArchiveEntry zipEntry = zip.GetEntry("Config.daml");
					MemoryStream ms = new MemoryStream();
					string daml = string.Empty;
					using (Stream stmZip = zipEntry.Open())
					{
						StreamReader streamReader = new StreamReader(stmZip);
						daml = streamReader.ReadToEnd();
						xDoc.LoadXml(daml); // @"<?xml version=""1.0"" encoding=""utf - 8""?>" + 
					}
				}
				XmlNodeList items = xDoc.GetElementsByTagName("AddInInfo");
				foreach (XmlNode xItem in items)
				{
					esriAddInX.Version = xItem.Attributes["version"].Value;
					esriAddInX.DesktopVersion = xItem.Attributes["desktopVersion"].Value;
					esriAddInX.Id = xItem.Attributes["id"].Value;
					esriAddInX.Name = "N/A";
					esriAddInX.AddInDate = "N/A";
					foreach (XmlNode xChild in xItem.ChildNodes)
					{
						switch (xChild.Name)
						{
							case "Name":
								esriAddInX.Name = xChild.InnerText;
								break;
							case "Image":
								esriAddInX.Image = xChild.InnerText;
								break;
							case "Date":
								esriAddInX.AddInDate = xChild.InnerText;
								break;
						}
					}
				}
				if (esriAddInX.Image != null) {
					using (ZipArchive zip = ZipFile.OpenRead(esriAddInXPath))
					{
						esriAddInX.Thumbnail = new BitmapImage();
						ZipArchiveEntry zipEntry = zip.GetEntryOrdinalIgnoreCase(esriAddInX.Image);
						if (zipEntry != null)
						{
							MemoryStream ms = new MemoryStream();
							using (Stream stmZip = zipEntry.Open())
							{
								stmZip.CopyTo(ms);
							}
							esriAddInX.Thumbnail.BeginInit();
							esriAddInX.Thumbnail.StreamSource = ms;
							esriAddInX.Thumbnail.EndInit();

							var encoder = new PngBitmapEncoder();
							encoder.Frames.Add(BitmapFrame.Create(esriAddInX.Thumbnail));
							using (var filestream = new FileStream(@"c:\temp\test.png", FileMode.Create))
							{
								encoder.Save(filestream);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception($@"Unable to parse config.daml {esriAddInXPath}: {ex.Message}");
			}
			return esriAddInX;
		}

		private static readonly string AddInSubFolderPath = @"ArcGIS\AddIns\ArcGISPro";
		private static string FindEsriAddInXPath(string fileName)
		{
			string defaultAddInPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AddInSubFolderPath);
			string thePath = Path.Combine(defaultAddInPath, fileName);
			if (File.Exists(thePath)) return thePath;
			foreach (var addinPath in GetAddInFolders())
			{
				thePath = Path.Combine(addinPath, fileName);
				if (File.Exists(thePath)) return thePath;
			}
			throw new FileNotFoundException($@"esriAddInX file for {fileName} was not found");
		}

		/// <summary>
		/// Returns the list of all Addins
		/// </summary>
		/// <returns></returns>
		public static List<AddIn> GetAddIns()
		{
			List<AddIn> addIns = new List<AddIn>();
			List<string> lstPaths = GetAddInFolders();
			string defaultAddInPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AddInSubFolderPath);
			lstPaths.Insert(0, defaultAddInPath);
			foreach (var addinPath in lstPaths)
			{
				foreach (var addinDirs in Directory.GetDirectories (addinPath))
				{
					foreach (var addinFile in Directory.GetFiles (addinDirs, "*.esriAddinX"))
					{
						addIns.Add (GetConfigDamlAddInInfo(addinFile));
					}
				}
			}
			return addIns;
		}

		/// <summary>
		/// Gets the well-known Add-in folders on the machine
		/// </summary>
		/// <returns>List of all well-known add-in folders</returns>
		public static List<string> GetAddInFolders()
		{
			List<string> myAddInPathKeys = new List<string>();
			string regPath = string.Format(@"Software\ESRI\ArcGISPro\Settings\Add-In Folders");
			//string path = "";
			string err1 = "This is an error";
			try
			{
				Microsoft.Win32.RegistryKey localKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Microsoft.Win32.RegistryView.Registry64);
				Microsoft.Win32.RegistryKey esriKey = localKey.OpenSubKey(regPath);
				if (esriKey == null)
				{
					localKey = Microsoft.Win32.RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.CurrentUser, Microsoft.Win32.RegistryView.Registry64);
					esriKey = localKey.OpenSubKey(regPath);
				}
				if (esriKey != null)
					myAddInPathKeys.AddRange(esriKey.GetValueNames().Select(key => key.ToString()));
			}
			catch (InvalidOperationException ie)
			{
				//this is ours
				throw ie;
			}
			catch (Exception ex)
			{
				throw new System.Exception(err1, ex);
			}
			return myAddInPathKeys;
		}

	}
}
