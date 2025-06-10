---
_layout: landing
---

# Annium.Backend Documentation

Annium.Backend is a modular .NET 9.0 framework providing essential utilities, patterns, and abstractions for building robust backend applications.

## Cache

High-performance caching system with multiple storage backends:

- **[Annium.Cache.Abstractions](api/base/Cache/Annium.Cache.Abstractions/Annium.Cache.Abstractions.yml)** - Core caching abstractions and interfaces
- **[Annium.Cache.InMemory](api/base/Cache/Annium.Cache.InMemory/Annium.Cache.InMemory.yml)** - In-memory caching implementation
- **[Annium.Cache.Redis](api/base/Cache/Annium.Cache.Redis/Annium.Cache.Redis.yml)** - Redis-based distributed caching

## Infrastructure

Core infrastructure components for hosting and messaging:

- **[Annium.Infrastructure.Hosting](api/base/Infrastructure/Annium.Infrastructure.Hosting/Annium.Infrastructure.Hosting.yml)** - Application hosting and lifecycle management
- **[Annium.Infrastructure.MessageBus.Node](api/base/Infrastructure/Annium.Infrastructure.MessageBus.Node/Annium.Infrastructure.MessageBus.Node.yml)** - Message bus node implementation with multiple transport options

## Mesh Communication

Distributed messaging and communication framework:

- **[Annium.Mesh.Client](api/base/Mesh/Annium.Mesh.Client/Annium.Mesh.Client.yml)** - Mesh client implementation and factory
- **[Annium.Mesh.Domain](api/base/Mesh/Annium.Mesh.Domain/Annium.Mesh.Domain.yml)** - Core domain models and message types
- **[Annium.Mesh.Server](api/base/Mesh/Annium.Mesh.Server/Annium.Mesh.Server.yml)** - Mesh server implementation and coordination
- **[Annium.Mesh.Server.InMemory](api/base/Mesh/Annium.Mesh.Server.InMemory/Annium.Mesh.Server.InMemory.yml)** - In-memory mesh server for testing
- **[Annium.Mesh.Server.Sockets](api/base/Mesh/Annium.Mesh.Server.Sockets/Annium.Mesh.Server.Sockets.yml)** - Socket-based mesh server
- **[Annium.Mesh.Server.Web](api/base/Mesh/Annium.Mesh.Server.Web/Annium.Mesh.Server.Web.yml)** - Web-based mesh server with HTTP support

### Mesh Serialization

- **[Annium.Mesh.Serialization.Abstractions](api/base/Mesh/Annium.Mesh.Serialization.Abstractions/Annium.Mesh.Serialization.Abstractions.yml)** - Serialization abstractions for mesh communication
- **[Annium.Mesh.Serialization.Json](api/base/Mesh/Annium.Mesh.Serialization.Json/Annium.Mesh.Serialization.Json.yml)** - JSON serialization for mesh messages
- **[Annium.Mesh.Serialization.MessagePack](api/base/Mesh/Annium.Mesh.Serialization.MessagePack/Annium.Mesh.Serialization.MessagePack.yml)** - MessagePack serialization for mesh messages

### Mesh Transport

- **[Annium.Mesh.Transport.Abstractions](api/base/Mesh/Annium.Mesh.Transport.Abstractions/Annium.Mesh.Transport.Abstractions.yml)** - Transport layer abstractions for mesh communication
- **[Annium.Mesh.Transport.InMemory](api/base/Mesh/Annium.Mesh.Transport.InMemory/Annium.Mesh.Transport.InMemory.yml)** - In-memory transport for testing
- **[Annium.Mesh.Transport.Sockets](api/base/Mesh/Annium.Mesh.Transport.Sockets/Annium.Mesh.Transport.Sockets.yml)** - Socket-based transport layer
- **[Annium.Mesh.Transport.WebSockets](api/base/Mesh/Annium.Mesh.Transport.WebSockets/Annium.Mesh.Transport.WebSockets.yml)** - WebSocket transport layer

## Storage

Unified storage abstraction supporting multiple backends:

- **[Annium.Storage.Abstractions](api/base/Storage/Annium.Storage.Abstractions/Annium.Storage.Abstractions.yml)** - Core storage abstractions and interfaces
- **[Annium.Storage.FileSystem](api/base/Storage/Annium.Storage.FileSystem/Annium.Storage.FileSystem.yml)** - File system storage implementation
- **[Annium.Storage.InMemory](api/base/Storage/Annium.Storage.InMemory/Annium.Storage.InMemory.yml)** - In-memory storage for testing and development
- **[Annium.Storage.S3](api/base/Storage/Annium.Storage.S3/Annium.Storage.S3.yml)** - Amazon S3 compatible storage

## Integrations

Third-party service integrations:

### ASP.NET Core

- **[Annium.AspNetCore.Extensions](api/integrations/AspNetCore/Annium.AspNetCore.Extensions/Annium.AspNetCore.Extensions.yml)** - ASP.NET Core extensions and middleware
- **[Annium.AspNetCore.IntegrationTesting](api/integrations/AspNetCore/Annium.AspNetCore.IntegrationTesting/Annium.AspNetCore.IntegrationTesting.yml)** - Integration testing utilities for ASP.NET Core
- **[Annium.AspNetCore.Mesh](api/integrations/AspNetCore/Annium.AspNetCore.Mesh/Annium.AspNetCore.Mesh.yml)** - Mesh communication integration for ASP.NET Core

### Database

- **[Annium.DbUp.Core](api/integrations/DbUp/Annium.DbUp.Core/Annium.DbUp.Core.yml)** - Database migration engine core
- **[Annium.DbUp.PostgreSql](api/integrations/DbUp/Annium.DbUp.PostgreSql/Annium.DbUp.PostgreSql.yml)** - PostgreSQL migration support
- **[Annium.EntityFrameworkCore.Extensions](api/integrations/EntityFrameworkCore/Annium.EntityFrameworkCore.Extensions/Annium.EntityFrameworkCore.Extensions.yml)** - Entity Framework Core extensions
- **[Annium.linq2db.Extensions](api/integrations/linq2db/Annium.linq2db.Extensions/Annium.linq2db.Extensions.yml)** - linq2db ORM extensions and utilities
- **[Annium.linq2db.PostgreSql](api/integrations/linq2db/Annium.linq2db.PostgreSql/Annium.linq2db.PostgreSql.yml)** - PostgreSQL support for linq2db

### External Services

- **[Annium.MongoDb.NodaTime](api/integrations/MongoDb/Annium.MongoDb.NodaTime/Annium.MongoDb.NodaTime.yml)** - NodaTime serialization support for MongoDB
- **[Annium.Redis](api/integrations/Redis/Annium.Redis/Annium.Redis.yml)** - Redis client and storage utilities

## Getting Started

Each component is designed to work independently while providing seamless integration when used together. Explore the API documentation for detailed information about each module.

## Key Features

- **Modular Design** - Use only what you need
- **Multiple Storage Backends** - Support for various storage and caching solutions
- **Mesh Communication** - Distributed messaging with multiple transport layers
- **Database Integration** - Support for multiple ORM and database technologies
- **ASP.NET Core Integration** - Seamless web application development

## Contributing

If you find any issues or have suggestions for improving the documentation, please feel free to contribute by submitting a pull request.