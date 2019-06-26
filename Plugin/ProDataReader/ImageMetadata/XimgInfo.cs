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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMetadata
{
    public class XimgInfo
    {
        private XimgParse _ximgParse = null;
        //private Image _image = null;

		/// <summary>
		/// Used to parse the metadata from an image stream
		/// see: https://www.exiv2.org/tags.html
		/// </summary>
		/// <param name="fileName">the path to an image</param>
		public XimgInfo(string fileName)
		{
			try
			{
				Path = fileName;
				var image = System.Drawing.Image.FromFile(fileName);
				_ximgParse = new XimgParse(image);
				IsImage = true;
			}
			catch (Exception ex)
			{
				IsImage = false;
				Error = ex.ToString();
			}
		}

		public string Path { get; set; }

		public string Name
		{
			get
			{
				return !String.IsNullOrEmpty(Path) ? System.IO.Path.GetFileName(Path) : "n/a";
			}
		}

		public bool IsFolder
		{
			get
			{
				return Directory.Exists(Path);
			}
		}

		public bool IsImage { get; set; }

		public bool IsGpsEnabled {
			get {
				var sLat = Latitude.ToString("0.000");
				var sLng = Longitude.ToString("0.000");
				return !(double.IsNaN(Latitude) || double.IsNaN(Longitude))
						&& !(sLat.Equals ("0.000") || sLng.Equals ("0.000"));
			}
		}

		/// <summary>
		/// Always returns 1 = point geometry
		/// </summary>
		public int GeometryType => 1;

		//public byte[] Image
		//{
		//	get
		//	{
		//		byte[] imgData = System.IO.File.ReadAllBytes(Path);
		//		return imgData;
		//	}
		//}

        public string Error { get; set; }

        public double Longitude => _ximgParse.Longitude;

        public double Latitude => _ximgParse.Latitude;

        public double Altitude => _ximgParse.Altitude;

        public double ImageBearing => _ximgParse.ImageBearing;

        public DateTime DateTimeOriginal => _ximgParse.DateTimeOriginal;
    }
}
