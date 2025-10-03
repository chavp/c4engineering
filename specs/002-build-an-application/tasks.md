# Tasks: C4Engineering Platform MVP with Backstage Integration

**Input**: Design documents from `C:\data\C4Engineering\specs\002-build-an-application\`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → Loaded: ASP.NET Core 8.0 + Bootstrap 5.3 + vanilla JavaScript + JSON storage ✓
   → Extract: tech stack, libraries, structure ✓
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks ✓
   → contracts/: 3 API files → contract test tasks ✓
   → research.md: Extract decisions → setup tasks ✓
3. Generate tasks by category:
   → Setup: project init, dependencies, linting ✓
   → Tests: contract tests, integration tests ✓
   → Core: models, services, controllers ✓
   → Integration: SignalR, Docker, logging ✓
   → Polish: unit tests, performance, docs ✓
4. Apply task rules:
   → Different files = mark [P] for parallel ✓
   → Same file = sequential (no [P]) ✓
   → Tests before implementation (TDD) ✓
5. Number tasks sequentially (T001, T002...) ✓
6. Generate dependency graph ✓
7. Create parallel execution examples ✓
8. Validate task completeness:
   → All contracts have tests? ✓
   → All entities have models? ✓
   → All endpoints implemented? ✓
9. Return: SUCCESS (tasks ready for execution) ✓
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Web application**: ASP.NET Core structure with `src/C4Engineering.Web/` and `tests/C4Engineering.Tests/`
- **Frontend**: `wwwroot/js/` for JavaScript modules, `wwwroot/css/` for styles
- **Data**: `wwwroot/data/` for JSON file storage

## Phase 3.1: Setup & Quality Foundation
- [ ] T001 Create ASP.NET Core project structure in `src/C4Engineering.Web/`
- [ ] T002 Initialize C# .NET 8 project with ASP.NET Core, SignalR, and Docker.DotNet dependencies
- [ ] T003 [P] Configure EditorConfig, code formatting, and C# analyzer rules
- [ ] T004 [P] Set up xUnit test project in `tests/C4Engineering.Tests/` with 90% coverage target
- [ ] T005 [P] Configure NBomber for performance testing and load testing capabilities
- [ ] T006 [P] Set up Playwright for end-to-end testing with browser automation
- [ ] T007 [P] Configure JSON data directory structure in `wwwroot/data/`
- [ ] T008 [P] Add Bootstrap 5.3 CSS and vanilla JavaScript module setup

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**

### Contract Tests
- [ ] T009 [P] Contract test Service Catalog API endpoints in `tests/C4Engineering.Tests/Contract/ServiceCatalogApiTests.cs`
- [ ] T010 [P] Contract test Diagram API endpoints in `tests/C4Engineering.Tests/Contract/DiagramApiTests.cs`
- [ ] T011 [P] Contract test Pipeline API endpoints in `tests/C4Engineering.Tests/Contract/PipelineApiTests.cs`

### Repository Tests
- [ ] T012 [P] Unit tests for JsonServiceRepository in `tests/C4Engineering.Tests/Repositories/JsonServiceRepositoryTests.cs`
- [ ] T013 [P] Unit tests for JsonDiagramRepository in `tests/C4Engineering.Tests/Repositories/JsonDiagramRepositoryTests.cs`
- [ ] T014 [P] Unit tests for JsonPipelineRepository in `tests/C4Engineering.Tests/Repositories/JsonPipelineRepositoryTests.cs`

### Service Layer Tests
- [ ] T015 [P] Unit tests for ServiceCatalogService in `tests/C4Engineering.Tests/Services/ServiceCatalogServiceTests.cs`
- [ ] T016 [P] Unit tests for DiagramService in `tests/C4Engineering.Tests/Services/DiagramServiceTests.cs`
- [ ] T017 [P] Unit tests for PipelineService in `tests/C4Engineering.Tests/Services/PipelineServiceTests.cs`
- [ ] T018 [P] Unit tests for DockerDeploymentService in `tests/C4Engineering.Tests/Services/DockerDeploymentServiceTests.cs`

### Integration Tests
- [ ] T019 [P] Integration test service catalog workflow in `tests/C4Engineering.Tests/Integration/ServiceCatalogIntegrationTests.cs`
- [ ] T020 [P] Integration test C4 diagram creation and editing in `tests/C4Engineering.Tests/Integration/DiagramIntegrationTests.cs`
- [ ] T021 [P] Integration test pipeline execution with Docker in `tests/C4Engineering.Tests/Integration/PipelineIntegrationTests.cs`
- [ ] T022 [P] Integration test SignalR real-time collaboration in `tests/C4Engineering.Tests/Integration/CollaborationIntegrationTests.cs`

## Phase 3.3: Core Implementation (ONLY after tests are failing)

### Domain Models
- [ ] T023 [P] Service domain models in `src/C4Engineering.Web/Models/ServiceCatalog/`
- [ ] T024 [P] Architecture domain models in `src/C4Engineering.Web/Models/Architecture/`
- [ ] T025 [P] Pipeline domain models in `src/C4Engineering.Web/Models/Pipeline/`
- [ ] T026 [P] Deployment domain models in `src/C4Engineering.Web/Models/Deployment/`

### Repository Layer
- [ ] T027 [P] IServiceRepository interface and JsonServiceRepository in `src/C4Engineering.Web/Data/Repositories/`
- [ ] T028 [P] IDiagramRepository interface and JsonDiagramRepository in `src/C4Engineering.Web/Data/Repositories/`
- [ ] T029 [P] IPipelineRepository interface and JsonPipelineRepository in `src/C4Engineering.Web/Data/Repositories/`
- [ ] T030 [P] IDeploymentRepository interface and JsonDeploymentRepository in `src/C4Engineering.Web/Data/Repositories/`
- [ ] T031 JsonDataStore base class for file operations in `src/C4Engineering.Web/Data/JsonDataStore.cs`

### Service Layer
- [ ] T032 [P] IServiceCatalogService interface and implementation in `src/C4Engineering.Web/Services/ServiceCatalogService.cs`
- [ ] T033 [P] IDiagramService interface and implementation in `src/C4Engineering.Web/Services/DiagramService.cs`
- [ ] T034 [P] IPipelineService interface and implementation in `src/C4Engineering.Web/Services/PipelineService.cs`
- [ ] T035 IDockerDeploymentService interface and implementation in `src/C4Engineering.Web/Services/DockerDeploymentService.cs`

### API Controllers
- [ ] T036 ServicesController with CRUD operations in `src/C4Engineering.Web/Controllers/ServicesController.cs`
- [ ] T037 DiagramsController with C4 model operations in `src/C4Engineering.Web/Controllers/DiagramsController.cs`
- [ ] T038 PipelinesController with build/deploy operations in `src/C4Engineering.Web/Controllers/PipelinesController.cs`
- [ ] T039 DeploymentsController with Docker management in `src/C4Engineering.Web/Controllers/DeploymentsController.cs`

## Phase 3.4: Frontend Implementation

### Razor Views and Layouts
- [ ] T040 [P] Main layout with Bootstrap navigation in `src/C4Engineering.Web/Views/Shared/_Layout.cshtml`
- [ ] T041 [P] Service catalog views in `src/C4Engineering.Web/Views/ServiceCatalog/`
- [ ] T042 [P] Architecture diagram views in `src/C4Engineering.Web/Views/Architecture/`
- [ ] T043 [P] Pipeline management views in `src/C4Engineering.Web/Views/Pipeline/`
- [ ] T044 [P] Deployment dashboard views in `src/C4Engineering.Web/Views/Deployment/`

### JavaScript Modules
- [ ] T045 [P] API client module in `src/C4Engineering.Web/wwwroot/js/shared/api-client.js`
- [ ] T046 [P] UI helpers and utilities in `src/C4Engineering.Web/wwwroot/js/shared/ui-helpers.js`
- [ ] T047 [P] Service catalog management in `src/C4Engineering.Web/wwwroot/js/service-catalog.js`
- [ ] T048 C4 diagram editor with drag-and-drop in `src/C4Engineering.Web/wwwroot/js/diagram-editor.js`
- [ ] T049 [P] Pipeline configuration UI in `src/C4Engineering.Web/wwwroot/js/pipeline-config.js`
- [ ] T050 [P] Deployment monitoring dashboard in `src/C4Engineering.Web/wwwroot/js/deployment-manager.js`

### CSS and Styling
- [ ] T051 [P] Custom CSS with Bootstrap overrides in `src/C4Engineering.Web/wwwroot/css/site.css`
- [ ] T052 [P] C4 diagram editor styles in `src/C4Engineering.Web/wwwroot/css/diagram-editor.css`

## Phase 3.5: Real-Time and Integration Features

### SignalR Hubs
- [ ] T053 DiagramCollaborationHub for real-time editing in `src/C4Engineering.Web/Hubs/DiagramCollaborationHub.cs`
- [ ] T054 [P] PipelineStatusHub for build/deploy notifications in `src/C4Engineering.Web/Hubs/PipelineStatusHub.cs`
- [ ] T055 [P] SignalR client JavaScript in `src/C4Engineering.Web/wwwroot/js/shared/signalr-client.js`

### Docker Integration
- [ ] T056 Docker Desktop connectivity and validation in DockerDeploymentService
- [ ] T057 Container lifecycle management (create, start, stop, remove)
- [ ] T058 Real-time container logs streaming via SignalR
- [ ] T059 Container health monitoring and resource usage tracking

### Data Initialization and Migration
- [ ] T060 [P] Sample data seeding for development in `src/C4Engineering.Web/Data/SampleDataSeeder.cs`
- [ ] T061 [P] Data validation and integrity checks
- [ ] T062 [P] File system watching for real-time updates

## Phase 3.6: Application Configuration and Startup

### Application Setup
- [ ] T063 Dependency injection configuration in `src/C4Engineering.Web/Program.cs`
- [ ] T064 SignalR hub registration and configuration
- [ ] T065 API endpoint registration with OpenAPI documentation
- [ ] T066 Static file serving and routing configuration
- [ ] T067 CORS and security headers configuration
- [ ] T068 Logging and application insights configuration

## Phase 3.7: Quality Assurance & Polish

### Performance and Reliability
- [ ] T069 [P] Performance benchmarking for API endpoints (≤500ms requirement)
- [ ] T070 [P] Load testing with NBomber for 50 concurrent users
- [ ] T071 [P] Memory usage optimization and monitoring
- [ ] T072 [P] Error handling middleware with user-friendly messages

### End-to-End Testing
- [ ] T073 [P] Playwright E2E test for service catalog workflow in `tests/C4Engineering.Tests/E2E/ServiceCatalogE2ETests.cs`
- [ ] T074 [P] Playwright E2E test for diagram creation and collaboration in `tests/C4Engineering.Tests/E2E/DiagramE2ETests.cs`
- [ ] T075 [P] Playwright E2E test for pipeline execution and deployment in `tests/C4Engineering.Tests/E2E/PipelineE2ETests.cs`

### Documentation and Deployment
- [ ] T076 [P] API documentation with Swagger/OpenAPI
- [ ] T077 [P] Docker Compose setup for local development
- [ ] T078 [P] README with setup and development instructions
- [ ] T079 [P] Deployment guide and environment configuration

## Dependencies

### Critical Path Dependencies
- **Phase 3.1 (Setup)** → **Phase 3.2 (Tests)** → **Phase 3.3 (Implementation)**
- **T001-T008** must complete before any test writing
- **T009-T022** (all tests) must complete and FAIL before **T023-T039** (implementation)
- **T031 (JsonDataStore)** blocks **T027-T030** (repositories)
- **T027-T030** (repositories) block **T032-T035** (services)
- **T032-T035** (services) block **T036-T039** (controllers)

### Parallel Execution Groups
```bash
# Phase 3.1 - Setup (all parallel)
T003, T004, T005, T006, T007, T008

# Phase 3.2 - Contract Tests (parallel within groups)
Group 1: T009, T010, T011
Group 2: T012, T013, T014
Group 3: T015, T016, T017, T018
Group 4: T019, T020, T021, T022

# Phase 3.3 - Models and Repositories (parallel)
T023, T024, T025, T026 (models)
T027, T028, T029, T030 (repositories)
T032, T033, T034 (services - T035 depends on Docker integration)

# Phase 3.4 - Frontend (mostly parallel)
T040, T041, T042, T043, T044 (views)
T045, T046, T047, T049, T050 (JavaScript - T048 depends on SignalR)
T051, T052 (CSS)
```

### Docker Desktop Dependencies
- **T056-T059** require Docker Desktop to be installed and running
- **T021** (Pipeline integration tests) depends on Docker availability
- **T075** (E2E pipeline tests) requires full Docker integration

## Constitution Compliance Notes

**Code Quality Excellence**:
- All tasks include comprehensive testing with 90%+ coverage requirement
- Static analysis and formatting configured in setup phase
- Code review checkpoints built into task dependencies

**Test-First Development**:
- TDD strictly enforced with Phase 3.2 completing before Phase 3.3
- Contract tests validate API specifications before implementation
- Integration tests cover cross-component interactions

**User Experience Consistency**:
- Bootstrap design system provides consistent UI components
- Error handling middleware ensures user-friendly error messages
- Performance requirements validated through load testing

**Performance Standards**:
- API response time requirements (≤500ms) validated through benchmarking
- Load testing ensures 50 concurrent user capacity
- Memory usage optimization included in quality assurance phase

**Security by Design**:
- CORS and security headers configured during application setup
- Input validation built into model definitions and API controllers
- Docker integration security considerations addressed

---

**Task Generation Complete**: 79 tasks generated across 7 phases with comprehensive dependency management and constitutional compliance.