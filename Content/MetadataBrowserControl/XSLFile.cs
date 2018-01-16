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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetadataBrowserControl
{
    /// <summary>
    /// Represents the stylesheet picked to apply the transform with
    /// </summary>
    internal class XSLFile
    {
        public XSLFile(string fileName)
        {            
             _file = new FileInfo(fileName);           
             _fileName = _file.Name;
             _fileFullName = _file.FullName;            
        }
        private string _fileName;
        public string FileName => _fileName;

        private FileInfo _file;
        public FileInfo File => _file;

        private string _fileFullName;
        public string FileFullName => _fileFullName;

    }
}
