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

# Evaluation Checklist

## Objective

Evaluate each selected project for conformance to both canonical guidance sources:
- `../../instructions/arcgis-pro-sdk-best-practices.instructions.md`
- `../../instructions/arcgis-pro-sdk-community-samples.instructions.md`

## Assessment Method

For each rule, assign one status:
- `Compliant`
- `Non-compliant`
- `Not Verifiable`
- `Not Applicable`

Every `Non-compliant` and `Not Verifiable` result must include evidence.

## Community Samples Guidance Checks (Required)

1. **Project/solution co-location check**
   - Confirm the sample `.csproj` and corresponding `.sln` are in the same folder when evaluating newly added sample structures.
2. **XML documentation output enabled check**
   - Confirm XML documentation file generation is enabled in build output settings.
3. **Example documentation source check**
   - Confirm `<summary>` and `<remarks>` source comments exist on the expected class by project type:
     - Add-in / Managed Configuration: derived `Module` class.
     - CoreHost: `Program` class.
     - Plugin Datasource: class implementing `PluginDatasourceTemplate`.
4. **Screenshot markdown links check**
   - If `<remarks>` section for the 'Example documentation source checks' includes markdown image/screenshot links, confirm every relative link is resolved relative to the `.csproj` file folder, exists in the project, and respects case-sensitive paths.

## Best Practices Checks (Required)

Evaluate relevant best-practice categories for each project, including at minimum:
1. Thread affinity and `QueuedTask` usage correctness where ArcGIS Pro API requires MCT.
2. Async correctness (`await` flow, no UI-thread blocking with `.Result`/`.Wait`, cancellation/error handling patterns where applicable).
3. API and state-safety patterns (no invalid thread mutations to ArcGIS Pro state).
4. DAML/code synchronization for IDs, class references, captions/tooltips, and conditions where applicable.
5. Naming, responsibility boundaries, and exception quality for maintainability and safe usage.

If a category has no applicable code paths, mark `Not Applicable` with explanation.

## Visual Studio Code Configuration Checks (Required)

For every selected `.csproj`, inspect both the workspace-root `.vscode` folder and the project's own `.vscode` folder:

1. **Configuration-file presence check**
   - Confirm each folder contains `launch.json` and `tasks.json`.
2. **Project entry coverage check**
   - Confirm the root `launch.json` and `tasks.json` each have an entry for the selected project.
   - Confirm the project's `launch.json` and `tasks.json` each have an entry for that project. If multiple projects share a folder, require an entry for each `.csproj`.
   - For a request to evaluate all samples, inventory all workspace `.csproj` files and confirm root-level entries cover every project.
3. **Configuration integrity check**
   - Confirm both JSON files parse successfully.
   - Confirm build tasks identify the correct `.csproj` and each launch configuration's `preLaunchTask` resolves to an existing task label.
   - Confirm the launch target is consistent with the project type (for example, ArcGIS Pro for add-ins and the built application for executable CoreHost projects).

Missing files or entries, invalid JSON, incorrect project paths, or unresolved task references are `Non-compliant`. If a configuration cannot be inspected, mark it `Not Verifiable` and identify the missing file or information. Do not repair configurations during evaluation.

## Evidence Requirements

Use concrete evidence:
- file path
- line number or line range
- short quoted snippet or precise rule summary

Do not speculate. If evidence is missing, mark `Not Verifiable`.

For Visual Studio Code findings, cite the affected root-level or project-level `.vscode/launch.json` or `.vscode/tasks.json` path and the relevant line. For missing files or entries, cite the available project path and state which expected configuration artifact or project entry is absent.
