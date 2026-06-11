LabelVerify AI
AI-Assisted Alcohol Label Verification Prototype for TTB Compliance Review
Live Application

Deployed Application URL:

https://labelverify-bcole-fffufzamd8a4bzbv.eastus-01.azurewebsites.net/

Source Repository

GitHub Repository:

https://github.com/BColeOnline2025/LabelVerify-AI

Overview

LabelVerify AI is a proof-of-concept application designed to assist Alcohol and Tobacco Tax and Trade Bureau (TTB) compliance agents in reviewing alcohol beverage labels against application data.

The prototype demonstrates how artificial intelligence, OCR (Optical Character Recognition), and automated validation can reduce the amount of manual verification required during the Certificate of Label Approval (COLA) review process.

Rather than replacing compliance agents, the application is designed to augment the review process by automating routine field comparisons and highlighting discrepancies that require human judgment.

Business Problem

The TTB reviews approximately 150,000 alcohol beverage label applications annually.

Current reviews involve significant manual verification, including:

Brand name matching
Alcohol content validation
Net contents verification
Class/type verification
Government warning verification

Stakeholder interviews identified several challenges:

Manual Verification Burden

Agents spend substantial time comparing values on applications with values displayed on label artwork.

Batch Processing Limitations

Large importers may submit hundreds of labels simultaneously, requiring individual review.

Performance Requirements

Previous automation efforts failed due to long processing times. Stakeholders indicated that results must be returned in approximately five seconds or less.

User Experience Requirements

The solution must remain simple and intuitive for users with varying technical proficiency.

Real-World Variations

Labels frequently contain formatting differences that may not represent actual compliance issues.

Examples:

STONE'S THROW
Stone's Throw

OLD TOM DISTILLERY
OLD TOM DISTILLERY LLC

These cases require intelligent matching rather than strict string comparisons.

Solution Overview

The application provides two primary workflows:

Single Label Review

Allows an agent to upload an individual label image and compare extracted label content against application data.

Features:

Image upload
OCR text extraction
Automated verification
Confidence scoring
Pass / Review / Fail recommendations
Batch Review

Allows multiple labels to be processed simultaneously.

Features:

Multi-file upload
Batch processing dashboard
Summary metrics
Performance measurements
Aggregate review recommendations
Key Features
Automated Label Verification

The system validates:

Brand Name
Class / Type
Alcohol Content
Net Contents
Government Warning Statement
Fuzzy Brand Matching

The prototype uses fuzzy string matching to identify likely matches despite formatting differences.

Examples:

OLD TOM DISTILLERY
OLD TOM DISTILLERY LLC
STONE'S THROW
Stone's Throw

This feature addresses concerns raised by experienced compliance agents regarding false mismatches.

Confidence Scoring

Each validation result includes a confidence score.

Example:

Brand Name
Confidence: 90%

Recommendation:
Review Recommended
Batch Processing

Supports multi-file uploads to address large importer submissions.

Performance Tracking

Measures processing time for each label.

Results include:

Processing Time
Average Processing Time
Under 5 Seconds Indicator

This directly addresses stakeholder performance requirements.

Human-in-the-Loop Design

The application intentionally preserves human review authority.

Possible outcomes:

Pass

High confidence match.

Review

Potential match requiring agent review.

Fail

Missing or inconsistent information requiring attention.

Architecture
High-Level Architecture
Compliance Agent
        │
        ▼
Razor Pages UI
        │
        ▼
Verification Engine
        │
 ┌──────┴──────┐
 ▼             ▼
OCR Service   Rules Engine
 ▼             ▼
OCR Text      Validation Rules
        │
        ▼
Results Dashboard
Technology Stack
Front-End
ASP.NET Core Razor Pages
Bootstrap 5
Back-End
ASP.NET Core
C#
Dependency Injection
Validation
Custom Rules Engine
FuzzySharp
Cloud Platform
Microsoft Azure
Azure App Service
Future Integration
Azure AI Vision OCR
Azure Blob Storage
Azure Key Vault
Application Insights
Project Structure
LabelVerify.Web
│
├── Models
├── Services
│   ├── OCR
│   ├── Verification
│   └── Interfaces
│
├── Rules
│   ├── BrandNameRule
│   ├── ClassTypeRule
│   ├── AlcoholContentRule
│   ├── NetContentsRule
│   └── GovernmentWarningRule
│
├── ViewModels
│
├── Pages
│   ├── Index
│   └── Batch
│
└── wwwroot
Stakeholder Requirements Traceability
Stakeholder Requirement	Implementation
Reduce manual verification	Automated rules engine
Support batch processing	Multi-file batch upload
Results within five seconds	Performance tracking dashboard
Simple user interface	Razor Pages + Bootstrap
Handle minor label variations	Fuzzy brand matching
Human oversight retained	Pass / Review / Fail workflow
Running Locally
Prerequisites
.NET SDK
Visual Studio 2026 or later
Clone Repository
git clone https://github.com/YOUR-GITHUB-USERNAME/LabelVerify-AI.git
Run Application
cd LabelVerify.Web

dotnet restore

dotnet run

Application URL:

https://localhost:xxxx
Deployment

The application is currently deployed to Azure App Service.

Deployment platform:

Azure App Service
GitHub Repository
GitHub Actions CI/CD

Deployment URL:

https://labelverify-bcole-fffufzamd8a4bzbv.eastus-01.azurewebsites.net/

Assumptions

For prototype purposes:

OCR is currently abstracted behind an OCR service interface.
Mock OCR is used for predictable local testing.
Label images are not retained after processing.
Authentication and authorization are outside the scope of this prototype.
Regulatory review authority remains with compliance agents.
Future Enhancements
Azure AI Vision Integration

Replace Mock OCR with Azure AI Vision OCR.

Azure Blob Storage

Store uploaded label images for processing and auditing.

Government Warning Validation

Validate exact wording, formatting, and capitalization.

Advanced Label Analysis

Support:

Rotated labels
Glare correction
Perspective correction
Application Integration

Potential future integration with the TTB COLA system.

Machine Learning Review Assistance

Identify common compliance issues and recommend corrective actions.

Conclusion

LabelVerify AI demonstrates how AI-assisted verification can reduce manual compliance review effort while preserving human oversight and regulatory judgment.

The prototype addresses key stakeholder concerns including performance, usability, batch processing, and intelligent matching while providing a scalable foundation for future modernization efforts within the TTB label review process.