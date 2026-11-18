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

# Scope and Target Selection

## Objective

Identify exactly which community sample project(s) will be evaluated and avoid implicit scope expansion.

## Resolution Rules

1. If the user provides explicit `.csproj` paths, evaluate only those projects.
2. If the user provides a folder, evaluate all `.csproj` projects under that folder, excluding `bin`, `obj`, generated asset folders, and any `.md` files.
3. If the user requests all community samples, evaluate all sample `.csproj` projects in the workspace, excluding non-sample infrastructure, generated assets, and any `.md` files.
4. Markdown files are never treated as evaluation targets or standalone findings.
5. If project intent is ambiguous, ask a focused clarification question before evaluating.

## Project Type Detection

For each target, classify the project as one of:
- Add-in
- Managed Configuration
- CoreHost
- Plugin Datasource
- Unknown (insufficient indicators)

Use project and manifest indicators (for example `Config.daml`, `Config.xml`, project properties, startup/profile metadata, ArcGIS references).

## Output for this Step

Return a short scope summary before findings:
- Target count
- Target project list
- Type classification per project
- Any exclusions and reason
