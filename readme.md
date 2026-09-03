# StorageSystem

A cloud storage backend built with ASP.NET Core and PostgreSQL, designed around pluggable storage providers and a layered architecture.

> **Status:** Active development
> The current version supports local file storage, file metadata persistence, hashing, and RESTful file operations. Planned work includes S3-compatible object storage, authentication, chunked uploads, deduplication, streaming, and deployment.

## Overview

StorageSystem is a personal software engineering project focused on building the foundations of a scalable cloud storage platform.

The project separates API, domain, persistence, and storage concerns so that storage providers and infrastructure components can be changed without tightly coupling them to the application layer.

The main goal is to explore backend engineering concepts including file storage abstractions, metadata persistence, hashing, REST API design, testing, and eventually distributed storage concepts.

## Tech Stack

* C# / .NET
* ASP.NET Core Web API
* Entity Framework Core
* PostgreSQL
* Npgsql
* xUnit

## Architecture

```text
Client
  |
  v
ASP.NET Core API
  |
  v
Core / Domain
  |
  +--------------------+
  |                    |
  v                    v
PostgreSQL         File Storage
Metadata           Provider
                   |
                   v
               Local Storage
               (current)
```

The project is split into several components:

```text
src/
├── CloudStorage.Api
├── CloudStorage.Core
├── CloudStorage.Infrastructure
└── CloudStorage.Storage

tests/
└── CloudStorage.IntegrationTests
```

### CloudStorage.Api

Contains the HTTP API and file management endpoints.

### CloudStorage.Core

Contains core entities and interfaces used throughout the application.

### CloudStorage.Infrastructure

Contains the Entity Framework Core database context and PostgreSQL persistence logic.

### CloudStorage.Storage

Contains implementations of the storage abstraction. The current implementation stores files locally, with support for external object storage planned.

### CloudStorage.IntegrationTests

Contains tests for storage and application behaviour.

## Current Features

* Upload files through the API
* Store file metadata in PostgreSQL
* Generate SHA-256 hashes for uploaded files
* Retrieve stored file metadata
* Update file names
* Delete stored files and associated metadata
* Abstract file storage behind an `IFileStorage` interface
* Local filesystem storage implementation
* Integration tests for storage operations

## API

Current endpoints include:

```text
GET    /api/files
GET    /api/files/{id}
POST   /api/files
PATCH  /api/files/{id}
DELETE /api/files/{id}
```

## Storage Abstraction

Storage operations are defined through an interface rather than being coupled directly to the local filesystem.

```csharp
public interface IFileStorage
{
    Task<string> StoreAsync(Stream data, string path);
    Task<Stream> OpenReadAsync(string path);
    Task<bool> DeleteAsync(string path);
}
```

This allows additional storage providers, such as S3-compatible object storage, to be introduced without changing the rest of the application architecture.

## Roadmap

Planned development includes:

* S3-compatible object storage
* User authentication and authorization
* Chunked file uploads
* Content-based deduplication
* File streaming and range requests
* Improved error handling and validation
* Containerisation
* CI/CD
* Cloud deployment
* Expanded integration and API testing

## Project Goals

This project is being developed as a practical exploration of backend and distributed systems engineering, more so an opportunity for me to learn.

the goal is to progressively introduce challenges that appear in real storage systems, including large file handling, storage provider abstraction, deduplication, consistency, authentication, and scalability.

## Running Locally

### Requirements

* .NET SDK
* PostgreSQL

Clone the repository:

```bash
git clone https://github.com/DionFinn/StorageSystem.git
cd StorageSystem
```

Restore dependencies:

```bash
dotnet restore
```

Configure the PostgreSQL connection string in your local development configuration, then run:

```bash
dotnet run --project src/CloudStorage.Api
```

<3