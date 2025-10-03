# Data Model: C4Engineering Platform MVP

**Feature**: C4Engineering Platform MVP with Backstage Integration  
**Date**: 2025-01-10  
**Storage**: JSON files with structured directory layout

## Core Entities

### Service Catalog Domain

#### Service
**File**: `data/services/{service-id}.json`
```json
{
  "id": "payment-service",
  "name": "Payment Processing Service",
  "description": "Handles payment transactions and billing",
  "type": "service",
  "owner": "payments-team",
  "repository": "https://github.com/company/payment-service",
  "documentation": "https://docs.company.com/payment-service",
  "apiSpec": "/contracts/payment-service-api.json",
  "tags": ["payment", "billing", "microservice"],
  "lifecycle": "production",
  "system": "e-commerce-platform",
  "dependsOn": ["user-service", "notification-service"],
  "providesApis": ["payment-api-v1", "billing-api-v1"],
  "consumesApis": ["user-api-v1", "email-api-v1"],
  "metadata": {
    "createdAt": "2025-01-10T10:00:00Z",
    "updatedAt": "2025-01-10T10:00:00Z",
    "version": "1.2.3",
    "healthCheckUrl": "http://localhost:8080/health"
  }
}
```

#### Team
**File**: `data/teams/{team-id}.json`
```json
{
  "id": "payments-team",
  "name": "Payments Team",
  "description": "Responsible for payment processing and billing systems",
  "email": "payments-team@company.com",
  "slack": "#payments-team",
  "members": [
    {
      "name": "Alice Johnson",
      "role": "Tech Lead",
      "email": "alice@company.com"
    },
    {
      "name": "Bob Smith", 
      "role": "Senior Developer",
      "email": "bob@company.com"
    }
  ],
  "services": ["payment-service", "billing-service"],
  "oncallSchedule": "https://oncall.company.com/payments-team"
}
```

### Architecture Domain

#### C4Diagram
**File**: `data/diagrams/{diagram-id}.json`
```json
{
  "id": "payment-system-context",
  "name": "Payment System Context Diagram",
  "type": "context",
  "system": "payment-system",
  "description": "High-level view of payment system interactions",
  "elements": [
    {
      "id": "customer",
      "type": "person",
      "name": "Customer",
      "description": "Purchases products and makes payments",
      "position": { "x": 100, "y": 100 },
      "style": {
        "backgroundColor": "#1168bd",
        "color": "#ffffff",
        "shape": "person"
      }
    },
    {
      "id": "payment-system",
      "type": "system",
      "name": "Payment System",
      "description": "Processes payments and manages billing",
      "position": { "x": 300, "y": 100 },
      "style": {
        "backgroundColor": "#999999",
        "color": "#ffffff", 
        "shape": "system"
      }
    },
    {
      "id": "bank-api",
      "type": "external-system",
      "name": "Bank Payment API",
      "description": "External banking system for payment processing",
      "position": { "x": 500, "y": 100 },
      "style": {
        "backgroundColor": "#666666",
        "color": "#ffffff",
        "shape": "system"
      }
    }
  ],
  "relationships": [
    {
      "id": "customer-payment-system",
      "sourceId": "customer",
      "targetId": "payment-system", 
      "description": "Makes payments using",
      "technology": "HTTPS/REST",
      "style": {
        "lineStyle": "solid",
        "arrowStyle": "filled"
      }
    },
    {
      "id": "payment-system-bank-api",
      "sourceId": "payment-system",
      "targetId": "bank-api",
      "description": "Processes payments via",
      "technology": "HTTPS/REST",
      "style": {
        "lineStyle": "solid", 
        "arrowStyle": "filled"
      }
    }
  ],
  "metadata": {
    "createdAt": "2025-01-10T10:00:00Z",
    "updatedAt": "2025-01-10T10:00:00Z",
    "createdBy": "alice@company.com",
    "version": "1.0",
    "parentDiagram": null,
    "childDiagrams": ["payment-system-container"]
  }
}
```

#### DiagramElement
**Embedded in Diagram**: Individual elements within C4 diagrams
```json
{
  "id": "payment-service-container",
  "type": "container",
  "name": "Payment Service",
  "description": "Handles payment processing logic",
  "technology": "C# ASP.NET Core",
  "position": { "x": 200, "y": 150 },
  "size": { "width": 120, "height": 80 },
  "style": {
    "backgroundColor": "#1168bd",
    "color": "#ffffff",
    "borderColor": "#0d47a1",
    "borderWidth": 2,
    "shape": "container"
  },
  "properties": {
    "repository": "https://github.com/company/payment-service",
    "deploymentConfig": "payment-service-pipeline",
    "healthCheckUrl": "/health"
  }
}
```

### Pipeline Domain

#### PipelineConfiguration
**File**: `data/pipelines/{service-id}-pipeline.json`
```json
{
  "id": "payment-service-pipeline",
  "serviceId": "payment-service",
  "name": "Payment Service Build & Deploy",
  "description": "Builds and deploys payment service to local Docker",
  "stages": [
    {
      "id": "build",
      "name": "Build",
      "type": "build",
      "commands": [
        "dotnet restore",
        "dotnet build --configuration Release",
        "dotnet test --configuration Release --logger trx --collect:'XPlat Code Coverage'"
      ],
      "workingDirectory": "/src",
      "timeout": 300,
      "retryCount": 2
    },
    {
      "id": "docker-build",
      "name": "Docker Build",
      "type": "docker-build",
      "dockerFile": "Dockerfile",
      "imageName": "payment-service",
      "imageTag": "latest",
      "buildArgs": {
        "BUILD_VERSION": "${BUILD_NUMBER}"
      },
      "timeout": 600
    },
    {
      "id": "deploy-local",
      "name": "Deploy to Local Docker",
      "type": "docker-deploy",
      "imageName": "payment-service:latest",
      "containerName": "payment-service-local",
      "ports": ["8080:80"],
      "environment": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "ConnectionStrings__Database": "Data Source=./data/payment.db"
      },
      "volumes": ["./data:/app/data"],
      "healthCheck": {
        "url": "http://localhost:8080/health",
        "interval": 10,
        "timeout": 5,
        "retries": 3
      }
    }
  ],
  "triggers": {
    "manual": true,
    "onDiagramChange": false,
    "onRepositoryChange": false
  },
  "metadata": {
    "createdAt": "2025-01-10T10:00:00Z",
    "updatedAt": "2025-01-10T10:00:00Z",
    "createdBy": "alice@company.com"
  }
}
```

#### PipelineExecution
**File**: `data/executions/{execution-id}.json`
```json
{
  "id": "exec-payment-service-20250110-001",
  "pipelineId": "payment-service-pipeline",
  "status": "success",
  "startedAt": "2025-01-10T10:15:00Z",
  "completedAt": "2025-01-10T10:18:30Z",
  "duration": 210,
  "triggeredBy": "alice@company.com",
  "buildNumber": 42,
  "stages": [
    {
      "stageId": "build",
      "status": "success",
      "startedAt": "2025-01-10T10:15:00Z",
      "completedAt": "2025-01-10T10:16:30Z",
      "duration": 90,
      "logs": [
        "Restoring packages...",
        "Building solution...",
        "Running tests...",
        "All tests passed (45/45)"
      ],
      "artifacts": [
        "bin/Release/PaymentService.dll",
        "TestResults/coverage.xml"
      ]
    },
    {
      "stageId": "docker-build", 
      "status": "success",
      "startedAt": "2025-01-10T10:16:30Z",
      "completedAt": "2025-01-10T10:17:45Z",
      "duration": 75,
      "logs": [
        "Building Docker image...",
        "Successfully tagged payment-service:latest"
      ],
      "artifacts": [
        "docker-image:payment-service:latest"
      ]
    },
    {
      "stageId": "deploy-local",
      "status": "success", 
      "startedAt": "2025-01-10T10:17:45Z",
      "completedAt": "2025-01-10T10:18:30Z",
      "duration": 45,
      "logs": [
        "Stopping existing container...",
        "Starting new container...",
        "Health check passed"
      ],
      "deploymentUrl": "http://localhost:8080"
    }
  ]
}
```

### Deployment Domain

#### DeploymentConfiguration
**File**: `data/deployments/{service-id}-deployment.json`
```json
{
  "id": "payment-service-local-deployment",
  "serviceId": "payment-service",
  "environment": "local-docker",
  "status": "running",
  "containerName": "payment-service-local",
  "imageName": "payment-service:latest",
  "ports": [
    {
      "containerPort": 80,
      "hostPort": 8080,
      "protocol": "tcp"
    }
  ],
  "environment": {
    "ASPNETCORE_ENVIRONMENT": "Development",
    "ASPNETCORE_URLS": "http://+:80"
  },
  "volumes": [
    {
      "hostPath": "./data",
      "containerPath": "/app/data",
      "readOnly": false
    }
  ],
  "resources": {
    "cpuLimit": "1.0",
    "memoryLimit": "512MB"
  },
  "healthCheck": {
    "url": "http://localhost:8080/health",
    "status": "healthy",
    "lastChecked": "2025-01-10T10:20:00Z"
  },
  "deployment": {
    "deployedAt": "2025-01-10T10:18:30Z",
    "deployedBy": "alice@company.com",
    "executionId": "exec-payment-service-20250110-001"
  }
}
```

#### ContainerInstance
**File**: `data/containers/{container-id}.json`
```json
{
  "id": "container-payment-service-abc123",
  "deploymentId": "payment-service-local-deployment",
  "dockerContainerId": "abc123def456",
  "status": "running",
  "startedAt": "2025-01-10T10:18:30Z",
  "restartCount": 0,
  "ports": ["8080:80"],
  "resources": {
    "cpuUsage": 15.2,
    "memoryUsage": 180.5,
    "networkIO": {
      "bytesReceived": 1024,
      "bytesSent": 2048
    }
  },
  "logs": [
    {
      "timestamp": "2025-01-10T10:18:30Z",
      "level": "Info",
      "message": "Application started successfully"
    },
    {
      "timestamp": "2025-01-10T10:19:00Z", 
      "level": "Info",
      "message": "Health check endpoint responding"
    }
  ]
}
```

## Relationships and Constraints

### Service → Diagram Relationship
- Services can have multiple associated diagrams (Context, Container, Component levels)
- Diagrams reference services through service IDs in element properties
- Bidirectional updates: Service changes trigger diagram notifications

### Diagram → Pipeline Relationship  
- Container elements in diagrams can have associated pipeline configurations
- Pipeline execution creates deployments linked to diagram elements
- Visual indicators in diagrams show pipeline status and deployment health

### Pipeline → Deployment Relationship
- Successful pipeline executions create deployment configurations
- Deployment configurations reference the pipeline execution that created them
- Container instances track back to the deployment configuration

### Data Consistency Rules
1. **Service Deletion**: Must check for references in diagrams and pipelines
2. **Diagram Element Changes**: Must validate against pipeline configurations
3. **Pipeline Execution**: Must create corresponding deployment records
4. **Container Cleanup**: Failed deployments must clean up partial container instances

## File System Organization

```
wwwroot/data/
├── services/                    # Service catalog entries
│   ├── {service-id}.json
│   └── index.json              # Service listing for fast lookup
├── teams/                      # Team information
│   ├── {team-id}.json
│   └── index.json
├── diagrams/                   # C4 model diagrams
│   ├── {diagram-id}.json
│   └── index.json
├── pipelines/                  # Pipeline configurations
│   ├── {pipeline-id}.json
│   └── index.json
├── executions/                 # Pipeline execution history
│   ├── {execution-id}.json
│   └── index.json
├── deployments/                # Deployment configurations
│   ├── {deployment-id}.json
│   └── index.json
├── containers/                 # Container instance tracking  
│   ├── {container-id}.json
│   └── index.json
└── system/                     # System configuration
    ├── settings.json
    └── users.json              # Basic auth user storage
```

## Validation Rules

### Service Validation
- ID must be unique across all services
- Owner team must exist in teams collection
- Repository URL must be valid HTTP/HTTPS URL
- API spec must be valid OpenAPI format (if provided)

### Diagram Validation
- Element IDs must be unique within diagram
- Relationship source/target IDs must reference existing elements
- Parent/child diagram relationships must form valid hierarchy
- Position coordinates must be within canvas bounds

### Pipeline Validation
- Service ID must reference existing service
- Docker image names must follow naming conventions
- Port mappings must not conflict with existing deployments
- Health check URLs must be valid and accessible

### Deployment Validation
- Container names must be unique within Docker Desktop
- Port mappings must not conflict with existing containers
- Resource limits must be within Docker Desktop capacity
- Volume paths must be accessible on host system

---

**Data Model Complete**: All entities defined with JSON schemas and validation rules ready for implementation.