---
description: "Best practices when authoring or modifying ArcGIS Pro SDK for .NET community samples prjects"
applyTo: "**/*.cs,**/*.csproj,**/*.xaml,**/*.daml"
license: Apache-2.0
metadata:
  author: Esri, Inc.
  version: "1.0.0"
---

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

# ArcGIS Pro SDK Community Samples Guidance

Use these rules and guidance when authoring or modifying related C# code in ArcGIS Pro SDK projects that are meant to be shared as community samples.

## Sample project and documentation requirements

- Ensure proper project structure: when adding a new sample, keep the project file and the solution file in the same folder.
- Ensure API help file generation is enabled: to generate required sample documentation (generated into a `Readme.md` for each project), enable the `XML Documentation file` option in the `.csproj` file build output settings.
- Ensure example documentation exists: documentation is extracted from the `<summary>` and `<remarks>` triple-slash comments on the derived `Module` class. For CoreHost apps, use comments on the `Program` class. For `PluginDatasource` projects, use comments on the class implementing `PluginDatasourceTemplate`.
- Ensure screenshot links are correct: if `<remarks>` contains markdown links to screenshots, verify relative links exist in the project. These links are case-sensitive.



