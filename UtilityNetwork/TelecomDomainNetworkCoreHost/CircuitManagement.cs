using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using ArcGIS.Core.Data;
using ArcGIS.Core.Data.UtilityNetwork;
using ArcGIS.Core.Data.UtilityNetwork.Telecom;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Hosting;
using Microsoft.Win32;

namespace CircuitManagement
{
  /// <summary>
  /// Builds and manages telecom circuits and equipment in a mobile-geodatabase utility network using a standalone CoreHost application.
  /// The sample creates an optical line terminal (OLT), an optical network terminal (ONT), a splitter, ports, fiber cables, and fiber strands. It establishes containment and connectivity associations, validates the network topology, and creates and verifies physical circuits.
  /// In addition, the sample demonstrates equipment management by querying and reserving unit identifiers on a splitter and resizing an ONT port group.
  /// </summary>
  /// <remarks>
  /// 1. Install and license ArcGIS Pro 3.8 and install the .NET 10 SDK. Use Visual Studio 2026 or VS Code with C# support to build the project.
  /// 1. Get the telecom utility network data from the [ArcGIS Pro SDK community samples releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) and place the working copy at **C:\Data\TelecomUtilityNetwork.geodatabase**.
  /// 1. Open `CircuitManagement.slnx` in Visual Studio, or open `CircuitManagement.csproj` in your development environment. Build the project and run the `CircuitManagement` console application.
  /// 1. The console reports the ObjectIDs of the devices, ports, cables, and fiber strands as they are created. The application then creates associations, validates the network topology, creates and verifies two physical circuits, reserves splitter units, and resizes the ONT port group.
  /// 1. To inspect the persisted results, open ArcGIS Pro after the application finishes and add the utility network from the working mobile geodatabase to a map.
  /// 
  /// ### Circuit management
  /// 
  /// The sample opens the utility network in the mobile geodatabase and obtains the telecom domain network named `Telco`. It reads the domain network's circuit properties, wavelength schemes, color schemes, color sets, and combine and divide policies.
  /// It creates the following network:
  /// - An OLT with port units 1 through 8.
  /// - A splitter with port group A covering units 1 through 8 and port group B covering units 9 through 12. A junction-junction connectivity association connects the two port groups.
  /// - An ONT with port units 1 through 4.
  /// - A fiber cable containing strand units 1 through 8 between the OLT and splitter, and a second cable containing strand units 1 through 4 between the splitter and ONT.
  /// Containment associations place the ports inside their devices and the fiber strands inside their cables. Junction-edge object connectivity associations connect the ports to the fiber strands.
  /// ![OLT connected to an ONT through a splitter, showing port groups, fiber unit ranges, and junction-junction connectivity](Screenshots/Screenshot1.PNG)
  /// 
  /// After validating the network topology, the sample uses `CircuitManager` to create two physical circuits:
  /// - `OLT_ONT_1_4_SectionedCircuit`: a sectioned circuit from OLT units 1 through 4 to the ONT through the splitter, with two sections connected in series.
  /// - `OLT_Splitter_5_8_Circuit`: a nonsectioned circuit from OLT units 5 through 8 to splitter port group A units 5 through 8. The sample also sets a user-defined circuit attribute.
  /// 
  /// Both circuits are verified with `ForceVerify` and `SynthesizeGeometries` enabled.
  /// ### Equipment management
  /// The sample uses `UnitIdentifierManager` to query the splitter's unit identifiers, reserve units 15 through 20 on the splitter, and query the updated unit information. It also resizes the ONT port group by changing its last unit from 4 to 8.
  /// The following diagram highlights the ONT port group after resizing. The fiber strand ranges remain unchanged.
  /// 
  /// ![ONT port group resized from last unit 4 to last unit 8, with the existing fiber strand ranges unchanged](Screenshots/Screenshot2.PNG)
  /// The final diagram includes an additional splitter position labeled C to illustrate the equipment reservation scenario. The code reserves unit identifiers 15 through 20 on the splitter; it does not create an additional port object at C.
  /// 
  /// ![Equipment management schematic showing splitter position C and the resized ONT port group](Screenshots/Screenshot3.PNG)
  /// 
  /// ### Using other data
  /// The sample is configured for the supplied mobile geodatabase. It expects a `Telco` telecom domain network, the `TelcoDevice`, `TelcoLine`, `TelcoJunctionObject`, and `TelcoEdgeObject` datasets, and the asset groups, asset types, fields, association rules, and circuit configuration used in `CircuitManagement.cs`.
  /// To use another dataset, update the hard-coded schema names, unit ranges, color scheme, circuit attributes, and geometries as needed. Changing only the geodatabase path is not sufficient unless the replacement data has the same configuration.
  /// </remarks>
  internal class Program
  {
    // ArcGIS Pro installation path. This is used to load ArcGIS Pro assemblies and native DLLs.
    private static string _arcgisProPath = "";

    private static string gdbPath = @"C:\Data\TelecomUtilityNetwork.geodatabase";

    // [STAThread] must be present on the application entry point
    [STAThread]
    static void Main(string[] args)
    {
      try
      {
        // Get the Pro installation path from the registry
        _arcgisProPath = GetInstallDirAndVersionFromReg().path;
        if (string.IsNullOrEmpty(_arcgisProPath)) throw new InvalidOperationException("ArcGIS Pro is not installed.");
        // Resolve ArcGIS Pro assembly paths
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyResolve += new ResolveEventHandler(ResolveProAssemblyPath);
        // Perform CoreHost tasks
        PerformCoreHostTask(args);
      }
      catch (Exception ex)
      {
        // Error (missing installation, no license, 64 bit mismatch, etc.)
        Console.Error.WriteLine(ex.Message);
        if (ex.InnerException != null)
        {
          Console.Error.WriteLine("Inner Exception:");
          Console.Error.WriteLine(ex.InnerException);
        }
        return;
      }
    }

    private static void PerformCoreHostTask(string[] args)
    {
      Host.Initialize();

      using (Geodatabase geodatabase = new Geodatabase(new MobileGeodatabaseConnectionPath(new Uri(gdbPath))))
      using (UtilityNetwork utilityNetwork = GetUtilityNetwork(geodatabase))
      {
        // Get the Telecom Domain Network named "Telco" 
        TelecomDomainNetwork telecomDomainNetwork = GetTelecomDomainNetwork(utilityNetwork, "Telco");

        TelecomElement ontPortTelecomElement = null;
        TelecomElement splitterTelecomElement = null;

        // Create a circuit between the OLT and ONT
        CircuitManagement(geodatabase, utilityNetwork, telecomDomainNetwork, ref splitterTelecomElement, ref ontPortTelecomElement);

        // Reserve port space and resize the ONT port group by changing its last unit from 4 to 8
        EquipmentManagement(geodatabase, utilityNetwork, splitterTelecomElement, ontPortTelecomElement);
      }
    }

    // Get the Utility Network from the geodatabase
    private static UtilityNetwork GetUtilityNetwork(Geodatabase geodatabase)
    {
      IReadOnlyList<UtilityNetworkDefinition> utilityNetworkDefinitions = geodatabase.GetDefinitions<UtilityNetworkDefinition>();
      UtilityNetworkDefinition utilityNetworkDefinition = utilityNetworkDefinitions.FirstOrDefault();
      if (utilityNetworkDefinition == null)
      {
        throw new Exception($"Utility Network not found in the geodatabase.");
      }
      return geodatabase.OpenDataset<UtilityNetwork>(utilityNetworkDefinition.GetName());
    }

    // Get the Telecom Domain Network and its properties from the Utility Network
    private static TelecomDomainNetwork GetTelecomDomainNetwork(UtilityNetwork utilityNetwork, string domainNetworkName)
    {
      UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition();
      TelecomDomainNetwork telecomDomainNetwork = utilityNetworkDefinition.GetDomainNetwork(domainNetworkName) as TelecomDomainNetwork;
      if (telecomDomainNetwork == null)
      {
        throw new Exception($"Telecom Domain Network '{domainNetworkName}' not found in the Utility Network.");
      }

      // Get telecom domain network properties
      CircuitProperties circutProperties = telecomDomainNetwork.CircuitProperties;
      IReadOnlyList<WavelengthScheme> wavelengthSchemes = telecomDomainNetwork.WavelengthSchemes;
      IReadOnlyList<ColorScheme> colorSchemes = telecomDomainNetwork.ColorSchemes;
      IReadOnlyList<ColorSet> colorSets = telecomDomainNetwork.ColorSets;
      IReadOnlyList<CombinePolicy> combinePolicies = telecomDomainNetwork.CombinePolicies;
      IReadOnlyList<DividePolicy> dividePolicies = telecomDomainNetwork.DividePolicies;

      return telecomDomainNetwork;
    }


    // Create a circuit between the OLT and ONT via the splitter
    private static void CircuitManagement(Geodatabase geodatabase, UtilityNetwork utilityNetwork, TelecomDomainNetwork telecomDomainNetwork, ref TelecomElement splitterTelecomElement, ref TelecomElement ontPortTelecomElement)
    {
      SpatialReference mercatorSR = SpatialReferenceBuilder.CreateSpatialReference(3857);
      MapPoint oltMapPoint = MapPointBuilderEx.CreateMapPoint(-13044701.12, 4036266.43, 0, mercatorSR);
      MapPoint ontMapPoint = MapPointBuilderEx.CreateMapPoint(-12973796.82, 4006067.14, 0, mercatorSR);
      MapPoint splitterMapPoint = MapPointBuilderEx.CreateMapPoint(-13012976.52, 4019638.58, 0, mercatorSR);
      Polyline oltToSplitterLineGeom = PolylineBuilderEx.CreatePolyline(new List<MapPoint> { oltMapPoint, splitterMapPoint }, mercatorSR);
      Polyline splitterToOntLineGeom = PolylineBuilderEx.CreatePolyline(new List<MapPoint> { splitterMapPoint, ontMapPoint }, mercatorSR);

      #region Telecom Elements
      // Local copies to avoid capturing ref parameters in lambdas
      TelecomElement localSplitterTelecomElement = null;
      TelecomElement localOntPortTelecomElement = null;

      TelecomElement oltTelecomElement = null;
      TelecomElement ontTelecomElement = null;
      TelecomElement oltPortTelecomElement = null;
      TelecomElement splitterPortTelecomElement_4 = null;
      TelecomElement splitterPortTelecomElement_8 = null;
      TelecomElement oltToSplitter = null;
      TelecomElement splitterToOnt = null;
      TelecomElement fiberStrandTelecomElement_4 = null;
      TelecomElement fiberStrandTelecomElement_8 = null;
      #endregion

      using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
      using (FeatureClass telcoDeviceFc = geodatabase.OpenDataset<FeatureClass>("TelcoDevice"))
      using (NetworkSource telcoDeviceNetworkSource = GetNetworkSource(utilityNetworkDefinition, telcoDeviceFc.GetName()))
      using (FeatureClass telcoLineFc = geodatabase.OpenDataset<FeatureClass>("TelcoLine"))
      using (NetworkSource telcoLineNetworkSource = GetNetworkSource(utilityNetworkDefinition, telcoLineFc.GetName()))
      using (Table telcoEdgeObject = geodatabase.OpenDataset<Table>("TelcoEdgeObject"))
      using (NetworkSource telcoEdgeObjectNetworkSource = GetNetworkSource(utilityNetworkDefinition, telcoEdgeObject.GetName()))
      using (Table telcoJunctionObject = geodatabase.OpenDataset<Table>("TelcoJunctionObject"))
      using (NetworkSource telcoJunctionObjectNetworkSource = GetNetworkSource(utilityNetworkDefinition, telcoJunctionObject.GetName()))
      {
        // Create an optical line terminal (OLT) device located at an ISP central office or a local exchange
        Subtype oltSubtype = telcoDeviceFc.GetDefinition().GetSubtypes().First(s => s.GetName() == "OLT");
        geodatabase.ApplyEdits(() => {
          using (RowBuffer oltRowBuffer = telcoDeviceFc.CreateRowBuffer(oltSubtype))
          {
            // Set the geometry for the OLT device
            oltRowBuffer["SHAPE"] = oltMapPoint;

            // Set the asset type for the OLT device
            oltRowBuffer["ASSETTYPE"] = telcoDeviceNetworkSource.GetAssetGroup(oltSubtype.GetName()).GetAssetType("OLT").Code;

            using (Feature oltFeature = telcoDeviceFc.CreateRow(oltRowBuffer) as Feature)
            {
              oltTelecomElement = utilityNetwork.CreateElement(oltFeature) as TelecomElement;
              Console.WriteLine($"Created OLT device with ObjectID: {oltFeature.GetObjectID()}");
            }
          }
        });

        // Create an optical network terminal (ONT) device located on the customer's premises
        Subtype ontSubtype = telcoDeviceFc.GetDefinition().GetSubtypes().First(s => s.GetName() == "ONT");
        geodatabase.ApplyEdits(() => {
          using (RowBuffer ontRowBuffer = telcoDeviceFc.CreateRowBuffer(ontSubtype))
          {
            // Set the geometry for the ONT device
            ontRowBuffer["SHAPE"] = ontMapPoint;

            // Set the asset type for the ONT device
            ontRowBuffer["ASSETTYPE"] = telcoDeviceNetworkSource.GetAssetGroup(ontSubtype.GetName()).GetAssetType("ONT").Code;

            using (Feature ontFeature = telcoDeviceFc.CreateRow(ontRowBuffer) as Feature)
            {
              ontTelecomElement = utilityNetwork.CreateElement(ontFeature) as TelecomElement;
              Console.WriteLine($"Created ONT device with ObjectID: {ontFeature.GetObjectID()}");
            }
          }
        });

        // Create a splitter device
        Subtype splitterSubtype = telcoDeviceFc.GetDefinition().GetSubtypes().First(s => s.GetName() == "Splitter");
        geodatabase.ApplyEdits(() => {
          using (RowBuffer splitterRowBuffer = telcoDeviceFc.CreateRowBuffer(splitterSubtype))
          {
            // Set the geometry for the splitter device
            splitterRowBuffer["SHAPE"] = splitterMapPoint;

            // Set the asset type for the splitter device
            splitterRowBuffer["ASSETTYPE"] = telcoDeviceNetworkSource.GetAssetGroup(splitterSubtype.GetName()).GetAssetType("Splitter").Code;

            using (Feature splitterFeature = telcoDeviceFc.CreateRow(splitterRowBuffer) as Feature)
            {
              localSplitterTelecomElement = utilityNetwork.CreateElement(splitterFeature) as TelecomElement;
              Console.WriteLine($"Created Splitter device with ObjectID: {splitterFeature.GetObjectID()}");
            }
          }
        });

        // Create ports for the OLT and ONT devices
        Subtype portSubtype = telcoJunctionObject.GetDefinition().GetSubtypes().First(s => s.GetName() == "Port");
        geodatabase.ApplyEdits(() => {
          using (RowBuffer portBuffer = telcoJunctionObject.CreateRowBuffer(portSubtype))
          {
            portBuffer["FIRSTUNIT"] = 1;
            portBuffer["LASTUNIT"] = 8;
            portBuffer["ASSETTYPE"] = telcoJunctionObjectNetworkSource.GetAssetGroup(portSubtype.GetName()).GetAssetType("Circuit Location").Code;

            // OLT port (units 1 through 8)
            using (Row oltPort = telcoJunctionObject.CreateRow(portBuffer))
            {
              oltPortTelecomElement = utilityNetwork.CreateElement(oltPort) as TelecomElement;
              Console.WriteLine($"Created OLT Port with ObjectID: {oltPort.GetObjectID()}");
            }

            // ONT port (units 1 through 4)
            portBuffer["LASTUNIT"] = 4;
            using (Row ontPort = telcoJunctionObject.CreateRow(portBuffer))
            {
              localOntPortTelecomElement = utilityNetwork.CreateElement(ontPort) as TelecomElement;
              Console.WriteLine($"Created ONT Port with ObjectID: {ontPort.GetObjectID()}");
            }

            portBuffer["ASSETTYPE"] = telcoJunctionObjectNetworkSource.GetAssetGroup(portSubtype.GetName()).GetAssetType("Splitter Port Circuit Location").Code;
            portBuffer["FIRSTUNIT"] = 1;
            portBuffer["LASTUNIT"] = 8;

            // Splitter port (units 1 through 8)
            using (Row splitterPort = telcoJunctionObject.CreateRow(portBuffer))
            {
              splitterPortTelecomElement_8 = utilityNetwork.CreateElement(splitterPort) as TelecomElement;
              Console.WriteLine($"Created Splitter Port (8 units) with ObjectID: {splitterPort.GetObjectID()}");
            }

            // Splitter port (units 9 through 12)
            portBuffer["FIRSTUNIT"] = 9;
            portBuffer["LASTUNIT"] = 12;

            using (Row splitterPort = telcoJunctionObject.CreateRow(portBuffer))
            {
              splitterPortTelecomElement_4 = utilityNetwork.CreateElement(splitterPort) as TelecomElement;
              Console.WriteLine($"Created Splitter Port (4 units) with ObjectID: {splitterPort.GetObjectID()}");
            }
          }
        });

        // Create telco fiber lines connecting the OLT and ONT via the splitter
        Subtype telcoFiberLineSubtype = telcoLineFc.GetDefinition().GetSubtypes().First(s => s.GetName() == "Cable");
        geodatabase.ApplyEdits((Action)(() => {
          using (RowBuffer telcoFiberLineRowBuffer = telcoLineFc.CreateRowBuffer())
          {
            telcoFiberLineRowBuffer["ASSETGROUP"] = telcoFiberLineSubtype.GetCode();

            // Set the geometry for the telco fiber line
            telcoFiberLineRowBuffer["SHAPE"] = oltToSplitterLineGeom;
            telcoFiberLineRowBuffer["COLORSCHEME"] = 2;

            // Set the asset type for the telco fiber line
            telcoFiberLineRowBuffer["ASSETTYPE"] = telcoLineNetworkSource.GetAssetGroup(telcoFiberLineSubtype.GetName()).GetAssetType("Fiber").Code;

            using (Feature oltToSplitterFiberLine = telcoLineFc.CreateRow(telcoFiberLineRowBuffer))
            {
              oltToSplitter = utilityNetwork.CreateElement(oltToSplitterFiberLine) as TelecomElement;
              Console.WriteLine($"Created Telco Fiber Line with ObjectID: {oltToSplitterFiberLine.GetObjectID()}");
            }

            // Set the geometry for the telco fiber line
            telcoFiberLineRowBuffer["SHAPE"] = splitterToOntLineGeom;
            using (Feature splitterToOntFiberLineFeature = telcoLineFc.CreateRow(telcoFiberLineRowBuffer))
            {
              splitterToOnt = utilityNetwork.CreateElement(splitterToOntFiberLineFeature) as TelecomElement;
              Console.WriteLine($"Created Telco Fiber Line with ObjectID: {splitterToOntFiberLineFeature.GetObjectID()}");
            }
          }
        }));

        // Create fiber strands
        Subtype telcoFiberStrandSubtype = telcoEdgeObject.GetDefinition().GetSubtypes().First(s => s.GetName() == "Fiber");
        geodatabase.ApplyEdits(() => {
          using (RowBuffer fiberStrandRowBuffer = telcoEdgeObject.CreateRowBuffer(telcoFiberStrandSubtype))
          {
            fiberStrandRowBuffer["ASSETTYPE"] = telcoEdgeObjectNetworkSource.GetAssetGroup(telcoFiberStrandSubtype.GetName()).GetAssetType("Fiber").Code;

            fiberStrandRowBuffer["FIRSTUNIT"] = 1;
            fiberStrandRowBuffer["LASTUNIT"] = 8;

            using (Row fiberStrand = telcoEdgeObject.CreateRow(fiberStrandRowBuffer))
            {
              fiberStrandTelecomElement_8 = utilityNetwork.CreateElement(fiberStrand) as TelecomElement;
              Console.WriteLine($"Created Telco Fiber strand with ObjectID: {fiberStrand.GetObjectID()}");
            }

            fiberStrandRowBuffer["LASTUNIT"] = 4;

            using (Row fiberStrand = telcoEdgeObject.CreateRow(fiberStrandRowBuffer))
            {
              fiberStrandTelecomElement_4 = utilityNetwork.CreateElement(fiberStrand) as TelecomElement;
              Console.WriteLine($"Created Telco Fiber strand with ObjectID: {fiberStrand.GetObjectID()}");
            }
          }
        });

        // Create a containment association between the OLT and its port
        Association oltDeviceToPortAssociation = new Association(AssociationType.Containment, oltTelecomElement, oltPortTelecomElement, ContainmentVisibility.ContentNotVisible);

        // Create a containment association between the ONT and its port
        Association ontDeviceToPortAssociation = new Association(AssociationType.Containment, ontTelecomElement, localOntPortTelecomElement, ContainmentVisibility.ContentNotVisible);

        // Create containment associations between the splitter and its ports
        Association splitterDeviceToPortAssociation_4 = new Association(AssociationType.Containment, localSplitterTelecomElement, splitterPortTelecomElement_4, ContainmentVisibility.ContentNotVisible);
        Association splitterDeviceToPortAssociation_8 = new Association(AssociationType.Containment, localSplitterTelecomElement, splitterPortTelecomElement_8, ContainmentVisibility.ContentNotVisible);

        // Create a junction-junction connectivity association between splitter port units 1 through 8 and port units 9 through 12
        Association splitterPortsJJAssociation = new Association(AssociationType.JunctionJunctionConnectivity, splitterPortTelecomElement_8, splitterPortTelecomElement_4);

        // Create a junction-edge object connectivity association between the OLT port and the fiber strand (from side)
        Association oltPortToFiberStrandAssociation = new Association(AssociationType.JunctionEdgeObjectConnectivityFromSide, oltPortTelecomElement, fiberStrandTelecomElement_8);

        // Create junction-edge object connectivity associations between the splitter ports and the fiber strands
        Association splitterPortAndFiberStrandAssociationTo = new Association(AssociationType.JunctionEdgeObjectConnectivityToSide, splitterPortTelecomElement_8, fiberStrandTelecomElement_8);
        Association splitterPortAndFiberStrandAssociationFrom = new Association(AssociationType.JunctionEdgeObjectConnectivityFromSide, splitterPortTelecomElement_4, fiberStrandTelecomElement_4);

        // Create a junction-edge object connectivity association between the ONT port and the fiber strand (to side)
        Association ontPortToFiberStrandAssociation = new Association(AssociationType.JunctionEdgeObjectConnectivityToSide, localOntPortTelecomElement, fiberStrandTelecomElement_4);

        // Create containment associations between the fiber lines and their fiber strands
        Association fiberLineToFiberStrands_4 = new Association(AssociationType.Containment, splitterToOnt, fiberStrandTelecomElement_4, ContainmentVisibility.ContentNotVisible);
        Association fiberLineToFiberStrands_8 = new Association(AssociationType.Containment, oltToSplitter, fiberStrandTelecomElement_8, ContainmentVisibility.ContentNotVisible);

        geodatabase.ApplyEdits(() => {
          utilityNetwork.AddAssociation(oltDeviceToPortAssociation);
          utilityNetwork.AddAssociation(ontDeviceToPortAssociation);

          utilityNetwork.AddAssociation(splitterDeviceToPortAssociation_8);
          utilityNetwork.AddAssociation(splitterDeviceToPortAssociation_4);
          utilityNetwork.AddAssociation(splitterPortsJJAssociation);

          utilityNetwork.AddAssociation(oltPortToFiberStrandAssociation);
          utilityNetwork.AddAssociation(ontPortToFiberStrandAssociation);

          utilityNetwork.AddAssociation(splitterPortAndFiberStrandAssociationFrom);
          utilityNetwork.AddAssociation(splitterPortAndFiberStrandAssociationTo);

          utilityNetwork.AddAssociation(fiberLineToFiberStrands_4);
          utilityNetwork.AddAssociation(fiberLineToFiberStrands_8);
        });


        // Validate the network topology
        utilityNetwork.ValidateNetworkTopology();

        // Create a circuit between ONT units 1 through 4 and OLT units 1 through 4 via the splitter
        using (CircuitManager circuitManager = utilityNetwork.GetCircuitManager(telecomDomainNetwork))
        using (Circuit oltOntCircuit = new Circuit(circuitManager))
        {
          oltOntCircuit.SetName("OLT_ONT_1_4_SectionedCircuit");
          oltOntCircuit.SetSectioned(true);
          oltOntCircuit.SetCircuitType(CircuitType.Physical);

          CircuitSection oltToSplitterSection = new CircuitSection(circuitManager);
          oltToSplitterSection.SetStartLocation(new CircuitLocation(oltPortTelecomElement) { FirstUnit = 1, LastUnit = 4 });
          oltToSplitterSection.SetStopLocation(new CircuitLocation(splitterPortTelecomElement_8) { FirstUnit = 1, LastUnit = 4 });

          CircuitSection splitterToOntSection = new CircuitSection(circuitManager);
          splitterToOntSection.SetStartLocation(new CircuitLocation(splitterPortTelecomElement_4));
          splitterToOntSection.SetStopLocation(new CircuitLocation(localOntPortTelecomElement));

          // Add sections in series
          Dictionary<CircuitSection, List<CircuitSection>> oltToOntCircuitSections = new Dictionary<CircuitSection, List<CircuitSection>>
          {
            { oltToSplitterSection, new List<CircuitSection>() { splitterToOntSection } },
            { splitterToOntSection, new List<CircuitSection>() { } }
          };

          oltOntCircuit.SetCircuitSections(oltToOntCircuitSections);
          circuitManager.Create(oltOntCircuit);
        }

        // Create a circuit between OLT units 5 through 8 and splitter units 5 through 8
        using (CircuitManager circuitManager = utilityNetwork.GetCircuitManager(telecomDomainNetwork))
        using (Circuit oltSplitterCircuit = new Circuit(circuitManager))
        {
          oltSplitterCircuit.SetName("OLT_Splitter_5_8_Circuit");
          oltSplitterCircuit.SetSectioned(false);
          oltSplitterCircuit.SetCircuitType(CircuitType.Physical);
          oltSplitterCircuit.SetStartLocation(new CircuitLocation(oltPortTelecomElement) { FirstUnit = 5, LastUnit = 8 });
          oltSplitterCircuit.SetStopLocation(new CircuitLocation(splitterPortTelecomElement_8) { FirstUnit = 5, LastUnit = 8 });

          // Set the value of a user-defined circuit attribute field (DESCRIPTION)
          IReadOnlyList<Field> circuitUserDefinedFields = circuitManager.GetCircuitUserFields();
          oltSplitterCircuit[circuitUserDefinedFields[0].Name] = "OLT-Splitter Connection";

          circuitManager.Create(oltSplitterCircuit);
        }

        // Get the circuits
        using (CircuitManager circuitManager = utilityNetwork.GetCircuitManager(telecomDomainNetwork))
        using (Circuit oltToSplitterCircuit = circuitManager.GetCircuits(new CircuitFilter(["OLT_Splitter_5_8_Circuit"])).First())
        using (Circuit oltToOntCircuit = circuitManager.GetCircuits(new CircuitFilter(["OLT_ONT_1_4_SectionedCircuit"])).First())
        {
          // Verify the circuits
          circuitManager.Verify([oltToSplitterCircuit.GetName(), oltToOntCircuit.GetName()], new CircuitVerifyOptions() { ForceVerify = true, SynthesizeGeometries = true });
        }
      }

      // Assign the local values back to the ref parameters after the lambdas complete
      splitterTelecomElement = localSplitterTelecomElement;
      ontPortTelecomElement = localOntPortTelecomElement;
    }

    // Equipment management - Reserve port space in the splitter device and resize the ONT port group by changing its last unit from 4 to 8
    private static void EquipmentManagement(Geodatabase geodatabase, UtilityNetwork utilityNetwork, TelecomElement splitterTelecomElement, TelecomElement ontPortTelecomElement)
    {
      using (UtilityNetworkDefinition utilityNetworkDefinition = utilityNetwork.GetDefinition())
      using (FeatureClass telcoDeviceFc = geodatabase.OpenDataset<FeatureClass>("TelcoDevice"))
      using (NetworkSource telcoDeviceNetworkSource = GetNetworkSource(utilityNetworkDefinition, telcoDeviceFc.GetName()))
      using (FeatureClass telcoLineFc = geodatabase.OpenDataset<FeatureClass>("TelcoLine"))
      using (NetworkSource telcoLineNetworkSource = GetNetworkSource(utilityNetworkDefinition, telcoLineFc.GetName()))
      using (Table telcoEdgeObject = geodatabase.OpenDataset<Table>("TelcoEdgeObject"))
      using (NetworkSource telcoEdgeObjectNetworkSource = GetNetworkSource(utilityNetworkDefinition, telcoEdgeObject.GetName()))
      using (Table telcoJunctionObject = geodatabase.OpenDataset<Table>("TelcoJunctionObject"))
      using (NetworkSource telcoJunctionObjectNetworkSource = GetNetworkSource(utilityNetworkDefinition, telcoJunctionObject.GetName()))
      using (UnitIdentifierManager unitIdentifierManager = utilityNetwork.GetUnitIdentifierManager())
      {
        // Reserve port space in the splitter device
        UnitIdentifier splitterUnitIdentifier = new UnitIdentifier(telcoDeviceNetworkSource, splitterTelecomElement.GlobalID);
        IReadOnlyList<UnitQueryResult> splitterQueryResult = unitIdentifierManager.Query([splitterUnitIdentifier]);

        unitIdentifierManager.ReserveUnitIDs(splitterUnitIdentifier, 15, 20);

        splitterQueryResult = unitIdentifierManager.Query([splitterUnitIdentifier]);

        // Resize the ONT port group by changing its last unit from 4 to 8
        UnitIdentifier ontPortUnitIdentifier = new UnitIdentifier(telcoJunctionObjectNetworkSource, ontPortTelecomElement.GlobalID);
        unitIdentifierManager.Resize(ontPortUnitIdentifier, 8);
      }
    }

    public static NetworkSource GetNetworkSource(UtilityNetworkDefinition unDefinition, string name)
    {
      IReadOnlyList<NetworkSource> allSources = unDefinition.GetNetworkSources();
      foreach (NetworkSource source in allSources)
      {
        if (name.Contains("Partitioned Sink"))
        {
          if (source.Name.Replace(" ", "").ToUpper().Contains(name.Replace(" ", "").ToUpper()) ||
            source.Name.Replace(" ", "").ToUpper().Contains(name.Replace("Partitioned Sink", "Part_Sink").Replace(" ", "").ToUpper()))
          {
            return source;
          }
        }
        if (name.Contains("Hierarchical Sink"))
        {
          if (source.Name.Replace(" ", "").ToUpper().Contains(name.Replace(" ", "").ToUpper()) ||
            source.Name.Replace(" ", "").ToUpper().Contains(name.Replace("Hierarchical Sink", "Hier_Sink").Replace(" ", "").ToUpper()))
          {
            return source;
          }
        }

        if (source.Name.Replace(" ", "").ToUpper().Contains(name.Replace(" ", "").ToUpper()))
        {
          return source;
        }
      }
      return null;
    }

    /// <summary>
    /// Resolves the ArcGIS Pro Assembly Path.  Called when loading of an assembly fails.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <returns>programmatically loaded assembly in the pro /bin path</returns>
    static Assembly ResolveProAssemblyPath(object sender, ResolveEventArgs args)
    {
      string assemblyPath = Path.Combine(_arcgisProPath, "bin", new AssemblyName(args.Name).Name + ".dll");
      if (!File.Exists(assemblyPath)) return null;
      Assembly assembly = Assembly.LoadFrom(assemblyPath);
      return assembly;
    }

    /// <summary>
    /// Gets the ArcGIS Pro install location, major version, and build number from the registry.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception">InvalidOperationException</exception>
    internal static (string path, string version, string buildNo) GetInstallDirAndVersionFromReg()
    {
      string regKeyName = "ArcGISPro";
      string regPath = $@"SOFTWARE\ESRI\{regKeyName}";

      string err1 = $@"Install location of ArcGIS Pro cannot be found. Please check your registry for HKLM\{regPath}\InstallDir";
      string err2 = $@"Version of ArcGIS Pro cannot be determined. Please check your registry for HKLM\{regPath}\Version";
      string err3 = $@"Build Number of ArcGIS Pro cannot be determined. Please check your registry for HKLM\{regPath}\BuildNumber";
      string path;
      string version;
      string buildNo;
      try
      {
        RegistryKey localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
        RegistryKey esriKey = localKey.OpenSubKey(regPath);
        if (esriKey == null)
        {
          localKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
          esriKey = localKey.OpenSubKey(regPath);
        }
        if (esriKey == null)
        {
          //this is an error
          throw new System.InvalidOperationException(err1);
        }
        path = esriKey.GetValue("InstallDir") as string;
        //this is an error
        if (path == null || path == string.Empty)
          throw new InvalidOperationException(err1);
        version = esriKey.GetValue("Version") as string;
        //this is an error
        if (version == null || version == string.Empty)
          throw new InvalidOperationException(err2);
        buildNo = esriKey.GetValue("BuildNumber") as string;
        //this is an error
        if (buildNo == null || buildNo == string.Empty)
          throw new InvalidOperationException(err3);
      }
      catch (Exception ex)
      {
        throw new Exception(err1, ex);
      }
      return (path, version, buildNo);
    }
  }
}
