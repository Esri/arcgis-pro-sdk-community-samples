## Telecom Domain Network

This console application demonstrates how to use the ArcGIS Pro SDK for .NET to manage circuits and equipment in a telecom domain network from a standalone CoreHost application.

The sample creates an optical line terminal (OLT), an optical network terminal (ONT), a splitter, ports, fiber cables, and fiber strands. It establishes containment and connectivity associations, validates the network topology, and creates and verifies physical circuits.

In addition, the sample demonstrates the equipment management by querying and reserving unit identifiers on a splitter and resizing an ONT port group.

<a href="https://pro.arcgis.com/en/pro-app/sdk/" target="_blank">View it live</a>

```
Language:              C#
Subject:               UtilityNetwork
Contributor:           ArcGIS Pro SDK Team <arcgisprosdk@esri.com>
Organization:          Esri, https://www.esri.com
Date:                  10/19/2026
ArcGIS Pro:            3.8
Visual Studio:         2026, VS Code
Target Framework:      net10.0-windows
```

## Resources

[Community Sample Resources](https://github.com/Esri/arcgis-pro-sdk-community-samples#resources)

### Samples Data

This sample utilizes the sample telecom utility network data provided provided in the [ArcGIS Pro SDK community samples releases][here](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases).  

## How to use the sample

1. Install and license ArcGIS Pro 3.8 and install the .NET 10 SDK. Use Visual Studio 2026 or VS Code with C# support to build the project.

1. Get the telecom utility network data from the [ArcGIS Pro SDK community samples releases](https://github.com/Esri/arcgis-pro-sdk-community-samples/releases) and place the working copy at **C:\Data\TelecomUtilityNetwork.geodatabase**.

1. Open `CircuitManagement.slnx` in Visual Studio, or open `CircuitManagement.csproj` in your development environment and build the solution and run the `CircuitManagement` console application.

1. The console reports the ObjectIDs of the devices, ports, cables, and fiber strands as they are created. The application then creates associations, validates the network topology, creates and verifies two physical circuits, reserves splitter units, and resizes the ONT port group.

1. To inspect the persisted results, open ArcGIS Pro after the application finishes and add the utility network from the working mobile geodatabase to a map.

### Circuit management

The sample opens the utility network in the mobile geodatabase and obtains the telecom domain network named `Telco`. It reads the domain network's circuit properties, wavelength schemes, color schemes, color sets, and combine and divide policies.

It creates the following network:

- An OLT with port units 1 through 8.
- A splitter with port group A covering units 1 through 8 and port group B covering units 9 through 12. A junction-junction connectivity association connects the two port groups.
- An ONT with port units 1 through 4.
- A fiber cable containing strand units 1 through 8 between the OLT and splitter, and a second cable containing strand units 1 through 4 between the splitter and ONT.

Containment associations place the ports inside their devices and the fiber strands inside their cables. Junction-edge object connectivity associations connect the ports to the fiber strands.

![OLT connected to an ONT through a splitter, showing port groups, fiber unit ranges, and junction-junction connectivity](Screenshots/Screenshot1.PNG)

After validating the network topology, the sample uses `CircuitManager` to create two physical circuits:

- `OLT_ONT_1_4_SectionedCircuit`: a sectioned circuit from OLT units 1 through 4 to the ONT through the splitter, with two sections connected in series.
- `OLT_Splitter_5_8_Circuit`: a nonsectioned circuit from OLT units 5 through 8 to splitter port group A units 5 through 8. The sample also sets a user-defined circuit attribute.

Both circuits are verified with `ForceVerify` and `SynthesizeGeometries` enabled.

### Equipment management

The sample uses `UnitIdentifierManager` to query the splitter's unit identifiers, reserve units 15 through 20 on the splitter, and query the updated unit information. It also resizes the ONT port group by changing its last unit from 4 to 8.

The following diagram highlights the ONT port group after resizing. The fiber strand ranges remain unchanged.

![ONT port group resized from last unit 4 to last unit 8, with the existing fiber strand ranges unchanged](Screenshots/Screenshot2.PNG)

The final diagram includes an additional splitter position labeled C to illustrate the equipment reservation scenario. The code reserves unit identifiers 15 through 20 on the splitter; it does not create an additional port object at C.

![Equipment management schematic showing splitter position C and the resized ONT port group](Screenshots/Screenshot3.PNG)

### Using other data

The sample is configured for the supplied mobile geodatabase. It expects a `Telco` telecom domain network, the `TelcoDevice`, `TelcoLine`, `TelcoJunctionObject`, and `TelcoEdgeObject` datasets, and the asset groups, asset types, fields, association rules, and circuit configuration used in `CircuitManagement.cs`.

To use another dataset, update the hard-coded schema names, unit ranges, color scheme, circuit attributes, and geometries as needed. Changing only the geodatabase path is not sufficient unless the replacement data has the same configuration.

<!-- End -->

&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="https://esri.github.io/arcgis-pro-sdk/images/ArcGISPro.png" alt="ArcGIS Pro SDK for .NET" height="20" width="20" align="top">
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
[Home](https://github.com/Esri/arcgis-pro-sdk/wiki) | <a href="https://pro.arcgis.com/en/pro-app/latest/sdk/api-reference" target="_blank">API Reference</a> | [Requirements](https://github.com/Esri/arcgis-pro-sdk/wiki#requirements) | [Download](https://github.com/Esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net) | <a href="https://github.com/esri/arcgis-pro-sdk-community-samples" target="_blank">Samples</a>
