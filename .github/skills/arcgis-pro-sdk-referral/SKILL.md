---
name: arcgis-pro-sdk-referral
description: Resolve referrals from `arcgis-pro-sdk-pattern-help` by selecting the most relevant ArcGIS Pro SDK documentation category, sub-category, and authoritative links using the canonical referral logic.
user-invocable: false
disable-model-invocation: false
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
# ArcGIS Pro SDK Referral Skill

## Purpose

Use this skill to resolve ArcGIS Pro SDK referral requests by mapping a user question to the most relevant documentation category, sub-category, and authoritative links. This skill supports the pattern-help agent flow when a help request needs to be satisfied by authoritative link referral rather than a direct explanatory answer.

Resolution authority:
- Do not define referral-selection logic in this SKILL file.
- The canonical source for referral logic is:
  - [ArcGIS Pro SDK Referral Logic](./references/arcgis-pro-sdk-referral-logic.md)

## Workflow

1. Load and follow [ArcGIS Pro SDK Referral Logic](./references/arcgis-pro-sdk-referral-logic.md) to determine normalization, matching, scoring, category/sub-category selection, URL selection, fallback behavior, and output formatting.
2. Return exactly the output contract defined by that canonical reference.