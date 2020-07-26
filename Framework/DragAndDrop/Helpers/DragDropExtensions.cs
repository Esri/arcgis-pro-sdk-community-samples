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
using ArcGIS.Desktop.Framework.DragDrop;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DragAndDrop.Helpers
{
  public static class DragDropExtensions
  {

      public static bool HasTOCContent(this DropInfo dropInfo)
      {
       return dropInfo?.Data is TOCDragData; 
    }

      public static List<MapMember> GetTOCContent(this DropInfo dropInfo)
      {
        if (!HasTOCContent(dropInfo))
          return new List<MapMember>();
        var tocPayload = dropInfo.Data as TOCDragData;
      //get the content
      return tocPayload.DraggedContent.ToList(); ;
      }

      public static List<T> GetTOCContentOfType<T>(this DropInfo dropInfo) where T : MapMember
      {
        return GetTOCContent(dropInfo).OfType<T>()?.ToList() ?? new List<T>();
      }

      public static string GetMapUri(this DropInfo dropInfo)
      {
        return HasTOCContent(dropInfo) ? ((TOCDragData)dropInfo.Data).SourceMapURI : string.Empty;
      }
    }

  }

