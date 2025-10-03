# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → If not found: ERROR "No implementation plan found"
   → Extract: tech stack, libraries, structure
2. Load optional design documents:
   → data-model.md: Extract entities → model tasks
   → contracts/: Each file → contract test task
   → research.md: Extract decisions → setup tasks
3. Generate tasks by category:
   → Setup: project init, dependencies, linting
   → Tests: contract tests, integration tests
   → Core: models, services, CLI commands
   → Integration: DB, middleware, logging
   → Polish: unit tests, performance, docs
4. Apply task rules:
   → Different files = mark [P] for parallel
   → Same file = sequential (no [P])
   → Tests before implementation (TDD)
5. Number tasks sequentially (T001, T002...)
6. Generate dependency graph
7. Create parallel execution examples
8. Validate task completeness:
   → All contracts have tests?
   → All entities have models?
   → All endpoints implemented?
9. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Path Conventions
- **Single project**: `src/`, `tests/` at repository root
- **Web app**: `backend/src/`, `frontend/src/`
- **Mobile**: `api/src/`, `ios/src/` or `android/src/`
- Paths shown below assume single project - adjust based on plan.md structure

## Phase 3.1: Setup & Quality Foundation
- [ ] T001 Create project structure per implementation plan
- [ ] T002 Initialize [language] project with [framework] dependencies
- [ ] T003 [P] Configure linting, formatting, and static analysis tools (constitution requirement)
- [ ] T004 [P] Set up code coverage reporting (90% target)
- [ ] T005 [P] Configure security scanning tools
- [ ] T006 [P] Set up performance monitoring and benchmarking tools

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
**CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation**
- [ ] T007 [P] Contract test POST /api/users in tests/contract/test_users_post.py
- [ ] T008 [P] Contract test GET /api/users/{id} in tests/contract/test_users_get.py
- [ ] T009 [P] Integration test user registration in tests/integration/test_registration.py
- [ ] T010 [P] Integration test auth flow in tests/integration/test_auth.py
- [ ] T011 [P] Performance test for API endpoints (≤500ms requirement)
- [ ] T012 [P] Security test for input validation and authorization

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [ ] T013 [P] User model in src/models/user.py (with comprehensive documentation)
- [ ] T014 [P] UserService CRUD in src/services/user_service.py
- [ ] T015 [P] CLI --create-user in src/cli/user_commands.py
- [ ] T016 POST /api/users endpoint with proper error handling
- [ ] T017 GET /api/users/{id} endpoint with performance optimization
- [ ] T018 Input validation (security by design)
- [ ] T019 Error handling with user-friendly messages (UX consistency)
- [ ] T020 Structured logging for observability

## Phase 3.4: Integration & Security
- [ ] T021 Connect UserService to DB with connection pooling
- [ ] T022 Auth middleware with security best practices
- [ ] T023 Request/response logging with performance metrics
- [ ] T024 CORS and security headers configuration
- [ ] T025 Data encryption at rest and in transit
- [ ] T026 Rate limiting and DDoS protection

## Phase 3.5: Quality Assurance & Polish
- [ ] T027 [P] Unit tests for validation in tests/unit/test_validation.py (90%+ coverage)
- [ ] T028 [P] Accessibility testing for user interfaces (WCAG 2.1 AA)
- [ ] T029 [P] Performance benchmarking and optimization (meet ≤500ms SLA)
- [ ] T030 [P] Security penetration testing
- [ ] T031 [P] Update API documentation (OpenAPI/Swagger)
- [ ] T032 [P] Code review and refactoring for maintainability
- [ ] T033 [P] Dependency security audit and updates
- [ ] T034 Load testing for scalability requirements (10x capacity)
- [ ] T035 Run comprehensive manual testing scenarios

## Dependencies
- Quality foundation (T001-T006) before tests (T007-T012)
- Tests (T007-T012) before implementation (T013-T020)
- Core implementation (T013-T020) before integration (T021-T026)
- Integration (T021-T026) before quality assurance (T027-T035)
- T013 blocks T014, T021
- T022 blocks T024
- Performance tests (T028, T034) require implementation completion

## Parallel Example
```
# Launch setup tasks together:
Task: "Configure linting, formatting, and static analysis tools"
Task: "Set up code coverage reporting (90% target)"
Task: "Configure security scanning tools"
Task: "Set up performance monitoring and benchmarking tools"

# Launch test tasks together:
Task: "Contract test POST /api/users in tests/contract/test_users_post.py"
Task: "Contract test GET /api/users/{id} in tests/contract/test_users_get.py"
Task: "Integration test registration in tests/integration/test_registration.py"
Task: "Performance test for API endpoints (≤500ms requirement)"
```

## Constitution Compliance Notes
- **Code Quality**: All tasks include documentation and follow clean code principles
- **Test-First**: TDD strictly enforced with 90%+ coverage requirement
- **UX Consistency**: Error handling provides clear, actionable feedback
- **Performance**: All tasks meet ≤500ms API response requirement
- **Security**: Security by design integrated throughout all phases
- **Maintainability**: Code review and refactoring explicitly included

## Notes
- [P] tasks = different files, no dependencies
- Verify tests fail before implementing (TDD mandatory)
- All tasks must pass quality gates (linting, security, performance)
- Code coverage must reach 90% minimum before task completion
- Performance benchmarks must meet constitution SLA requirements
- Security scans must pass before integration phase
- Commit after each task with meaningful commit messages
- Avoid: vague tasks, same file conflicts, skipping quality checks

## Task Generation Rules
*Applied during main() execution*

1. **From Contracts**:
   - Each contract file → contract test task [P]
   - Each endpoint → implementation task
   
2. **From Data Model**:
   - Each entity → model creation task [P]
   - Relationships → service layer tasks
   
3. **From User Stories**:
   - Each story → integration test [P]
   - Quickstart scenarios → validation tasks

4. **Ordering**:
   - Setup → Tests → Models → Services → Endpoints → Polish
   - Dependencies block parallel execution

## Validation Checklist
*GATE: Checked by main() before returning*

- [ ] All contracts have corresponding tests
- [ ] All entities have model tasks
- [ ] All tests come before implementation
- [ ] Parallel tasks truly independent
- [ ] Each task specifies exact file path
- [ ] No task modifies same file as another [P] task