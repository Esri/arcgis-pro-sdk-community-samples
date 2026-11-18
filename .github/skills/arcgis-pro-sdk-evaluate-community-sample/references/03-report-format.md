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

# Report Format

## Output Contract

Produce one report section per evaluated project.

### 1) Scope Summary
- Evaluated projects (count + paths)
- Excluded projects and reasons

### 2) Per-Project Result
- Project path
- Project type
- Overall status (`Compliant`, `Compliant with Notes`, `Non-compliant`, `Incomplete`)

### 3) Findings Table
For each finding include:
- Rule source (`Best Practices`, `Community Samples Guidance`, or `Visual Studio Code Configuration`)
- Rule/check name
- Status (`Compliant`, `Non-compliant`, `Not Verifiable`, `Not Applicable`)
- Evidence (path + line reference)
- Impact (short)

Include Visual Studio Code findings separately for workspace-root and project-level configuration coverage when relevant.

### 4) Required Follow-up
- List only actionable remediation items for `Non-compliant` findings.
- Keep recommendations minimal and specific.

### 5) Verification Gaps
- List all `Not Verifiable` checks with missing artifacts.

## Constraints

- Report findings only.
- Do not provide or apply patches.
- Do not change files.
