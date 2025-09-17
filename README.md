# API Testing with C# (NUnit + RestSharp)

This project contains automated tests for the Restful API (https://restful-api.dev/) using C#, NUnit, and RestSharp.  
The focus is on testing the PATCH /objects/{id} endpoint with positive, negative, security, and performance scenarios.

---

## Project Setup

### Prerequisites
- .NET SDK 7.0+ (https://dotnet.microsoft.com/download)
- Visual Studio Code (https://code.visualstudio.com/)
- VS Code Extensions:
  - C# (by Microsoft)
  - Test Explorer UI (optional for running/debugging tests inside VS Code)

---

## Project Structure
```
ApiTests/
 ├── PatchObjectTests.cs   # NUnit test cases for PATCH API
 ├── bin/                  # Build output
 ├── obj/                  # Temporary build files
 └── README.md             # Project documentation
```

---

## How to Run Tests

### Run All Tests
```bash
dotnet test
```

### Run a Single Test
```bash
dotnet test --filter "Name=TC01_UpdateSingleField_ShouldReturn200"
```

### Build Only
```bash
dotnet build
```

---

## Covered Test Scenarios
- Positive Cases
  - Update single/multiple fields
  - Update numeric and special character fields
- Negative Cases
  - Invalid ID
  - Empty body
  - Invalid JSON
  - Large payloads
- Security
  - SQL injection attempt
  - XSS attempt
- Performance
  - Concurrent updates
  - Frequent rapid updates

---

## Sample Output
When running tests:
```
Passed!  - Failed: 0, Passed: 14, Skipped: 0, Total: 14, Duration: 4.1s
```

---

## CI/CD Integration
You can integrate with GitHub Actions by adding .github/workflows/dotnet-tests.yml:

```yaml
name: API Tests

on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    - run: dotnet restore
    - run: dotnet build --no-restore
    - run: dotnet test --no-build --verbosity normal
```

---

## Notes
- Logs show request & response bodies for debugging.  
- You can add new endpoints by creating additional test classes (e.g., PostObjectTests.cs, GetObjectTests.cs).  
- For full request/response inspection, you can also run tests through a proxy tool like Fiddler or Postman proxy.  

---

Author: Pardeep Ohri  
Contact: pardeepohri@gmail.com
