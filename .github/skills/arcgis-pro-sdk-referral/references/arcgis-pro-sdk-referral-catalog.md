<!--
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
-->
```
license: Apache-2.0
metadata:
  author: Esri, Inc.
  version: "1.0.0"
```
# ArcGIS Pro SDK Help Referral Resources

## Scope

This is the authoritative internal catalog of categories, sub-categories, descriptions, and URLs relevant to the ArcGIS Pro SDK. The canonical
normalization, matching, scoring, fallback, and output rules are maintained in
[ArcGIS Pro SDK Referral Logic](./arcgis-pro-sdk-referral-logic.md).

## Routing and Source Selection

- Keep pattern-purpose explanations, comparisons, limitations, and workflow-to-pattern recommendations with pattern help. Use the internal pattern
summary, decision guide, and relevant detail document before consulting external resources.
- Route general ArcGIS Pro SDK API and feature-domain questions to referral, including questions about mapping, editing, geodatabase, geometry,
layouts, raster, portal, networks, tasks, and similar domains.
- Route explicit requests for external ArcGIS Pro SDK resources to referral.
- Route explicit requests to create a new Add-in, Configuration, Plugin Datasource, or CoreHost project to the project creator rather than referral.
- Use the version-matched API reference as the primary authority for API availability, signatures, thread affinity, and supported behavior.
- Use official conceptual documentation to explain architecture and workflows. Treat snippets and community samples as implementation starting points
that must be checked against the target ArcGIS Pro and .NET versions.
- Return the narrowest category and sub-category that satisfy the request; do not include every general resource when focused links are available.

## Categories

### Overview
Description: High-level information about the Pro SDK, including supported IDEs, .NET versions, available extension assemblies, What's New, ArcGIS Pro SDK NuGet support, installation, registry keys, Add-in and Configuration loading, assembly references, obfuscation, EULAs, build-server configuration, signing and digital signatures, localization, embedded content and geoprocessing toolboxes, compile and runtime diagnostics, regression testing, security settings, developer resources, Copilot usage, compatibility, and Add-in versioning.

URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki#overview-of-the-arcgis-pro-sdk
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Installation-and-Upgrade
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Advanced-Topics
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-How-to-increment-addin-version-number
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#side-by-side-loading-to-resolve-addin-dependencies
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#display-end-user-license-agreement-eula-with-addin-installation

Sub-categories:

#### Installation
Description: Detailed instructions on how to download and install the ArcGIS Pro SDK.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki#installing-arcgis-pro-sdk-for-net

#### Developer Resources
Description: API reference, videos, Dev Summit tech sessions, snippets, and samples.
URLs:
- https://mediaspace.esri.com/channel/ArcGIS+Pro+SDK+for+.NET/256320933
- https://github.com/esri/arcgis-pro-sdk/wiki/tech-sessions
- https://github.com/Esri/arcgis-pro-sdk-community-samples
- https://github.com/Esri/arcgis-pro-sdk-snippets
- https://doc.esri.com/en/arcgis-pro/latest/sdk/api-reference/

#### Developer Copilot Resources
Description: How to use Copilot with the ArcGIS Pro SDK.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-develop-addins-with-copilot
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-writing-an-addin-with-copilot

#### Migration and .NET Upgrade
Description: Information regarding migrating SDK custom code between .NET versions and ArcGIS Pro versions.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-3.0-Migration-Guide
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-NET-8-Upgrade
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-NET-10-Upgrade

#### NuGet Support
Description: Information regarding ArcGIS Pro SDK NuGet support.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-ArcGIS-Pro-Extensions-NuGet
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Advanced-Topics#method-1-using-arcgis-pro-extensions-nuget

#### Registry Keys, Command-Line Options, and Security Settings
Description: Registry keys for Add-ins and Configurations, security levels, and command-line options.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ArcGIS-Pro-Registry-Keys
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Command-line-switches-for-ArcGISPro.exe

#### Debug and Regression Testing
Description: Compilation issues, debugging, common issues, and regression testing.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Diagnosing-ArcGIS-Pro-Add-ins
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Regression-Testing
- https://github.com/esri/arcgis-pro-sdk/wiki/FAQ
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#diagnostic-mode-event-logging

#### Embedding Content
Description: Embedding toolboxes, images, documents, or other custom content within an add-in or configuration.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-content-and-image-resources
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-content-and-image-resources#embedding-toolboxes

#### AllSource
Description: Support for ArcGIS Pro AllSource.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-ArcGIS-AllSource-Project-Template

#### Localization
Description: Localization.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcept-Localization

#### Signing, Digital Certificates, and Digital Signatures
Description: Signing Add-ins and Configurations, including HSM, cloud HSM, and certificate workflows.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Digitally-signed-add-ins-and-configurations

### Patterns
Description: Add-ins, configurations, plugin datasources, CoreHost, ArcGIS Pro UI extensibility points, standalone applications, styles/colors, DAML and declarative UI.

URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Configurations
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Configurations
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Plugin-Datasources
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Plugin-Datasources
- https://github.com/esri/arcgis-pro-sdk/wiki/proconcepts-CoreHost

Sub-categories:

#### Add-in and Configuration Extensibility Points
Description: Extending ArcGIS Pro UI/ribbon/dockpanes/panes for add-ins or configurations, including drag-drop and undo-redo.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Buttons
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Label-Controls
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Checkboxes
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Edit-Boxes
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Combo-boxes
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Context-Menus
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Palettes-and-Split-Buttons
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Galleries
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Dockpanes
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Command-Filter
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Custom-toolbars-for-view-panes
- https://github.com/esri/arcgis-pro-sdk/wiki/ProSnippets-CustomPaneWithContents
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Editing-Tool
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Sketch-Tool-With-Halo
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Construction-Tools-with-Options
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Knowledge-Graph-Construction-Tools
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Feature-Selection
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Identify
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Tray-buttons
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-MapView-Interaction
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Using-Embeddable-Controls
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Custom-Pop-ups
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Dynamic-Pop-up-Menu
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Keyboard-shortcuts
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#drag-and-drop
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#undoredo-framework
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#check-in-and-check-out-licences
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#quick-access-toolbar-qat
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#notifications

#### DAML and Declarative UI
Description: DAML, Config.daml, declarative UI, tabs, groups, conditions, states.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#introduction-to-daml-desktop-application-markup-language
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Ribbon-Tabs-and-Groups
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Code-Your-Own-States-and-Conditions

#### Styles and Styling
Description: Available ArcGIS Pro WPF styles, brushes, and colors for controls and text.
URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/proguide-style-guide
- https://github.com/esri/arcgis-pro-sdk/wiki/proguide-applying-custom-styles
- https://esri.github.io/arcgis-pro-sdk/content/brushescolors/brushes.html
- https://esri.github.io/arcgis-pro-sdk/content/brushescolors/colors.html

### API and Feature Functionality
Description: Available APIs, feature functionality, capabilities, namespaces, classes, functions, and methods.

URLs:
- https://doc.esri.com/en/arcgis-pro/latest/sdk/api-reference/

Sub-categories:

#### Concepts and Guides
Description: Tutorials, white papers, and reference documentation for functional ArcGIS Pro SDK API areas.
URLs:
- https://doc.esri.com/en/arcgis-pro/latest/sdk/api-reference/
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Arcade
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Content-and-Items
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Custom-Items
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Custom-Browse-Dialog-Filters
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Editing
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Templates
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-COGO
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Geodatabase
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-DDL
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-3D-Analyst-Data
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Topology
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Linear-Referencing
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Geometry
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Multipatches
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Relational-Operations
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Custom-Relational-Operations
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Geoprocessing
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Knowledge-Graph
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Layouts
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Reports
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Presentations
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Map-Authoring
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Annotation
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-3D-Analyst-Layers
- https://github.com/esri/arcgis-pro-sdk/wiki/CIM-Resources
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-GraphicsLayers
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Scene-Layers
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Stream-Layers
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Voxel-Layers
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Map-Exploration
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Map-Pane-Impersonation
- https://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-TableControl
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Utility-Network
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Telecom-Domain-Network
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Network-Diagrams
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Parcel-Fabric
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Raster
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Portal
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Tasks
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Workflow-Manager

### QueuedTask and Threading
Description: Anything related to QueuedTask, asynchronous programming, and multithreading.

URLs:
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Asynchronous-Programming-in-ArcGIS-Pro
- https://github.com/esri/arcgis-pro-sdk/wiki/ProConcepts-Framework#working-with-multithreading-in-arcgis-pro

### General
Description: Catch-all for any question that cannot be matched to a category or sub-category.

URLs:
- https://doc.esri.com/en/arcgis-pro/latest/sdk/api-reference/
