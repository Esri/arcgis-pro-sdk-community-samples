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
using ArcGIS.Desktop.Framework;
using ReusableUserControl.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ReusableUserControl
{
  public class CommonUseWindowViewModel
  {
    private DispatcherTimer _timer = null;

    public CommonUseWindowViewModel()
    {
      // program a timer to update the age field asynchronously 
      _timer = new DispatcherTimer();
      _timer.Interval = TimeSpan.FromSeconds(4);
      _timer.Tick += Timer_Tick;
      _timer.Start();
      Person = new PersonModel()
      {
        Age = 1,
        Name = "Tester",
        Email = "Tester@acme.com"
      };
    }

    public PersonModel Person { get; set; }

    private ICommand _saveInfo = null;

    public ICommand SaveInfo
    {
      get
      {
        if (_saveInfo == null)
        {
          _saveInfo = new RelayCommand(() => 
          {
            // save the info from the current user control
            _timer.IsEnabled = false;
            var output = $@"Name: {Person.Name}, {Person.Age} years old, Email: {Person.Email}";
            ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(output);
            _timer.IsEnabled = true;
          }, () => true);
        }
        return _saveInfo;
      }
    }

    void Timer_Tick(object sender, EventArgs e)
    {
      if (Person.Age > 60) Person.Age = 0;
      Person.Age++;
    }
  }
}
