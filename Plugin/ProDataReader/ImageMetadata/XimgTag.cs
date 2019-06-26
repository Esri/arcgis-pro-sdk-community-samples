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
    public class XimgTag
    {
        public XimgTag(int id, string fieldName, string description)
        {
            Id = id;
            Description = description;
            FieldName = fieldName;
        }
        public int Id { get; set; }
        public string Description { get; set; }
        public string FieldName { get; set; }
        public object Value { get; set; }
        public string StrValue { get; set; }
        public int ItemType { get; set; }

        public override string ToString()
        {
            return $@"{Description} ({FieldName}) = {StrValue}";
        }

        public XimgTag Clone() => new XimgTag (Id, Description, FieldName);
    }
}
