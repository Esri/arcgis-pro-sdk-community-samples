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
