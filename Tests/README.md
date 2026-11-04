# Tests

Run everything: `dotnet test` (or Test Explorer, Run All).

```
Tests/
  Helpers/
    TestData.cs                        creates valid requests / vehicles / responses
    CustomWebApplicationFactory.cs     starts the real API with an in-memory SQLite database
  UnitTests/
    Controllers/                       one file per endpoint, the service is faked with Moq
    Services/VehicleServiceTests.cs    the service with an in-memory SQLite database
    Validation/                        the validation rules of VehicleRequest
  IntegrationTests/
    Vehicles/                          one file per endpoint, real HTTP requests
    Health/                            api/health, health/live, health/ready
```

Every test follows Arrange / Act / Assert and is named `Method_Scenario_ExpectedResult`.
