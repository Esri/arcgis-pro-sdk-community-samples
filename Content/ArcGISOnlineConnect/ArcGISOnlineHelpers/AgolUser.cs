//   Copyright 2018 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace ArcGISOnlineConnect.ArcGISOnlineHelpers
{
    /// <summary>
    /// Class used to encapsulate ArcGIS Online user
    /// </summary>
    public class AgolUser
    {
        public string username;
        public string fullName;
        public string preferredView;
        public string description;
        public string email;
        public string access;
        public long storageUsage;
        public long storageQuota;
        public string orgId;
        public string role;
        public string[] tags;

        public string culture;
        public string region;
        public string thumbnail;

        public IList<AgolGroup> groups;

        /// <summary>
        /// Convert json string into ArcGIS Online user class
        /// </summary>
        /// <param name="json">returned json string from AGOL user query</param>
        /// <returns>ArcGIS online user class</returns>
        public static AgolUser LoadAgolUser(string json)
        {
            var serializer = new JavaScriptSerializer();
            return serializer.Deserialize<AgolUser>(json);
        }

    }


    /*
    {
  "username": "<username>",
  "fullName": "<first name> <last name>",
  "preferredView": "Web | GIS | null",
  "description": "<description>",
  "email": "<email address>",
  "access": "private | org |  public",
  "storageUsage": <storage used - bytes>,    
  "storageQuota": <storage quota - bytes>,
  "orgId": "<Organization id>",    
  "role":  "org_admin | org_publisher | org_user",    
  "tags": [
      "<tag1>",      
      "<tag2>"    
],
  "culture": "<culture code>",    
  "region": "<region>",   
  "thumbnail": "<file name>",  
  "created": date created shown in UNIX time,
  "modified": date modified shown in UNIX time,  
  "groups": [{
    "id": "<group id>",
    "title": "<group title>",
    "isInvitationOnly": true | false,
    "owner": "<group owner username>",
    "description": "<description>",
    "snippet": "<summary>",
    "tags": [
      "<tag1>",
      "<tag2>",
      "<tag3>"
    ],
    "phone": "<contact>",
    "thumbnail": "<file name>",
    "created": date created shown in UNIX time,
    "modified": date modified shown in UNIX time,  
    "access": "private | org |  public" 
    "userMembership": {
      "username": "<username>",
      "memberType": "<owner>",
      "applications": 0
    }
  }]
}
*/
}
