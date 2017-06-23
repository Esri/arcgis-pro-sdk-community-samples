//   Copyright 2017 Esri
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//       http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using ArcGIS.Desktop.Framework.Contracts;


namespace ControlStyles
{
    public class StyleViewModelBase : ViewModelBase
    {
        private string _styleXaml;

        public string StyleXaml
        {
            get { return _styleXaml; }
            set
            {
                SetProperty(ref _styleXaml, value, () => StyleXaml);

            }
        }
        public virtual void Initialize()
        {

        }

    }
}
