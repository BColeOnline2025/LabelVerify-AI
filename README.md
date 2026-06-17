# LabelVerify AI 

## AI-Assisted Alcohol Beverage Label Verification Platform

---

## Overview

LabelVerify AI is an AI-assisted alcohol beverage label verification platform designed to support reviewers in validating production labels against approved TTB COLA submissions.

The application combines deterministic compliance validation, OCR extraction, risk scoring, workflow management, and Azure OpenAI generated narratives to provide a first-pass compliance review experience.

The solution is intended to augment reviewer decision-making, reduce manual effort, identify material compliance issues, and improve consistency across reviews.

---

# Repository

**Source Code Repository**

https://github.com/BColeOnline2025/LabelVerify-AI

---

# Live Demonstration

**Deployed Application**

https://labelverify-bcole-fffufzamd8a4bzbv.eastus-01.azurewebsites.net

### Demo Credentials

Username

```text
rev1
```

Password

```text
Password123!
```

---

# Business Problem

TTB label reviewers manually compare approved COLA applications against production labels.

Common issues include:

* Incorrect alcohol content
* Net contents discrepancies
* Missing sulfites declarations
* Government Warning formatting defects
* Incomplete warning statements
* Reviewer workload management

LabelVerify AI attempts to reduce repetitive manual review while maintaining deterministic and explainable compliance decisions.

---

# Key Capabilities

### OCR Extraction

Extracts text from:

* PDF
* PNG
* JPG
* JPEG

using:

* Azure Document Intelligence
* Azure Vision OCR
* Mock OCR provider

---

### Compliance Validation

Deterministic validators include:

| Validator          | Capability                   |
| ------------------ | ---------------------------- |
| Government Warning | Exact wording                |
| Government Warning | Header capitalization        |
| Government Warning | Statement completeness       |
| Government Warning | Estimated visual prominence  |
| Alcohol Content    | Value comparison             |
| Alcohol Content    | Formatting validation        |
| Net Contents       | Equivalent volume comparison |
| Sulfites           | Required declaration         |
| Brand Name         | Fuzzy comparison             |
| Fanciful Name      | Comparison                   |
| Country of Origin  | Presence validation          |

---

# Government Warning Validation

The Government Warning validator is intentionally strict and models reviewer behavior.

Checks include:

### Header Format Validation

Required

```text
GOVERNMENT WARNING:
```

Rejects examples such as

```text
Government Warning:
```

```text
Government warning:
```

```text
Government Warning
```

---

### Exact Text Validation

Required statement

```text
GOVERNMENT WARNING:

(1) According to the Surgeon General,
women should not drink alcoholic beverages during
pregnancy because of the risk of birth defects.

(2) Consumption of alcoholic beverages impairs your ability
to drive a car or operate machinery, and may cause health
problems.
```

---

### Completeness Validation

Sentence 1

```text
According to the Surgeon General
```

Sentence 2

```text
Consumption of alcoholic beverages impairs your ability
```

Both statements must exist.

---

### Government Warning Prominence Validation

LabelVerify estimates visual prominence of the Government Warning header using Azure Document Intelligence layout polygons.

The OCR engine measures the height of the detected text bounding polygon.

Example

```text
Government Warning Prominence


Expected

Estimated prominence >= 0.10


Actual

0.117


Status

PASS
```

Example

```text
Actual

0.061


Status

REVIEW
```

This validation is intended as a reviewer assistance signal only.

It does not replace physical measurement of label typography.

---

# AI Features

Azure OpenAI provides

### AI Compliance Summary

Summarizes validation findings

---

### AI Risk Assessment

Explains operational and regulatory risks

---

### Compliance Insights

Provides reviewer-focused explanations

---

### Queue Recommendation

Suggests review prioritization

---

### Monthly Compliance Reports

Management-oriented summaries

---

# Risk Scoring

Reviews receive a calculated risk score.

Levels

Low

Medium

High

Examples of material findings

Government Warning defects

Alcohol discrepancies

Missing sulfites declarations

Net content mismatches

---

# Reviewer Productivity Features

## Dynamic Reviewer Notes

Reviewer note templates are generated automatically from failed validations.

Examples

Government Warning

Alcohol Content

Net Contents

Sulfites

Selecting a button inserts standardized reviewer language.

---

## Recommendation Override

Reviewers may override recommendations.

Examples

Pass

Review

Fail

Override reasons are preserved in the audit trail.

---

## Work Queue

Displays

Assigned Reviews

Batch Reviews

Completed Reviews

Pending Reviews

---

# Single Review Workflow

Step 1

Navigate to

Single Review

---

Step 2

Upload

Approved COLA Package

---

Step 3

Upload

Production Label Images

---

Step 4

Enable

Inline AI

---

Step 5

Submit

---

System performs

OCR Extraction

↓

Field Extraction

↓

Production Label Merge

↓

Compliance Validation

↓

Risk Assessment

↓

AI Summary Generation

↓

Persistence

---

Step 6

Review Results

Recommendation

Risk Score

Compliance Insights

AI Summary

Reviewer Decision

Field Validation Results

Audit Trail

---

# Batch Review Workflow

Step 1

Navigate to

Batch Review

---

Step 2

Upload

COLA Packages

ZIP Files

Production Labels

---

Step 3

Assign Reviewer

---

Step 4

Submit Batch

---

System creates

Review Batch

Review Sessions

Assignments

Audit Records

---

Step 5

Navigate to

My Work Queue

---

Step 6

Perform Review

Approve

Review

Reject

---

# PDF Export

The exported compliance report includes

Review Metadata

Risk Assessment

AI Risk Assessment

Compliance Insights

Critical Compliance Findings

Reviewer Notes

Field Validation Results

Source Documents

Audit Trail

---

# Technology Stack

| Component      | Technology                  |
| -------------- | --------------------------- |
| Framework      | ASP.NET Core 10 Razor Pages |
| Language       | C#                          |
| ORM            | Entity Framework Core       |
| Database       | Azure SQL                   |
| OCR            | Azure Document Intelligence |
| OCR Fallback   | Azure Vision                |
| AI             | Azure OpenAI                |
| Storage        | Azure Blob Storage          |
| Reporting      | QuestPDF                    |
| Authentication | ASP.NET Identity            |
| Hosting        | Azure App Service           |
| UI             | Bootstrap 5                 |

---

# Setup Instructions

## Prerequisites

Visual Studio 2026

.NET 10 SDK

Azure Subscription

Azure SQL Database

Azure Blob Storage

Azure OpenAI

Azure Document Intelligence

---

## Clone Repository

```bash
git clone https://github.com/BColeOnline2025/LabelVerify-AI.git

cd LabelVerify-AI
```

---

## Configure User Secrets

```json
{
  "AzureBlobStorage": {

    "ConnectionString": ""

  },

  "AzureDocumentIntelligence": {

    "Endpoint": "",

    "ApiKey": ""

  },

  "AzureOpenAi": {

    "Endpoint": "",

    "ApiKey": "",

    "DeploymentName": ""

  }
}
```

---

## Apply Migrations

```bash
dotnet ef database update
```

---

## Run

```bash
dotnet run
```

or

Press

F5

in Visual Studio

---

# Assumptions

Government Warning prominence estimation uses OCR bounding polygon heights.

Alcohol validation assumes accurate OCR extraction.

Net contents currently support

ML

CL

L

AI generated narratives are advisory only.

Final disposition decisions remain under reviewer control.

---

# Future Enhancements

Government Warning millimeter-based font size validation

Wine appellation validation

Varietal validation

Country of origin validation

Supervisor dashboard

SLA monitoring

TEFCA enabled ingestion

Automatic workload balancing

Unclear image validation

Processing time optimization

---

# Architectural Approach

Compliance decisions are driven by deterministic rules.

Artificial intelligence is used only to explain findings, summarize results, and assist reviewers.

At no point does AI independently approve or reject labels.

This approach was selected to maintain explainability, repeatability, and alignment with regulatory review expectations.

---

Author

Brian Cole

2026