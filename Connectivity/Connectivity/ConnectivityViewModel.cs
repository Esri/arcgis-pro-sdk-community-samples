/*

   Copyright 2026 Esri

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
using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.SystemCore;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.KnowledgeGraph.FFP;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Connectivity
{
  internal class ConnectivityViewModel : DockPane
  {
    private const string _dockPaneID = "Connectivity_Connectivity";

    public ConnectivityViewModel()
    {
      //Initialize properties
      _isMobileDevice = ArcGIS.Core.SystemCore.Connectivity.IsMobileDevice() ? Visibility.Visible : Visibility.Hidden;
      _isDesktop = !ArcGIS.Core.SystemCore.Connectivity.IsMobileDevice() ? Visibility.Visible : Visibility.Hidden; ;
      _isInAirplaneMode = ArcGIS.Core.SystemCore.Connectivity.IsInAirplaneMode() ? Visibility.Visible : Visibility.Hidden;
      if (_isInAirplaneMode == Visibility.Visible)
      _airplaneStatusIcon = "pack://application:,,,/Connectivity;component/Images/plane_icon.png";
      else
      _airplaneStatusIcon = "pack://application:,,,/Connectivity;component/Images/plane-off_icon.png";

      _internetConnectionStatus = ArcGIS.Core.SystemCore.Connectivity.GetInternetConnectionStatus();

      switch (_internetConnectionStatus)
      {
        case ArcGIS.Core.SystemCore.Connectivity.ConnectionStatus.statusPublic:
          _internetConnectionStatusIcon = "pack://application:,,,/Connectivity;component/Images/connected_icon.png";
          break;
        case ArcGIS.Core.SystemCore.Connectivity.ConnectionStatus.statusDisconnected:
          _internetConnectionStatusIcon = "pack://application:,,,/Connectivity;component/Images/disconnected_icon.png";
          break;
        case ArcGIS.Core.SystemCore.Connectivity.ConnectionStatus.statusPrivate:
          _internetConnectionStatusIcon = "pack://application:,,,/Connectivity;component/Images/limited_icon.png";
          break;
      }
    }

    /// <summary>
    /// Show the DockPane.
    /// </summary>
    internal static void Show()
    {
      DockPane pane = FrameworkApplication.DockPaneManager.Find(_dockPaneID);
      if (pane == null)
        return;

      pane.Activate();
    }

    /// <summary>
    /// Text shown near the top of the DockPane.
    /// </summary>
    private string _heading = "Connectivity DockPane";
    public string Heading
    {
      get => _heading;
      set => SetProperty(ref _heading, value);
    }

    private string _internetConnectionStatusIcon = "pack://application:,,,/Connectivity;component/Images/unknown_icon.png";

    public string InternetConnectionStatusIcon
    {
      get => _internetConnectionStatusIcon;
      set => SetProperty(ref _internetConnectionStatusIcon, value);
    }

    private string _airplaneStatusIcon = string.Empty;

    public string AirplaneStatusIcon
    {
      get => _airplaneStatusIcon;
      set => SetProperty(ref _airplaneStatusIcon, value);
    }

    private ArcGIS.Core.SystemCore.Connectivity.ConnectionStatus _internetConnectionStatus;
    public ArcGIS.Core.SystemCore.Connectivity.ConnectionStatus InternetConnectionStatus
    {
      get => _internetConnectionStatus;
      set => SetProperty(ref _internetConnectionStatus, value);
    }

    private Visibility _isInAirplaneMode = Visibility.Hidden;

    public Visibility isInAirplaneMode
    {
      get => _isInAirplaneMode;
      set => SetProperty(ref _isInAirplaneMode, value);
    }

    private Visibility _isMobileDevice = Visibility.Hidden;

    public Visibility isMobileDevice
    {
      get => _isMobileDevice;
      set => SetProperty(ref _isMobileDevice, value);
    }

    private Visibility _isDesktop = Visibility.Hidden;

    public Visibility isDesktop
    {
      get => _isDesktop;
      set => SetProperty(ref _isDesktop, value);
    }
  }

  /// <summary>
  /// Button implementation to show the DockPane.
  /// </summary>
	internal class Connectivity_ShowButton : Button
  {
    protected override void OnClick()
    {
      ConnectivityViewModel.Show();
    }
  }
}
