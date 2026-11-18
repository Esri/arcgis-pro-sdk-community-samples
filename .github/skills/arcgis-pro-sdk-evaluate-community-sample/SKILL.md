---
name: arcgis-pro-sdk-evaluate-community-sample
description: Evaluate one or more ArcGIS Pro SDK community sample projects for adherence to ArcGIS Pro SDK Best Practices and ArcGIS Pro SDK Community Samples Guidance, then report findings without changing code.
argument-hint: Provide one project path, multiple project paths, a folder, or request all community sample projects in the workspace.
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

# ArcGIS Pro SDK Evaluate Community Sample

## Purpose

Use this skill to assess one or more existing ArcGIS Pro SDK community sample projects against:
- [ArcGIS Pro SDK Best Practices](.github/instructions/arcgis-pro-sdk-best-practices.instructions.md)
- [ArcGIS Pro SDK Community Samples Guidance](.github/instructions/arcgis-pro-sdk-community-samples.instructions.md)
- Visual Studio Code configuration coverage for each C# project

This skill is evaluation-only. Report findings and evidence. Do not modify source files.

## Workflow

1. Resolve evaluation scope and target projects using [Scope and Target Selection](./references/01-scope-and-target-selection.md).
2. Evaluate each target project against both instruction sources and the Visual Studio Code configuration checks using [Evaluation Checklist](./references/02-evaluation-checklist.md).
3. Return a structured compliance report for each project using [Report Format](./references/03-report-format.md).

## Mandatory Constraints

- Never edit, format, move, rename, or delete files.
- Never apply automated fixes.
- Exclude any file with a `.md` extension from evaluation scope and findings.
- Treat missing or invalid Visual Studio Code configurations as findings; do not create or modify configuration files during an evaluation.
- If a rule cannot be evaluated from available files, mark it `Not Verifiable` and explain exactly what is missing.
- Prefer precise evidence with file paths and line references for every non-compliant finding.
