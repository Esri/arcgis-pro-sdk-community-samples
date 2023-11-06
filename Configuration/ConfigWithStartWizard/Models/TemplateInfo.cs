/*

   Copyright 2019 Esri

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigWithStartWizard.Models
{
  public class TemplateInfo
  {

    public static Task<List<PathItem>> GetDefaultTemplatesAsync()
    {
      return Task.Run(() =>
      {
        return GetDefaultTemplates();
      });
    }

    public static List<PathItem> GetDefaultTemplates()
    {

      var templates = new List<PathItem>();
      templates.Add(new PathItem("Blank"));
      string templatesDir = GetDefaultTemplateFolder();
      if (System.IO.Directory.Exists(templatesDir))
      {
        var defaults = Directory.GetFiles(templatesDir, "*", SearchOption.TopDirectoryOnly)
                .Where(f => f.EndsWith(".ppkx") || f.EndsWith(".aptx")).Select(s => new PathItem(s)).ToList();
        foreach (var pi in defaults)
          templates.Add(pi);
      }
      return templates;
    }
    public static string GetDefaultTemplateFolder()
    {
      string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
      string root = dir.Split(new string[] { @"\bin" }, StringSplitOptions.RemoveEmptyEntries)[0];
      return System.IO.Path.Combine(root, @"Resources\ProjectTemplates");
    }
  }
}

