/*

   Copyright 2016 Esri

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
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RemoveAddins
{
    internal class AddInFileInfo
    {
        public AddInFileInfo(string path)
        {
            _AddInFullPath = path;
            _AddInFileName = Path.GetFileName(path);

            Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(path);
            _Image = ToImageSource(icon);

        }

        private string _AddInFileName = "";

        /// <summary>
        /// 
        /// </summary>
        
        public string AddInFileName
        {
            get { return _AddInFileName; }
        }
        
        private string _AddInFullPath = "";
        public string AddInFullPath
        {
            get { return _AddInFullPath; }
        }

        private ImageSource _Image = null;
        public ImageSource Image
        {
            get { return _Image; }
        }
        public ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        public override string ToString()
        {
            return _AddInFileName;
        }

        //is add in selected
        public bool IsSelected{ get; set; }


    }
}
