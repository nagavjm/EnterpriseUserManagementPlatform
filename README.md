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

## 4. Development Roadmap

- [x] **Phase 1: Solution Setup** - Scaffolding Clean Architecture projects, Angular SPA, launch profiles, and build verification.
- [ ] **Phase 2: Backend Development** - Domain entity, DTOs, JSON persistence engine, services, REST controllers, health checks.
- [ ] **Phase 3: Frontend Development** - Dashboard, user tables, search/filters, reactive forms, status toggles.
- [ ] **Phase 4: Testing & Quality** - Unit tests & ASP.NET Core integration tests.
- [ ] **Phase 5: Docker Containerization** - Multi-stage Dockerfiles & Docker Compose with persistent volume mount.
- [ ] **Phase 6: Kubernetes Deployment** - Manifests, PVC, ConfigMap, Deployment, Service, and Ingress.
- [ ] **Phase 7: CI/CD** - Automated GitHub Actions workflow.
