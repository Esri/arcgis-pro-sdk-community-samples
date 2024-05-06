/*

   Copyright 2024 Esri

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
using ArcGIS.Desktop.Core.Portal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DockpaneAndThreads
{
  internal class OnlineQuery
  {

    private static Dictionary<string, string> _onlineQueries = new Dictionary<string, string>();
    private string _portalURL = string.Empty;
    private string _contentType = string.Empty;
    private string _path = string.Empty;
    private string _query = string.Empty;
    private string _token = string.Empty;
    private string _format = string.Empty;
    private string _name = string.Empty;
    private string _downloadFileName = string.Empty;
    private int _start = 0;

    static OnlineQuery()
    {
      _onlineQueries.Add("Rule packages", "/sharing/rest/search?q=(type:\"Rule Package\")");
      _onlineQueries.Add("Projects", "/sharing/rest/search?q=(type:\"Project Package\")");
      _onlineQueries.Add("Templates", "/sharing/rest/search?q=(owner:\"esri_solutions\" AND type:\"Project Template\")");
      _onlineQueries.Add("Map Packages", "/sharing/rest/search?q=(type:\"Pro Map\")");
      _onlineQueries.Add("Web Maps", "/sharing/rest/search?q=(type:\"Web Scene\" OR type:\"Web Map\")");
      _onlineQueries.Add("Layers", "/sharing/rest/search?q=(type:\"Layer Package\" OR type:\"Feature Service\" OR type:\"Map Service\")");
      _onlineQueries.Add("Item Data", "/sharing/rest/content/items/{0}/data");
    }

    public static IReadOnlyList<string> Content => _onlineQueries.Keys.ToList();

    public OnlineQuery(string portalURL)
    {
      _portalURL = portalURL;
    }

    public void Configure(string contentType)
    {
      Configure(contentType, string.Empty, 0);
    }

    public void Configure(string contentType, string token)
    {
      Configure(contentType, token, 0);
    }

    public void Configure(string contentType, string token, int start)
    {
      if (!_onlineQueries.ContainsKey(contentType))
        throw new ArgumentException($"Invalid content type: {contentType}");
      _contentType = contentType;
      _token = token;
      _start = start;
      var query = _onlineQueries[_contentType].Split(new char[] { '?' });
      _path = query[0];
      _format = "json";

      _query = query[1] ?? string.Empty;
    }

    public void ConfigureData(string token, string id, string name)
    {
      _contentType = "Item Data";
      _token = token;
      _name = name;
      _start = 0;
      _format = string.Empty;
      _query = string.Format(_onlineQueries["Item Data"], id);
      if (!string.IsNullOrEmpty(_token))
        _query += $"?token={_token}";
    }

    public string URL => _portalURL;
    public string Path => _path;

    public int Start
    {
      get
      {
        return _start;
      }
      set
      {
        _start = value;
      }
    }

    public string Query
    {
      get
      {
        var query = _query;
        if (!string.IsNullOrEmpty(_token))
          query += $"&token={_token}";
        if (_start > 0)
          query += $"&start={_start}";
        query += $"&f={_format}";
        return query;
      }
    }

    public string Name => _name;
    public string FinalUrl => URL + Path + "?" + Query;

    public string DownloadUrl => _portalURL + _query;

    public string DownloadFileName
    {
      get
      {
        if (string.IsNullOrEmpty(_downloadFileName))
          _downloadFileName = UniqueFileName(
                                   System.IO.Path.Combine(
                                        System.IO.Path.Combine(
                                            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), Name));
        return _downloadFileName;
      }
    }

    private PortalQueryParameters _portalQueryParams;
    public PortalQueryParameters PortalQueryParams
    {
      get
      {
        var types = _query.Split(new char[] { '=' });

        _portalQueryParams = new PortalQueryParameters(types[1]);

        _portalQueryParams.StartIndex = _start;
        return _portalQueryParams;
      }
    }

    public Uri FinalUri => new Uri(this.FinalUrl, UriKind.Absolute);

    private static string UniqueFileName(string fileName)
    {
      if (!System.IO.File.Exists(fileName))
        return fileName;
      //var fileName = System.IO.Path.Combine(pathDownload, result.Name);
      var path = System.IO.Path.GetDirectoryName(fileName);
      var temp = System.IO.Path.GetFileNameWithoutExtension(fileName);
      var ext = System.IO.Path.GetExtension(fileName);

      int i = 1;
      while (System.IO.File.Exists(fileName))
      {
        fileName = System.IO.Path.Combine(path, $"{temp} ({i}){ext}");
        i++;
      }
      return fileName;
    }


  }
}
