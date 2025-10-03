
# Implementation Plan: C4Engineering Platform MVP with Backstage Integration

**Branch**: `002-build-an-application` | **Date**: 2025-01-10 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `C:\data\C4Engineering\specs\002-build-an-application\spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → Loaded: C4Engineering Platform MVP with Backstage Integration ✓
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Project Type: Web application (frontend + backend) ✓
   → Structure Decision: ASP.NET Core backend + Bootstrap frontend ✓
3. Fill the Constitution Check section based on the constitution document.
   → Constitution requirements evaluated ✓
4. Evaluate Constitution Check section below
   → No constitutional violations identified ✓
   → Update Progress Tracking: Initial Constitution Check ✓
5. Execute Phase 0 → research.md
   → Technical decisions documented ✓
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, .github/copilot-instructions.md
   → Design artifacts generated ✓
7. Re-evaluate Constitution Check section
   → Post-design evaluation completed ✓
   → Update Progress Tracking: Post-Design Constitution Check ✓
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
   → Task strategy defined ✓
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary
Build a comprehensive platform engineering MVP that combines Backstage-inspired service catalog functionality with interactive C4 model architecture visualization and local Docker Desktop deployment capabilities. The platform uses C# .NET backend with minimal Bootstrap frontend, vanilla web technologies, and JSON file storage for rapid prototyping and development.

## Technical Context
**Language/Version**: C# .NET 8.0 with ASP.NET Core 8.0  
**Primary Dependencies**: Bootstrap 5.3, SignalR (real-time), System.Text.Json, Docker.DotNet  
**Storage**: JSON files with structured directory layout for local prototype development  
**Testing**: xUnit, Moq, Playwright for end-to-end testing  
**Target Platform**: Cross-platform web application (Windows, macOS, Linux) with Docker Desktop integration
**Project Type**: Web application (ASP.NET Core backend + Bootstrap frontend)  
**Performance Goals**: <200ms UI response, <500ms API response, <2s diagram rendering, handle 50 concurrent users  
**Constraints**: Minimal external dependencies, vanilla JavaScript preferred, local Docker Desktop only, JSON storage  
**Scale/Scope**: 100-500 services in catalog, 20-50 concurrent users, 5-10 simultaneous deployments per user

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Code Quality Excellence**:
- [x] Design follows clean code principles with clear separation of concerns (MVC pattern, service layer)
- [x] All public APIs will be documented with XML comments and OpenAPI specifications
- [x] Technical debt introduction justified: JSON storage temporary for MVP, will migrate to database

**Test-First Development**:
- [x] Test strategy documented: Unit tests (xUnit), Integration tests (TestServer), E2E tests (Playwright)
- [x] Unit tests planned for all business logic with 90%+ coverage target
- [x] Integration tests planned for API endpoints and Docker integration
- [x] End-to-end tests planned for critical user workflows (service catalog → diagram → deploy)

**User Experience Consistency**:
- [x] Bootstrap design system provides consistent UI components and responsive layout
- [x] Accessibility requirements (WCAG 2.1 AA) addressed through semantic HTML and ARIA attributes
- [x] Error handling strategy provides clear, actionable feedback through toast notifications and inline validation
- [x] Cross-platform consistency maintained through responsive Bootstrap design

**Performance Standards**:
- [x] Response time requirements defined (≤200ms UI, ≤500ms API, ≤2s diagram rendering)
- [x] Scalability requirements specified (50 concurrent users, 10x load capacity)
- [x] Performance testing strategy included using NBomber for load testing
- [x] Resource efficiency considerations: Minimal JavaScript libraries, efficient JSON serialization

**Security by Design**:
- [x] Security threat model will address Docker Desktop integration and file system access
- [x] Authentication/authorization requirements: Basic auth for MVP, extensible to SSO
- [x] Data protection: Local JSON files with proper file permissions, input validation
- [x] Input validation and output encoding strategy planned for all user inputs

## Project Structure

### Documentation (this feature)
```
specs/002-build-an-application/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
# Web application structure (ASP.NET Core + Bootstrap frontend)
src/
├── C4Engineering.Web/                    # ASP.NET Core web application
│   ├── Controllers/                      # MVC controllers for API endpoints
│   │   ├── ServiceCatalogController.cs
│   │   ├── DiagramController.cs
│   │   ├── PipelineController.cs
│   │   └── DeploymentController.cs
│   ├── Models/                           # Data models and DTOs
│   │   ├── ServiceCatalog/
│   │   ├── Architecture/
│   │   ├── Pipeline/
│   │   └── Deployment/
│   ├── Services/                         # Business logic services
│   │   ├── ServiceCatalogService.cs
│   │   ├── DiagramService.cs
│   │   ├── PipelineService.cs
│   │   └── DockerDeploymentService.cs
│   ├── Data/                            # JSON file data access
│   │   ├── JsonDataStore.cs
│   │   └── Repositories/
│   ├── Hubs/                            # SignalR hubs for real-time features
│   │   └── DiagramCollaborationHub.cs
│   ├── wwwroot/                         # Static web assets
│   │   ├── css/                         # Custom CSS + Bootstrap
│   │   ├── js/                          # Vanilla JavaScript modules
│   │   │   ├── service-catalog.js
│   │   │   ├── diagram-editor.js
│   │   │   ├── pipeline-config.js
│   │   │   └── deployment-manager.js
│   │   ├── lib/                         # Third-party libraries
│   │   │   └── bootstrap/               # Bootstrap 5.3
│   │   └── data/                        # JSON data storage
│   │       ├── services/
│   │       ├── diagrams/
│   │       ├── pipelines/
│   │       └── deployments/
│   ├── Views/                           # Razor views
│   │   ├── Home/
│   │   ├── ServiceCatalog/
│   │   ├── Architecture/
│   │   └── Shared/
│   └── Program.cs                       # Application entry point

tests/
├── C4Engineering.Tests/                 # Unit and integration tests
│   ├── Controllers/                     # Controller tests
│   ├── Services/                        # Service layer tests
│   ├── Integration/                     # Integration tests
│   └── E2E/                            # End-to-end Playwright tests
```

## Phase 0: Research & Technical Decisions

**Output**: research.md with all technical decisions documented

### Key Technology Decisions Made
1. **ASP.NET Core 8.0**: Modern web framework with excellent Docker integration
2. **Bootstrap 5.3**: Minimal UI framework with responsive design
3. **Vanilla JavaScript**: ES6 modules for frontend without framework overhead
4. **JSON File Storage**: Rapid prototyping with structured directory layout
5. **SignalR**: Real-time collaboration for diagram editing
6. **Docker.DotNet**: Official Docker SDK for container management
7. **xUnit + Playwright**: Comprehensive testing strategy

### Architecture Patterns Established
- **Domain-Driven Design (DDD) Lite**: Service layer separation
- **Repository Pattern**: Data access abstraction for JSON storage
- **MVC with API**: Combined REST API and server-side rendering
- **Module Pattern**: JavaScript ES6 modules for frontend organization

## Phase 1: Design & Contracts

**Output**: API contracts, data model, quickstart guide, and agent instructions

### 1. API Contract Generation
Generated OpenAPI specifications for:
- **Service Catalog API**: CRUD operations with Backstage compatibility
- **Diagram API**: C4 model management with real-time collaboration
- **Pipeline API**: Build/deploy pipeline configuration and execution
- **Deployment API**: Docker Desktop container management

### 2. Data Model Design
Comprehensive JSON schema definitions for:
- **Service Catalog Domain**: Services, teams, dependencies
- **Architecture Domain**: C4 diagrams, elements, relationships
- **Pipeline Domain**: Configurations, executions, stages
- **Deployment Domain**: Container instances, health monitoring

### 3. File System Organization
Structured data directory layout:
```
wwwroot/data/
├── services/          # Service catalog entries
├── teams/             # Team information  
├── diagrams/          # C4 model diagrams
├── pipelines/         # Pipeline configurations
├── executions/        # Pipeline execution history
├── deployments/       # Deployment configurations
├── containers/        # Container instance tracking
└── system/            # System configuration
```

### 4. Quickstart Validation Scenarios
End-to-end user workflows defined:
- Service discovery and catalog management
- C4 architecture visualization with drag-and-drop
- Pipeline configuration from diagram elements
- Local Docker Desktop deployment and monitoring

### 5. Agent-Specific Instructions
GitHub Copilot instructions document created with:
- Code style and conventions
- Domain model patterns
- Data access patterns
- API controller patterns
- SignalR hub patterns
- Testing patterns
- Docker integration patterns

## Phase 2: Task Generation Strategy

**Approach**: Generate tasks by domain areas with TDD enforcement

### Task Categories
1. **Setup & Infrastructure**: Project initialization, dependencies, tooling
2. **Domain Models**: Core entities and data transfer objects
3. **Data Access Layer**: JSON repository implementations with testing
4. **Service Layer**: Business logic services with comprehensive unit tests
5. **API Controllers**: REST endpoints with integration tests
6. **Frontend Components**: JavaScript modules and Razor views
7. **Real-Time Features**: SignalR hubs for collaboration
8. **Docker Integration**: Container deployment and monitoring
9. **End-to-End Validation**: Playwright tests for critical workflows

### TDD Enforcement Strategy
- **Contract Tests First**: API contract validation before implementation
- **Unit Tests Before Code**: Red-Green-Refactor cycle strictly enforced
- **Integration Tests**: Full HTTP pipeline testing with TestServer
- **E2E Tests**: Critical user workflows with Playwright automation

### Parallel Execution Opportunities
- Domain model development across different domains
- Independent JavaScript module development
- API controller implementation per domain
- Test development parallel to implementation
- Documentation creation alongside feature development

---

## Complexity Tracking
No constitutional violations identified. Design follows established patterns:
- Clean architecture with clear separation of concerns
- Minimal external dependencies for reduced complexity
- JSON storage provides simplicity for MVP phase
- Performance requirements met through async patterns and efficient rendering

## Progress Tracking

**Phase Status**:
- [x] Phase 0: Research complete (all technical decisions documented)
- [x] Phase 1: Design artifacts generated (contracts, data model, quickstart, agent instructions)
- [ ] Phase 2: Tasks generated (/tasks command)
- [ ] Phase 3: Implementation complete
- [ ] Phase 4: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS  
- [x] All technical decisions documented
- [x] No constitutional violations identified

---

*Based on Constitution v1.0.0 - See `.specify/memory/constitution.md`*
└── tests/

# [REMOVE IF UNUSED] Option 3: Mobile + API (when "iOS/Android" detected)
api/
└── [same as backend above]

ios/ or android/
└── [platform-specific structure: feature modules, UI flows, platform tests]
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Phase 0: Outline & Research
1. **Extract unknowns from Technical Context** above:
   - For each NEEDS CLARIFICATION → research task
   - For each dependency → best practices task
   - For each integration → patterns task

2. **Generate and dispatch research agents**:
   ```
   For each unknown in Technical Context:
     Task: "Research {unknown} for {feature context}"
   For each technology choice:
     Task: "Find best practices for {tech} in {domain}"
   ```

3. **Consolidate findings** in `research.md` using format:
   - Decision: [what was chosen]
   - Rationale: [why chosen]
   - Alternatives considered: [what else evaluated]

**Output**: research.md with all NEEDS CLARIFICATION resolved

## Phase 1: Design & Contracts
*Prerequisites: research.md complete*

1. **Extract entities from feature spec** → `data-model.md`:
   - Entity name, fields, relationships
   - Validation rules from requirements
   - State transitions if applicable

2. **Generate API contracts** from functional requirements:
   - For each user action → endpoint
   - Use standard REST/GraphQL patterns
   - Output OpenAPI/GraphQL schema to `/contracts/`

3. **Generate contract tests** from contracts:
   - One test file per endpoint
   - Assert request/response schemas
   - Tests must fail (no implementation yet)

4. **Extract test scenarios** from user stories:
   - Each story → integration test scenario
   - Quickstart test = story validation steps

5. **Update agent file incrementally** (O(1) operation):
   - Run `.specify/scripts/powershell/update-agent-context.ps1 -AgentType copilot`
     **IMPORTANT**: Execute it exactly as specified above. Do not add or remove any arguments.
   - If exists: Add only NEW tech from current plan
   - Preserve manual additions between markers
   - Update recent changes (keep last 3)
   - Keep under 150 lines for token efficiency
   - Output to repository root

**Output**: data-model.md, /contracts/*, failing tests, quickstart.md, agent-specific file

## Phase 2: Task Planning Approach
*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy**:
- Load `.specify/templates/tasks-template.md` as base
- Generate tasks from Phase 1 design docs (contracts, data model, quickstart)
- Each contract → contract test task [P]
- Each entity → model creation task [P] 
- Each user story → integration test task
- Implementation tasks to make tests pass

**Ordering Strategy**:
- TDD order: Tests before implementation 
- Dependency order: Models before services before UI
- Mark [P] for parallel execution (independent files)

**Estimated Output**: 25-30 numbered, ordered tasks in tasks.md

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)  
**Phase 4**: Implementation (execute tasks.md following constitutional principles)  
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [ ] Phase 0: Research complete (/plan command)
- [ ] Phase 1: Design complete (/plan command)
- [ ] Phase 2: Task planning complete (/plan command - describe approach only)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [ ] Initial Constitution Check: PASS
- [ ] Post-Design Constitution Check: PASS
- [ ] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on Constitution v1.0.0 - See `.specify/memory/constitution.md`*
