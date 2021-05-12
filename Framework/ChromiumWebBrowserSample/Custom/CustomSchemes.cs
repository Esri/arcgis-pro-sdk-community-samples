/*

   Copyright 2020 Esri

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
using CefSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ChromiumWebBrowserSample.Custom
{
	/// <summary>
	/// Custom scheme to load resources. This sample loads pngs that have been
	/// stored as resources in the sample add-in assembly
	/// </summary>
	/// <remarks>The custom scheme name is &quot;resource&quot;</remarks>
	public class ImageResourceSchemeHandler : ResourceHandler
	{
		/// <summary>
		/// 
		/// </summary>
		public ImageResourceSchemeHandler()
		{

		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
		{
			using (callback)
			{
				var url = request.Url;
				var image_path = url.Replace(@"/", "").Replace(@"resource:", "");
				MimeType = Cef.GetMimeType(Path.GetExtension(image_path));

				var asm_name = Assembly.GetExecutingAssembly().GetName().Name;
				var bitmap = new BitmapImage(new Uri(
	         $"pack://application:,,,/{asm_name};component/Images/{image_path}", UriKind.Absolute));

				//make image a little bigger (just for purposes of visibility)
				var scale = new ScaleTransform()
				{
					ScaleX = 2,
					ScaleY = 2
				};
				var transformBmp = new TransformedBitmap(bitmap, scale);

				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(transformBmp));
				MemoryStream mem_stream = new MemoryStream();
				encoder.Save(mem_stream);
				mem_stream.Position = 0;

				Stream = mem_stream;
				StatusCode = (int)HttpStatusCode.OK;
				callback.Continue();
			}
			return CefReturnValue.Continue;
		}
	}

	/// <summary>
	/// Custom scheme to load embedded resources. This sample loads an html page that has been
	/// stored as an embedded resource in the sample add-in assembly
	/// </summary>
	/// <remarks>The custom scheme name is &quot;embeddedresource&quot;<br/>
	/// Note: not every "common" file type can be shown in Chrome - many, like
	/// .docx (and other office formats) require a Chrome extension.<br/>
	/// In other words, just because you embedd a particular file within your add-in 
	/// doesn't necessarily mean that Chrome can display it.
	/// </remarks>
	public class EmbeddedResourceSchemeHandler : ResourceHandler
	{
		/// <summary>
		/// 
		/// </summary>
		public EmbeddedResourceSchemeHandler()
		{

		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
		{
			using (callback)
			{
				var url = request.Url;
				var content_path = url.Replace(@"/", "").Replace(@"embeddedresource:", "");
				var asm_name = Assembly.GetExecutingAssembly().GetName().Name;
				var resource_path = $@"{asm_name}.Content.{content_path}";

				var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource_path);
				if (stream != null)
				{
					MimeType = Cef.GetMimeType(Path.GetExtension(content_path));
					Stream = stream;
					ResponseLength = stream.Length;
					StatusCode = (int)System.Net.HttpStatusCode.OK;
					callback.Continue();
				}
			}
			return CefReturnValue.Continue;
		}
	}
}
