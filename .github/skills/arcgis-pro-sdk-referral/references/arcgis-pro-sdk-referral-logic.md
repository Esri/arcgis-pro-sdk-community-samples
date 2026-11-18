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
# ArcGIS Pro SDK Referral Logic

## Objective

Determine the most relevant ArcGIS Pro SDK documentation category, sub-category, and authoritative links for a referral request.

Canonical ownership:
- This document is the authoritative source for referral-time logic used by the ArcGIS Pro SDK referral flow.
- Do not duplicate or redefine this logic in the referral agent file or in the parent SKILL file.

## Required Grounding Source

- You must read and use [ArcGIS Pro SDK Referral Catalog](./arcgis-pro-sdk-referral-catalog.md) for every referral decision.

## Decision Sequence

### Step 1. Normalize user question
- lowercase
- remove punctuation
- extract key terms and noun phrases

### Step 2. Match by category and sub-category description
- prioritize sub-category matches over top-level category matches
- prefer direct term matches in description text

### Step 3. Refine matches using category and sub-category URL slugs
- prefer URL slugs that contain key terms from the user question

### Step 4. Score matches
- Score each normalized key term or noun phrase against each candidate category or sub-category independently.
- For each term-to-candidate comparison, apply exactly one mutually exclusive score: use the highest applicable rule and do not add lower-scoring rules for the same match.
    - +4 for a direct keyword or phrase match in the sub-category description
    - +2 for a direct keyword or phrase match in the category description
    - +2 if a candidate URL slug/path directly contains the keyword or phrase
    - +1 for a semantically adjacent term
    - 0 for no match
- If a term matches both a category and one of its sub-categories, apply only the sub-category score for that term.
- Repeated occurrences of the same normalized term do not increase the score.
- Sum the mutually exclusive scores for distinct normalized terms to obtain each candidate's total score. The candidate with the highest total score wins.

### Step 5. Select target
- choose highest-scoring sub-category when present
- otherwise choose highest-scoring top-level category
- For tied total scores, choose the candidate matched by the greatest number of distinct normalized terms.
- If still tied, choose the candidate whose URL slug/path contains the greatest number of distinct normalized terms.
- If still tied, choose the candidate that appears first in the referral catalog.

Special-case selection rules:
- If the user asks a broad inventory question about available feature functionality, capabilities, or what is available in the SDK, force-select category `API and Feature Functionality` and sub-category `Concepts and Guides`.
- Apply the same forced selection for prompts like: "what feature functionality is available", "what APIs are available", "what can the SDK do", and equivalent broad capability questions.

### Step 6. URL applicability check
- prefer URL(s) whose path/title tokens best match question terms
- if a question asks for one narrow topic, return 1 to 3 most specific links
- if broad, return 3 to 6 links across the matched category/sub-category
- if the selected target is `API and Feature Functionality` -> `Concepts and Guides` for a broad inventory question:
    - always return the first two URLs listed for `Concepts and Guides`, preserving catalog order
    - score every remaining `Concepts and Guides` URL against the normalized key terms and noun phrases by applying the Step 4 hierarchy to the URL slug/path; each term-to-URL comparison receives exactly one mutually exclusive score
    - sum the distinct term scores for each URL, rank URLs by descending total score, and use catalog order to break ties
    - append the top 2 additional URLs when only two have a positive score; append the top 3 when three or more have a positive score
    - if fewer than two remaining URLs have a positive score, append URLs in catalog order until two additional URLs have been selected
    - return no more than 5 URLs total for this special case
- if the selected category is `API and Feature Functionality` (any sub-category), ALWAYS include category url `https://doc.esri.com/en/arcgis-pro/latest/sdk/api-reference/` FIRST in `## Recommended Authoritative Links`

### Step 7. Fallback
- A request has no match when every candidate's final score is `0`.
- If the highest candidate score is `0`, route to category `General` with sub-category `None`.
- Do not use the `General` fallback when any candidate has a positive score.

## Output Rules

- Always provide selected category and sub-category names.
- Category and sub-category values must be plain text names, not hyperlinks.
- Place URLs only under `## Recommended Authoritative Links`.
- Return inline markdown and do not wrap the full response in a fenced code block.

## Output Contract

Return exactly:

# Referral Category: <Category Name>

## Referral Sub-category
<Sub-category Name or `None`>

## Why This Match
<2 to 5 concise sentences>

## Recommended Authoritative Links
- <URL 1>
- <URL 2>
- ...
- <URL N>

## Constraints

- Do not provide implementation code.
- Do not invent links.
- Do not omit category/sub-category names.
- Use `General` only when the highest candidate score is `0`.