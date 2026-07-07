# Project Context Builder

## Overview

Project Context Builder is a .NET 10 console application that generates a single text file containing the important parts of a software project. The generated file is designed to be uploaded to AI assistants such as ChatGPT, allowing them to understand the project's architecture, source code, configuration, deployment files, and overall structure before answering questions.

The application recursively scans a project directory, filters files based on configurable rules, masks sensitive information, generates a directory tree, and combines everything into one text file.

---

# Features

* Recursively scans a project folder.
* Generates a complete directory tree.
* Combines multiple project files into a single text file.
* Writes the relative path before each file.
* Supports configurable include and exclude rules.
* Ignores build artifacts and unnecessary folders.
* Masks common secrets and credentials.
* Limits processing of extremely large files.
* Configuration driven through `appsettings.json`.

---

# Typical Use Case

Instead of uploading hundreds of files individually, generate one context file and upload it to an AI assistant.

Example prompt:

> This file contains my project context. Please understand the project architecture, source code, configuration, and deployment before answering my questions.

---

# Supported File Types

The default configuration includes common development files such as:

* C#
* Project files (`.csproj`)
* Solution files (`.sln`)
* JSON
* XML
* YAML
* Dockerfile
* Markdown
* SQL
* PowerShell
* Shell scripts

Additional extensions can easily be added through configuration.

---

# Ignored Folders

Typical folders excluded from processing include:

* `.git`
* `.vs`
* `.idea`
* `bin`
* `obj`
* `node_modules`
* `packages`
* `TestResults`

These folders generally contain generated or temporary files that are not useful for AI understanding.

---

# Ignored Files

Examples include:

* Development configuration
* User-specific settings
* Certificates
* Lock files
* Secret files

All exclusions are configurable.

---

# Secret Masking

The application attempts to mask commonly used sensitive values before writing them to the output file.

Examples include:

* Password
* Pwd
* Secret
* ClientSecret
* ApiKey
* Token
* ConnectionString

Example:

Before

```
Password=MySuperSecretPassword
```

After

```
Password=***MASKED***
```

> **Note:** Secret masking is best-effort and relies on pattern matching. Review the generated output before sharing it externally.

---

# Output Format

The generated file contains the following sections:

```
Project Information

Directory Tree

------------------------------------------------------------

File: src/MyApi/Program.cs

(file contents)

------------------------------------------------------------

File: src/MyApi/appsettings.json

(file contents)

...
```

Each file is clearly separated to help both humans and AI navigate the document.

---

# Configuration

All settings are stored in `appsettings.json`.

Important options include:

* Root project folder
* Output file
* Included extensions
* Included file names
* Ignored folders
* Ignored files
* Maximum file size

No code changes are required when modifying these settings.

---

# Limitations

* Binary files are not processed.
* Very large files are skipped based on the configured size limit.
* Secret masking is pattern-based and may not detect every custom secret format.
* Generated files should always be reviewed before being shared publicly.

---

# Future Enhancements

Potential future improvements include:

* HTML output
* Markdown output
* AI-generated project summary
* Dependency graph
* Namespace summary
* Class and method indexing
* Parallel file processing
* Incremental generation
* Git-aware filtering
* Plugin-based secret masking

---

# Requirements

* .NET 10 SDK
* Windows, Linux, or macOS

---

# Running

```
dotnet run
```

The application reads the configuration from `appsettings.json` and generates the configured output file.

---

# Project Structure

```
ProjectContextBuilder/
│
├── Program.cs
├── ProjectContextOptions.cs
├── appsettings.json
└── ProjectContextBuilder.csproj
```

---

# License

Internal utility project for generating AI-friendly project context files.
