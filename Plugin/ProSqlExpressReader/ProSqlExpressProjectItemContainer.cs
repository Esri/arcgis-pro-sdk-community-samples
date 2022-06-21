/*

   Copyright 2017 Esri

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
using ArcGIS.Desktop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProSqlExpressReader
{
    internal class ProDataProjectItemContainer : CustomProjectItemContainer<ProDataProjectItem>
    {
        //This should be an arbitrary unique string. It must match your <content type="..." 
        //in the Config.daml for the container
        public static readonly string ContainerName = "ProSqlExpressReaderContainer";

        public ProDataProjectItemContainer() : base(ContainerName)
        {

        }

        /// <summary>
        /// Create item is called whenever a custom item, registered with the container,
        /// is browsed or fetched (eg the user is navigating through different folders viewing
        /// content in the catalog pane).
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="containerType"></param>
        /// <param name="data"></param>
        /// <returns>A custom item created from the input parameters</returns>
        public override Item CreateItem(string name, string path, string containerType, string data)
        {
            var item = ItemFactory.Instance.Create(path);
            if (item is ProDataProjectItemContainer)
            {
                this.Add(item as ProDataProjectItem);
            }
            return item;
        }

        public override ImageSource LargeImage
        {
            get
            {
                var largeImg = new BitmapImage(new Uri(@"pack://application:,,,/ProSqlExpressReader;component/Images/FolderWithGISData32.png"));
                return largeImg;
            }
        }

        public override Task<System.Windows.Media.ImageSource> SmallImage
        {
            get
            {
                var smallImage = new BitmapImage(new Uri(@"pack://application:,,,/ProSqlExpressReader;component/Images/FolderWithGISData16.png"));
                if (smallImage == null) throw new ArgumentException("SmallImage for CustomProjectContainer doesn't exist");
                return Task.FromResult(smallImage as ImageSource);
            }
        }

    }
}
