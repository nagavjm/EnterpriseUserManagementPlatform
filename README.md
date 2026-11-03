# Enterprise User Management Platform (EUMP)

An enterprise-grade User Management Platform designed for hands-on learning of **.NET 9**, **Angular 19**, **Clean Architecture**, **Docker**, **Kubernetes**, and **CI/CD**.

---

## 1. Architecture Overview

EUMP is designed following **Clean Architecture** and **SOLID principles**:

- **Domain (`EnterpriseUserManagement.Domain`)**: Core enterprise entities (`User`), enums (`Department`, `Role`), and business rules. Completely decoupled from external frameworks.
- **Application (`EnterpriseUserManagement.Application`)**: DTOs, business contracts, input validation, and repository abstractions (`IUserRepository`).
- **Infrastructure (`EnterpriseUserManagement.Infrastructure`)**: Persistent JSON file storage engine utilizing `System.Text.Json`, `SemaphoreSlim` async write synchronization, safe temporary-file atomic replacement, and configurable data paths for Docker volumes and Kubernetes PVCs.
- **API (`EnterpriseUserManagement.Api`)**: ASP.NET Core 9 Web API presentation layer with thin controllers, Swagger/OpenAPI documentation, health probes (`/health`, `/health/ready`), and global exception handling.
- **Frontend (`enterprise-user-management-ui`)**: Angular 19 SPA with standalone components, reactive forms, routing, and centralized REST client services.

---

## 2. Technology Stack

- **Backend**: .NET 9 Web API, C#, System.Text.Json, Swagger / OpenAPI
- **Frontend**: Angular 19, TypeScript, RxJS, Reactive Forms
- **Storage**: JSON file-based persistence (`users.json`)
- **Containers & Orchestration**: Docker, Docker Compose, Kubernetes

---

## 3. Getting Started

### Prerequisites
- .NET 9 SDK
- Node.js (v20+) and npm
- Angular CLI 19+

### Running Locally

#### Option A: One-Click Development Script
Run the automated PowerShell launcher from the project root:
```powershell
.\run-dev.ps1
```
This automatically starts:
- **Backend API**: `http://localhost:5100` (Swagger: `http://localhost:5100/swagger`)
- **Angular Frontend**: `http://localhost:4200`

#### Option B: VS Code Debugger
Open the project in VS Code, press `F5`, and choose **Full Stack: Backend + Frontend**.

#### Option C: Manual Terminal Launch

**Backend:**
```powershell
cd backend/src/EnterpriseUserManagement.Api
dotnet run --launch-profile http
```

**Frontend:**
```powershell
cd frontend/enterprise-user-management-ui
npm start
```

---

## 4. Azure Kubernetes Service (AKS) Deployment

EUMP includes production deployment support for **Azure Kubernetes Service (AKS)** and **Azure Container Registry (ACR)**:

1. **Detailed Azure Portal Guide**: See [docs/aks-deployment-guide.md](docs/aks-deployment-guide.md) for step-by-step instructions on creating Resource Groups, ACR, and AKS clusters.
2. **Automated CI/CD Deployment**: Deployed continuously via Azure DevOps pipelines (`azure-pipelines.yml`).
3. **Manual CLI Deployment**:
```powershell
az aks get-credentials --resource-group "rg-eump" --name "aks-eump"
kubectl apply -f ./kubernetes/
```

---

## 5. Azure DevOps CI/CD Pipeline

EUMP includes an **industry-standard multi-stage Azure DevOps pipeline** (`azure-pipelines.yml`):

- **Automated CI Validation**: Runs parallel .NET 9 unit/integration tests with Cobertura code coverage and Angular 19 compilation checks.
- **Immutable Container Builds**: Multi-stage Docker builds tagged with `$(Build.BuildId)` and pushed to Azure Container Registry (ACR).
- **Automated CD to AKS**: Deploys to AKS using Azure DevOps Environments (`eump-production`) with automated rollout validation and optional approval gates.
- **Detailed Setup Guide**: See [docs/azure-devops-cicd-guide.md](docs/azure-devops-cicd-guide.md) for step-by-step instructions on setting up service connections, approvals, and branch policies.

---

## 6. Development Roadmap

- [x] **Phase 1: Solution Setup** - Scaffolding Clean Architecture projects, Angular SPA, launch profiles, and build verification.
- [x] **Phase 2: Backend Development** - Domain entity, DTOs, JSON persistence engine, services, REST controllers, health checks.
- [x] **Phase 3: Frontend Development** - Dashboard, user tables, search/filters, reactive forms, status toggles.
- [x] **Phase 4: Testing & Quality** - Unit tests & ASP.NET Core integration tests.
- [x] **Phase 5: Docker Containerization** - Multi-stage Dockerfiles & Docker Compose with persistent volume mount.
- [x] **Phase 6: Kubernetes & AKS Deployment** - Manifests, PVC, ConfigMap, Deployment, Service, Ingress, and AKS security context.
- [x] **Phase 7: CI/CD** - Automated Azure DevOps multi-stage pipeline with test reporting and AKS deployment.

