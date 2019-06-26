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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMetadata
{
    public class XimgParse
    {
        private Dictionary<int, XimgTag> _parseTags;

        public XimgParse(System.Drawing.Image image)
        {
            Encoding ascii = Encoding.ASCII;
            _parseTags = new Dictionary<int, XimgTag>();
            foreach (System.Drawing.Imaging.PropertyItem pitem in image.PropertyItems)
            {
                if (!ParseTags.ContainsKey(pitem.Id)) continue;
                XimgTag xTag = ParseTags[pitem.Id].Clone();
                string strValue = "";
                object value = null;
                switch (pitem.Type)
                {
                    case 0x1:
                        {
                            #region BYTE (8-bit unsigned int)
                            value = pitem.Value[0];
                            if (pitem.Value.Length == 4)
                                strValue = "Version " + pitem.Value[0].ToString() + "." + pitem.Value[1].ToString();
                            else if (pitem.Id == 0x5 && pitem.Value[0] == 0)
                                strValue = "Sea level";
                            else
                                strValue = pitem.Value[0].ToString();
                            #endregion
                        }
                        break;
                    case 0x2:
                        {
                            #region ASCII (8 bit ASCII code)

                            strValue = ascii.GetString(pitem.Value).Trim('\0');
                            value = strValue;
                            if (pitem.Id == 0x1 || pitem.Id == 0x13)
                                if (strValue == "N") strValue = "North latitude";
                                else if (strValue == "S") strValue = "South latitude";
                                else strValue = "n/a";

                            if (pitem.Id == 0x3 || pitem.Id == 0x15)
                                if (strValue == "E") strValue = "East longitude";
                                else if (strValue == "W") strValue = "West longitude";
                                else strValue = "n/a";

                            if (pitem.Id == 0x9)
                                if (strValue == "A") strValue = "Measurement in progress";
                                else if (strValue == "V") strValue = "Measurement Interoperability";
                                else strValue = "n/a";

                            if (pitem.Id == 0xA)
                                if (strValue == "2") strValue = "2-dimensional measurement";
                                else if (strValue == "3") strValue = "3-dimensional measurement";
                                else strValue = "n/a";

                            if (pitem.Id == 0xC || pitem.Id == 0x19)
                                if (strValue == "K") strValue = "Kilometers per hour";
                                else if (strValue == "M") strValue = "Miles per hour";
                                else if (strValue == "N") strValue = "Knots";
                                else strValue = "n/a";

                            if (pitem.Id == 0xE || pitem.Id == 0x10 || pitem.Id == 0x17)
                                if (strValue == "T") strValue = "True direction";
                                else if (strValue == "M") strValue = "Magnetic direction";
                                else strValue = "n/a";
                            #endregion
                        }
                        break;
                    case 0x3:
                        {
                            #region 3 = SHORT (16-bit unsigned int)
                            UInt16 uintval = BitConverter.ToUInt16(pitem.Value, 0);
                            value = uintval;

                            // orientation // lookup table					
                            switch (pitem.Id)
                            {
                                case 0x8827: // ISO speed rating
                                    strValue = "ISO-" + uintval.ToString();
                                    break;
                                case 0xA217: // sensing method
                                    {
                                        switch (uintval)
                                        {
                                            case 1: strValue = "Not defined"; break;
                                            case 2: strValue = "One-chip color area sensor"; break;
                                            case 3: strValue = "Two-chip color area sensor"; break;
                                            case 4: strValue = "Three-chip color area sensor"; break;
                                            case 5: strValue = "Color sequential area sensor"; break;
                                            case 7: strValue = "Trilinear sensor"; break;
                                            case 8: strValue = "Color sequential linear sensor"; break;
                                            default: strValue = " reserved"; break;
                                        }
                                    }
                                    break;
                                case 0x8822: // Exposure program
                                    switch (uintval)
                                    {
                                        case 0: strValue = "Not defined"; break;
                                        case 1: strValue = "Manual"; break;
                                        case 2: strValue = "Normal program"; break;
                                        case 3: strValue = "Aperture priority"; break;
                                        case 4: strValue = "Shutter priority"; break;
                                        case 5: strValue = "Creative program (biased toward depth of field)"; break;
                                        case 6: strValue = "Action program (biased toward fast shutter speed)"; break;
                                        case 7: strValue = "Portrait mode (for closeup photos with the background out of focus)"; break;
                                        case 8: strValue = "Landscape mode (for landscape photos with the background in focus)"; break;
                                        default: strValue = "n/a"; break;
                                    }
                                    break;
                                case 0x9207: // metering mode
                                    switch (uintval)
                                    {
                                        case 0: strValue = "unknown"; break;
                                        case 1: strValue = "Average"; break;
                                        case 2: strValue = "Center Weighted Average"; break;
                                        case 3: strValue = "Spot"; break;
                                        case 4: strValue = "MultiSpot"; break;
                                        case 5: strValue = "Pattern"; break;
                                        case 6: strValue = "Partial"; break;
                                        case 255: strValue = "Other"; break;
                                        default: strValue = "n/a"; break;
                                    }
                                    break;
                                case 0x9208: // Light source
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "unknown"; break;
                                            case 1: strValue = "Daylight"; break;
                                            case 2: strValue = "Fluorescent"; break;
                                            case 3: strValue = "Tungsten (incandescent light)"; break;
                                            case 4: strValue = "Flash"; break;
                                            case 9: strValue = "Fine weather"; break;
                                            case 10: strValue = "Cloudy weather"; break;
                                            case 11: strValue = "Shade"; break;
                                            case 12: strValue = "Daylight fluorescent (D 5700 – 7100K)"; break;
                                            case 13: strValue = "Day white fluorescent (N 4600 – 5400K)"; break;
                                            case 14: strValue = "Cool white fluorescent (W 3900 – 4500K)"; break;
                                            case 15: strValue = "White fluorescent (WW 3200 – 3700K)"; break;
                                            case 17: strValue = "Standard light A"; break;
                                            case 18: strValue = "Standard light B"; break;
                                            case 19: strValue = "Standard light C"; break;
                                            case 20: strValue = "D55"; break;
                                            case 21: strValue = "D65"; break;
                                            case 22: strValue = "D75"; break;
                                            case 23: strValue = "D50"; break;
                                            case 24: strValue = "ISO studio tungsten"; break;
                                            case 255: strValue = "ISO studio tungsten"; break;
                                            default: strValue = "other light source"; break;
                                        }
                                    }
                                    break;
                                case 0x9209: // Flash
                                    {
                                        switch (uintval)
                                        {
                                            case 0x0: strValue = "Flash did not fire"; break;
                                            case 0x1: strValue = "Flash fired"; break;
                                            case 0x5: strValue = "Strobe return light not detected"; break;
                                            case 0x7: strValue = "Strobe return light detected"; break;
                                            case 0x9: strValue = "Flash fired, compulsory flash mode"; break;
                                            case 0xD: strValue = "Flash fired, compulsory flash mode, return light not detected"; break;
                                            case 0xF: strValue = "Flash fired, compulsory flash mode, return light detected"; break;
                                            case 0x10: strValue = "Flash did not fire, compulsory flash mode"; break;
                                            case 0x18: strValue = "Flash did not fire, auto mode"; break;
                                            case 0x19: strValue = "Flash fired, auto mode"; break;
                                            case 0x1D: strValue = "Flash fired, auto mode, return light not detected"; break;
                                            case 0x1F: strValue = "Flash fired, auto mode, return light detected"; break;
                                            case 0x20: strValue = "No flash function"; break;
                                            case 0x41: strValue = "Flash fired, red-eye reduction mode"; break;
                                            case 0x45: strValue = "Flash fired, red-eye reduction mode, return light not detected"; break;
                                            case 0x47: strValue = "Flash fired, red-eye reduction mode, return light detected"; break;
                                            case 0x49: strValue = "Flash fired, compulsory flash mode, red-eye reduction mode"; break;
                                            case 0x4D: strValue = "Flash fired, compulsory flash mode, red-eye reduction mode, return light not detected"; break;
                                            case 0x4F: strValue = "Flash fired, compulsory flash mode, red-eye reduction mode, return light detected"; break;
                                            case 0x59: strValue = "Flash fired, auto mode, red-eye reduction mode"; break;
                                            case 0x5D: strValue = "Flash fired, auto mode, return light not detected, red-eye reduction mode"; break;
                                            case 0x5F: strValue = "Flash fired, auto mode, return light detected, red-eye reduction mode"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0x0128: //ResolutionUnit
                                    {
                                        switch (uintval)
                                        {
                                            case 2: strValue = "Inch"; break;
                                            case 3: strValue = "Centimeter"; break;
                                            default: strValue = "No Unit"; break;
                                        }
                                    }
                                    break;
                                case 0xA409: // Saturation
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "Normal"; break;
                                            case 1: strValue = "Low saturation"; break;
                                            case 2: strValue = "High saturation"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;

                                case 0xA40A: // Sharpness
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "Normal"; break;
                                            case 1: strValue = "Soft"; break;
                                            case 2: strValue = "Hard"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0xA408: // Contrast
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "Normal"; break;
                                            case 1: strValue = "Soft"; break;
                                            case 2: strValue = "Hard"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0x103: // Compression
                                    {
                                        switch (uintval)
                                        {
                                            case 1: strValue = "Uncompressed"; break;
                                            case 6: strValue = "JPEG compression (thumbnails only)"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0x106: // PhotometricInterpretation
                                    {
                                        switch (uintval)
                                        {
                                            case 2: strValue = "RGB"; break;
                                            case 6: strValue = "YCbCr"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0x112: // Orientation
                                    {
                                        switch (uintval)
                                        {
                                            case 1: strValue = "The 0th row is at the visual top of the image, and the 0th column is the visual left-hand side."; break;
                                            case 2: strValue = "The 0th row is at the visual top of the image, and the 0th column is the visual right-hand side."; break;
                                            case 3: strValue = "The 0th row is at the visual bottom of the image, and the 0th column is the visual right-hand side."; break;
                                            case 4: strValue = "The 0th row is at the visual bottom of the image, and the 0th column is the visual left-hand side."; break;
                                            case 5: strValue = "The 0th row is the visual left-hand side of the image, and the 0th column is the visual top."; break;
                                            case 6: strValue = "The 0th row is the visual right-hand side of the image, and the 0th column is the visual top."; break;
                                            case 7: strValue = "The 0th row is the visual right-hand side of the image, and the 0th column is the visual bottom."; break;
                                            case 8: strValue = "The 0th row is the visual left-hand side of the image, and the 0th column is the visual bottom."; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0x213: // YCbCrPositioning
                                    {
                                        switch (uintval)
                                        {
                                            case 1: strValue = "centered"; break;
                                            case 6: strValue = "co-sited"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0xA001: // ColorSpace
                                    {
                                        switch (uintval)
                                        {
                                            case 1: strValue = "sRGB"; break;
                                            case 0xFFFF: strValue = "Uncalibrated"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0xA401: // CustomRendered
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "Normal process"; break;
                                            case 1: strValue = "Custom process"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0xA402: // ExposureMode
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "Auto exposure"; break;
                                            case 1: strValue = "Manual exposure"; break;
                                            case 2: strValue = "Auto bracket"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0xA403: // WhiteBalance
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "Auto white balance"; break;
                                            case 1: strValue = "Manual white balance"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0xA406: // SceneCaptureType
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "Standard"; break;
                                            case 1: strValue = "Landscape"; break;
                                            case 2: strValue = "Portrait"; break;
                                            case 3: strValue = "Night scene"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;

                                case 0xA40C: // SubjectDistanceRange
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "unknown"; break;
                                            case 1: strValue = "Macro"; break;
                                            case 2: strValue = "Close view"; break;
                                            case 3: strValue = "Distant view"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0x1E: // GPSDifferential
                                    {
                                        switch (uintval)
                                        {
                                            case 0: strValue = "Measurement without differential correction"; break;
                                            case 1: strValue = "Differential correction applied"; break;
                                            default: strValue = "n/a"; break;
                                        }
                                    }
                                    break;
                                case 0xA405: // FocalLengthIn35mmFilm
                                    strValue = uintval.ToString() + " mm";
                                    break;
                                default://
                                    strValue = uintval.ToString();
                                    break;
                            }
                            #endregion
                        }
                        break;
                    case 0x4:
                        {
                            #region 4 = LONG (32-bit unsigned int)
                            value = BitConverter.ToUInt32(pitem.Value, 0);
                            strValue = value.ToString();
                            #endregion
                        }
                        break;
                    case 0x5:
                        {
                            #region 5 = RATIONAL (Two LONGs, unsigned)

                            XimgURational rat = new XimgURational(pitem.Value);
                            value = rat;
                            switch (pitem.Id)
                            {
                                case 0x9202: // ApertureValue
                                    strValue = "F/" + Math.Round(Math.Pow(Math.Sqrt(2), rat.ToDouble()), 2).ToString();
                                    break;
                                case 0x9205: // MaxApertureValue
                                    strValue = "F/" + Math.Round(Math.Pow(Math.Sqrt(2), rat.ToDouble()), 2).ToString();
                                    break;
                                case 0x920A: // FocalLength
                                    strValue = rat.ToDouble().ToString() + " mm";
                                    break;
                                case 0x829D: // F-number
                                    strValue = "F/" + rat.ToDouble().ToString();
                                    break;
                                case 0x11A: // Xresolution
                                    strValue = rat.ToDouble().ToString();
                                    break;
                                case 0x11B: // Yresolution
                                    strValue = rat.ToDouble().ToString();
                                    break;
                                case 0x829A: // ExposureTime
                                    strValue = rat.ToString() + " sec";
                                    break;
                                case 0x2: // GPSLatitude      
                                    value = new XimgGPSRational(pitem.Value);
                                    strValue = value.ToString();
                                    break;
                                case 0x4: // GPSLongitude
                                    value = new XimgGPSRational(pitem.Value);
                                    strValue = value.ToString();
                                    break;
                                case 0x6: // GPSAltitude
                                    strValue = rat.ToDouble() + " meters";
                                    break;
                                case 0xA404: // Digital Zoom Ratio
                                    strValue = rat.ToDouble().ToString();
                                    if (strValue == "0") strValue = "none";
                                    break;
                                case 0xB: // GPSDOP
                                    strValue = rat.ToDouble().ToString();
                                    break;
                                case 0xD: // GPSSpeed
                                    strValue = rat.ToDouble().ToString();
                                    break;
                                case 0xF: // GPSTrack
                                    strValue = rat.ToDouble().ToString();
                                    break;
                                case 0x11: // GPSImgDir
                                    strValue = rat.ToDouble().ToString();
                                    break;
                                case 0x14: // GPSDestLatitude
                                    value = new XimgGPSRational(pitem.Value);
                                    strValue = value.ToString();
                                    break;
                                case 0x16: // GPSDestLongitude
                                    value = new XimgGPSRational(pitem.Value);
                                    strValue = value.ToString();
                                    break;
                                case 0x18: // GPSDestBearing
                                    strValue = rat.ToDouble().ToString();
                                    break;
                                case 0x1A: // GPSDestDistance
                                    strValue = rat.ToDouble().ToString();
                                    break;
                                case 0x7: // GPSTimeStamp                                
                                    value = new XimgGPSRational(pitem.Value);
                                    strValue = (value as XimgGPSRational).ToString(":");
                                    break;

                                default:
                                    strValue = rat.ToString();
                                    break;
                            }

                            #endregion
                        }
                        break;
                    case 0x7:
                        {
                            #region UNDEFINED (8-bit)
                            value = pitem.Value[0];
                            switch (pitem.Id)
                            {
                                case 0xA300: //FileSource
                                    {
                                        if (pitem.Value[0] == 3)
                                            strValue = "DSC";
                                        else
                                            strValue = "n/a";
                                        break;
                                    }
                                case 0xA301: //SceneType
                                    if (pitem.Value[0] == 1)
                                        strValue = "A directly photographed image";
                                    else
                                        strValue = "n/a";
                                    break;
                                case 0x9000:// Exif Version
                                    strValue = ascii.GetString(pitem.Value).Trim('\0');
                                    break;
                                case 0xA000: // Flashpix Version
                                    strValue = ascii.GetString(pitem.Value).Trim('\0');
                                    if (strValue == "0100")
                                        strValue = "Flashpix Format Version 1.0";
                                    else strValue = "n/a";
                                    break;
                                case 0x9101: //ComponentsConfiguration
                                    strValue = GetComponentsConfig(pitem.Value);
                                    break;
                                case 0x927C: //MakerNote
                                    strValue = ascii.GetString(pitem.Value).Trim('\0');
                                    break;
                                case 0x9286: //UserComment
                                    strValue = ascii.GetString(pitem.Value).Trim('\0');
                                    break;
                                case 0x1B: //GPS Processing Method
                                    strValue = ascii.GetString(pitem.Value).Trim('\0');
                                    break;
                                case 0x1C: //GPS Area Info
                                    strValue = ascii.GetString(pitem.Value).Trim('\0');
                                    break;
                                default:
                                    strValue = "-";
                                    break;
                            }
                            #endregion
                        }
                        break;
                    case 0x9:
                        {
                            #region 9 = SLONG (32-bit int)
                            value = BitConverter.ToInt32(pitem.Value, 0);
                            strValue = value.ToString();
                            #endregion
                        }
                        break;
                    case 0xA:
                        {
                            #region 10 = SRATIONAL (Two SLONGs, signed)

                            XimgRational rat = new XimgRational(pitem.Value);
                            value = rat;
                            switch (pitem.Id)
                            {
                                case 0x9201: // ShutterSpeedValue
                                    strValue = "1/" + Math.Round(Math.Pow(2, rat.ToDouble()), 2).ToString();
                                    break;
                                case 0x9203: // BrightnessValue
                                    strValue = Math.Round(rat.ToDouble(), 4).ToString();
                                    break;
                                case 0x9204: // ExposureBiasValue
                                    strValue = Math.Round(rat.ToDouble(), 2).ToString() + " eV";
                                    break;
                                default:
                                    strValue = rat.ToString();
                                    break;
                            }
                            #endregion
                        }
                        break;
                }
                xTag.Value = value;
                xTag.StrValue = strValue;
                xTag.ItemType = pitem.Type;
                _parseTags.Add(xTag.Id, xTag);
            }
        }

        /// <summary>
        /// get data from 0x4, "GPSLongitude" and 0x3, "GPSLongitudeRef", "East or West Longitude"
        /// </summary>
        public double Longitude
        {
            get
            {
				if (_parseTags.ContainsKey(0x3) && _parseTags.ContainsKey(0x4))
				{
					var value = (_parseTags[0x4].Value as XimgGPSRational).Degrees;
					if (_parseTags[0x3].StrValue.ToLower().StartsWith ("w"))
					{
						value = -value;
					}
					return value;
				}
				return double.NaN;
            }
        }

        /// <summary>
        /// get value from 0x2, "GPSLatitude"  and 0x1, "GPSLatitudeRef", "North or South Latitude")
        /// </summary>
        public double Latitude
        {
            get
			{
				if (_parseTags.ContainsKey(0x1) && _parseTags.ContainsKey(0x2))
				{
					var value = (_parseTags[0x2].Value as XimgGPSRational).Degrees;
					if (_parseTags[0x1].StrValue.ToLower().StartsWith("s"))
					{
						value = -value;
					}
					return value;
				}
				return double.NaN;
			}
		}

		/// <summary>
		/// get value from 0x6, "GPSAltitude"
		/// </summary>
		public double Altitude
        {
            get
			{
				if (_parseTags.ContainsKey(0x6))
				{
					return (_parseTags[0x6].Value as XimgURational).ToDouble();
				}
				return double.NaN;
            }
        }

        /// <summary>
        /// get value from 0x11, "GPSImgDirection", "Direction of image"
        /// </summary>
        public double ImageBearing
        {
            get
            {
				if (_parseTags.ContainsKey(0x11))
				{
					return (_parseTags[0x11].Value as XimgURational).ToDouble();
				}
				return double.NaN;
            }
        }

        /// <summary>
        /// get value from 0x9003, "DateTimeOriginal"
        /// </summary>
        public DateTime DateTimeOriginal
		{
			get
			{
				var dateStr = string.Empty;
				DateTime dateValue = DateTime.Now;
				try
				{
					if (_parseTags.ContainsKey(0x132))
					{
						dateStr = _parseTags[0x132].Value.ToString();
					}
					dateValue = DateTime.Parse(dateStr);
					Console.WriteLine("'{0}' converted to {1}.", dateStr, dateValue);
				}
				catch (FormatException)
				{
					Console.WriteLine("Unable to convert '{0}'.", dateStr);
				}
				return dateValue;
			}
		}

		private static Dictionary<int, XimgTag> ParseTags
        {
            get
            {
                var parseTags = new Dictionary<int, XimgTag>
                {
                    { 0x100, new XimgTag(0x100, "ImageWidth", "Image width") },
                    { 0x101, new XimgTag(0x101, "ImageHeight", "Image height") },
                    { 0x0, new XimgTag(0x0, "GPSVersionID", "GPS tag version") },
                    { 0x5, new XimgTag(0x5, "GPSAltitudeRef", "Altitude reference") },
                    { 0x111, new XimgTag(0x111, "StripOffsets", "Image data location") },
                    { 0x116, new XimgTag(0x116, "RowsPerStrip", "Number of rows per strip") },
                    { 0x117, new XimgTag(0x117, "StripByteCounts", "Bytes per compressed strip") },
                    { 0xA002, new XimgTag(0xA002, "PixelXDimension", "Valid image width") },
                    { 0xA003, new XimgTag(0xA003, "PixelYDimension", "Valid image height") },
                    { 0x102, new XimgTag(0x102, "BitsPerSample", "Number of bits per component") },
                    { 0x103, new XimgTag(0x103, "Compression", "Compression scheme") },
                    { 0x106, new XimgTag(0x106, "PhotometricInterpretation", "Pixel composition") },
                    { 0x112, new XimgTag(0x112, "Orientation", "Orientation of image") },
                    { 0x115, new XimgTag(0x115, "SamplesPerPixel", "Number of components") },
                    { 0x11C, new XimgTag(0x11C, "PlanarConfiguration", "Image data arrangement") },
                    { 0x212, new XimgTag(0x212, "YCbCrSubSampling", "Subsampling ratio of Y to C") },
                    { 0x213, new XimgTag(0x213, "YCbCrPositioning", "Y and C positioning") },
                    { 0x128, new XimgTag(0x128, "ResolutionUnit", "Unit of X and Y resolution") },
                    { 0x12D, new XimgTag(0x12D, "TransferFunction", "Transfer function") },
                    { 0xA001, new XimgTag(0xA001, "ColorSpace", "Color space information") },
                    { 0x8822, new XimgTag(0x8822, "ExposureProgram", "Exposure program") },
                    { 0x8827, new XimgTag(0x8827, "ISOSpeedRatings", "ISO speed rating") },
                    { 0x9207, new XimgTag(0x9207, "MeteringMode", "Metering mode") },
                    { 0x9208, new XimgTag(0x9208, "LightSource", "Light source") },
                    { 0x9209, new XimgTag(0x9209, "Flash", "Flash") },
                    { 0x9214, new XimgTag(0x9214, "SubjectArea", "Subject area") },
                    { 0xA210, new XimgTag(0xA210, "FocalPlaneResolutionUnit", "Focal plane resolution unit") },
                    { 0xA214, new XimgTag(0xA214, "SubjectLocation", "Subject location") },
                    { 0xA217, new XimgTag(0xA217, "SensingMethod", "Sensing method") },
                    { 0xA401, new XimgTag(0xA401, "CustomRendered", "Custom image processing") },
                    { 0xA402, new XimgTag(0xA402, "ExposureMode", "Exposure mode") },
                    { 0xA403, new XimgTag(0xA403, "WhiteBalance", "White balance") },
                    { 0xA405, new XimgTag(0xA405, "FocalLengthIn35mmFilm", "Focal length in 35 mm film") },
                    { 0xA406, new XimgTag(0xA406, "SceneCaptureType", "Scene capture type") },
                    { 0xA408, new XimgTag(0xA408, "Contrast", "Contrast") },
                    { 0xA409, new XimgTag(0xA409, "Saturation", "Saturation") },
                    { 0xA40A, new XimgTag(0xA40A, "Sharpness", "Sharpness") },
                    { 0xA40C, new XimgTag(0xA40C, "SubjectDistanceRange", "Subject distance range") },
                    { 0x1E, new XimgTag(0x1E, "GPSDifferential", "GPS differential correction") },
                    { 0x9201, new XimgTag(0x9201, "ShutterSpeedValue", "Shutter speed") },
                    { 0x9203, new XimgTag(0x9203, "BrightnessValue", "Brightness") },
                    { 0x9204, new XimgTag(0x9204, "ExposureBiasValue", "Exposure bias") },
                    { 0x201, new XimgTag(0x201, "JPEGInterchangeFormat", "Offset to JPEG SOI") },
                    { 0x202, new XimgTag(0x202, "JPEGInterchangeFormatLength", "Bytes of JPEG data") },
                    { 0x11A, new XimgTag(0x11A, "XResolution", "Image resolution in width direction") },
                    { 0x11B, new XimgTag(0x11B, "YResolution", "Image resolution in height direction") },
                    { 0x13E, new XimgTag(0x13E, "WhitePoint", "White point chromaticity") },
                    { 0x13F, new XimgTag(0x13F, "PrimaryChromaticities", "Chromaticities of primaries") },
                    { 0x211, new XimgTag(0x211, "YCbCrCoefficients", "Color space transformation matrix coefficients") },
                    { 0x214, new XimgTag(0x214, "ReferenceBlackWhite", "Pair of black and white reference values") },
                    { 0x9102, new XimgTag(0x9102, "CompressedBitsPerPixel", "Image compression mode") },
                    { 0x829A, new XimgTag(0x829A, "ExposureTime", "Exposure time") },
                    { 0x829D, new XimgTag(0x829D, "FNumber", "F number") },
                    { 0x9202, new XimgTag(0x9202, "ApertureValue", "Aperture") },
                    { 0x9205, new XimgTag(0x9205, "MaxApertureValue", "Maximum lens aperture") },
                    { 0x9206, new XimgTag(0x9206, "SubjectDistance", "Subject distance") },
                    { 0x920A, new XimgTag(0x920A, "FocalLength", "Lens focal length") },
                    { 0xA404, new XimgTag(0xA404, "DigitalZoomRatio", "Digital zoom ratio") },
                    { 0x2, new XimgTag(0x2, "GPSLatitude", "Latitude") },
                    { 0x4, new XimgTag(0x4, "GPSLongitude", "Longitude") },
                    { 0x6, new XimgTag(0x6, "GPSAltitude", "Altitude") },
                    { 0x7, new XimgTag(0x7, "GPSTimeStamp", "GPS time (atomic clock)") },
                    { 0xB, new XimgTag(0xB, "GPSDOP", "Measurement precision") },
                    { 0xD, new XimgTag(0xD, "GPSSpeed", "Speed of GPS receiver") },
                    { 0xF, new XimgTag(0xF, "GPSTrack", "Direction of movement") },
                    { 0x11, new XimgTag(0x11, "GPSImgDirection", "Direction of image") },
                    { 0x14, new XimgTag(0x14, "GPSDestLatitude", "Latitude of destination") },
                    { 0x16, new XimgTag(0x16, "GPSDestLongitude", "Longitude of destination") },
                    { 0x18, new XimgTag(0x18, "GPSDestBearing", "Bearing of destination") },
                    { 0x1A, new XimgTag(0x1A, "GPSDestDistance", "Distance to destination") },
                    { 0x132, new XimgTag(0x132, "DateTime", "File change date and time") },
                    { 0x10E, new XimgTag(0x10E, "ImageDescription", "Image title") },
                    { 0x10F, new XimgTag(0x10F, "Make", "Image input equipment manufacturer") },
                    { 0x110, new XimgTag(0x110, "Model", "Image input equipment model") },
                    { 0x131, new XimgTag(0x131, "Software", "Software used") },
                    { 0x13B, new XimgTag(0x13B, "Artist", "Person who created the image") },
                    { 0x8298, new XimgTag(0x8298, "Copyright", "Copyright holder") },
                    { 0xA004, new XimgTag(0xA004, "RelatedSoundFile", "Related audio file") },
                    { 0x9003, new XimgTag(0x9003, "DateTimeOriginal", "Date and time of original data generation") },
                    { 0x9004, new XimgTag(0x9004, "DateTimeDigitized", "Date and time of digital data generation") },
                    { 0x9290, new XimgTag(0x9290, "SubSecTime", "DateTime subseconds") },
                    { 0x9291, new XimgTag(0x9291, "SubSecTimeOriginal", "DateTimeOriginal subseconds") },
                    { 0x9292, new XimgTag(0x9292, "SubSecTimeDigitized", "DateTimeDigitized subseconds") },
                    { 0xA420, new XimgTag(0xA420, "ImageUniqueID", "Unique image ID") },
                    { 0x8824, new XimgTag(0x8824, "SpectralSensitivity", "Spectral sensitivity") },
                    { 0x1, new XimgTag(0x1, "GPSLatitudeRef", "North or South Latitude") },
                    { 0x3, new XimgTag(0x3, "GPSLongitudeRef", "East or West Longitude") },
                    { 0x8, new XimgTag(0x8, "GPSSatellites", "GPS satellites used for measurement") },
                    { 0x9, new XimgTag(0x9, "GPSStatus", "GPS receiver status") },
                    { 0xA, new XimgTag(0xA, "GPSMeasureMode", "GPS measurement mode") },
                    { 0xC, new XimgTag(0xC, "GPSSpeedRef", "Speed unit") },
                    { 0xE, new XimgTag(0xE, "GPSTrackRef", "Reference for direction of movement") },
                    { 0x10, new XimgTag(0x10, "GPSImgDirectionRef", "Reference for direction of image") },
                    { 0x12, new XimgTag(0x12, "GPSMapDatum", "Geodetic survey data used") },
                    { 0x13, new XimgTag(0x13, "GPSDestLatitudeRef", "Reference for latitude of destination") },
                    { 0x15, new XimgTag(0x15, "GPSDestLongitudeRef", "Reference for longitude of destination") },
                    { 0x17, new XimgTag(0x17, "GPSDestBearingRef", "Reference for bearing of destination") },
                    { 0x19, new XimgTag(0x19, "GPSDestDistanceRef", "Reference for distance to destination") },
                    { 0x1D, new XimgTag(0x1D, "GPSDateStamp", "GPS date") },
                    { 0xA40B, new XimgTag(0xA40B, "DeviceSettingDescription", "Device settings description") },
                    { 0x9000, new XimgTag(0x9000, "ExifVersion", "Exif version") },
                    { 0x9286, new XimgTag(0x9286, "UserComment", "User comments") },
                    { 0x1B, new XimgTag(0x1B, "GPSProcessingMethod", "Name of GPS processing method") },
                    { 0x1C, new XimgTag(0x1C, "GPSAreaInformation", "Name of GPS area") }
                };
                return parseTags;
            }
        }

        #region Private members
        private static string GetComponentsConfig(byte[] bytes)
        {
            string s = "";
            string[] vals = new string[] { "", "Y", "Cb", "Cr", "R", "G", "B" };

            foreach (byte b in bytes)
                s += vals[b];

            return s;
        }

        #endregion
    }
}
