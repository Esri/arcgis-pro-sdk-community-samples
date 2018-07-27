/*

   Copyright 2018 Esri

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

namespace ConfigWithStartWizard.Models {
    public class PathItem {
        
        public PathItem() {
        }

        public PathItem(string pathName) {
            Path = pathName;
        }

        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        public string FileName => System.IO.Path.GetFileName(Path);
        public string Path { get; set; }

        public string PathType
        {
            get
            {
                if (Path == "Blank")
                    return "ArcGIS Pro Template";

                var suffix = System.IO.Path.GetExtension(FileName).ToLower();

                if (suffix == ".aprx")
                    return "ArcGIS Pro Project";
                else if (suffix == ".aptx")
                    return "ArcGIS Pro Template";
                else if (suffix == ".mxd")
                    return "ArcGIS Desktop Map Document";
                else if (suffix == ".ppkx")
                    return "ArcGIS Pro Project Package";
                return "Unknown";
            }
        }

        //TODO - what if the path is a URL?
        public DateTime LastModifiedTime => Path != "Blank" ? System.IO.File.GetLastWriteTime(Path) : DateTime.Now;
    }
}