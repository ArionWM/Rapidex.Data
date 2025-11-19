# Rapidex Logging - Quick Reference

## Setup (3 Steps)

### 1. Add Package
```bash
dotnet add package Rapidex.Base.Common.Logging.Serilog.Core8
```

### 2. Configure in Program.cs
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.UseRapidexSerilog();  // That's it!
var app = builder.Build();
app.Run();
```

### 3. Configure in appsettings.json
```json
{
  "RapidexLogging": {
    "DefaultMinimumLevel": "Information",
    "CategoryMinimumLevels": {
      "Microsoft": "Warning",
      "Rapidex": "Debug"
    }
  }
}
```

## Common Configurations

### Minimal Setup
```csharp
builder.UseRapidexSerilog();
```

### Production Setup
```csharp
builder.UseRapidexSerilog(config =>
{
    config.DefaultMinimumLevel = LogLevel.Information;
    config.UseBufferForNonErrors = true;
    config.BufferFlushIntervalSeconds = 10;
    config.CategoryMinimumLevels["Microsoft"] = LogLevel.Warning;
});
```

### Development Setup
```csharp
builder.UseRapidexSerilog(config =>
{
    config.DefaultMinimumLevel = LogLevel.Debug;
    config.WriteToConsole = true;
    config.UseBufferForNonErrors = false;
});
```

## Log Levels

| Level | Value | Usage |
|-------|-------|-------|
| `Trace` | Most verbose | Loop iterations, variable values |
| `Debug` | Debugging | Method calls, state changes |
| `Information` | Default | User actions, business events |
| `Warning` | Caution | Deprecated APIs, retries |
| `Error` | Problems | Failed operations, exceptions |
| `Critical` | Severe | System failures, data loss |

## Using in Code

### Basic Logging
```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoWork()
    {
        _logger.LogInformation("Starting work");
        _logger.LogDebug("Processing item {ItemId}", itemId);
        _logger.LogError(ex, "Failed to process {ItemId}", itemId);
    }
}
```

### Runtime Level Control
```csharp
public class AdminController : ControllerBase
{
    private readonly ILogLevelController _logLevelController;

    public AdminController(ILogLevelController logLevelController)
    {
        _logLevelController = logLevelController;
    }

    [HttpPost("set-log-level")]
    public IActionResult SetLevel(string category, string level)
    {
        _logLevelController.SetMinimumLevel(category, Enum.Parse<LogLevel>(level));
        return Ok();
    }
}
```

## Configuration Properties Quick Ref

| Property | Type | Default | Use When |
|----------|------|---------|----------|
| `LogDirectory` | string | "Logs" | Change log location |
| `LogFilePrefix` | string | "app" | Customize file names |
| `DefaultMinimumLevel` | LogLevel | Debug | Set base log level |
| `UseSeperateErrorLogFile` | bool | true | Want error-only logs |
| `UseSeperateWarningLogFile` | bool | true | Want warning-only logs |
| `UseBufferForNonErrors` | bool | true | Optimize disk I/O |
| `BufferFlushIntervalSeconds` | int | 5 | Control flush timing |
| `WriteToConsole` | bool | false | Debug or Docker |
| `CategoryMinimumLevels` | Dictionary | {} | Filter framework logs |
| `CategorySeparateFiles` | Dictionary | {} | Isolate critical logs |

## Common Scenarios

### Suppress Framework Noise
```json
{
  "RapidexLogging": {
    "DefaultMinimumLevel": "Debug",
    "CategoryMinimumLevels": {
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}
```

### Separate Critical Service Logs
```json
{
  "RapidexLogging": {
    "CategorySeparateFiles": {
      "MyApp.Services.Payment": true
    }
  }
}
```

### Development with Console
```json
{
  "RapidexLogging": {
    "DefaultMinimumLevel": "Debug",
    "WriteToConsole": true,
    "UseBufferForNonErrors": false
  }
}
```

### Production Optimized
```json
{
  "RapidexLogging": {
    "DefaultMinimumLevel": "Information",
    "UseBufferForNonErrors": true,
    "BufferFlushIntervalSeconds": 10,
    "CategoryMinimumLevels": {
      "Microsoft": "Warning"
    }
  }
}
```

## Log File Outputs

With defaults, you get:
```
Logs/
??? app-20240101.log           # All logs (buffered)
??? app-error-20240101.log     # Errors only (immediate)
??? app-warning-20240101.log   # Warnings+ (immediate)
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| No logs appearing | Set `DefaultMinimumLevel` to `Trace` and `WriteToConsole` to `true` |
| Too many framework logs | Add to `CategoryMinimumLevels`: `"Microsoft": "Warning"` |
| Logs not immediate | Set `UseBufferForNonErrors` to `false` |
| Files too large | Reduce `MaxLogFileSize` or `RetainedFileCountLimit` |

## Best Practices

? **DO**
- Use structured logging: `_logger.LogInformation("User {UserId} logged in", userId)`
- Set `Microsoft` and `System` categories to `Warning` in production
- Enable buffering in production for better performance
- Use separate files for critical services
- Use `ILogLevelController` for runtime diagnostics

? **DON'T**
- Use string interpolation: `_logger.LogInformation($"User {userId}")`
- Set global level to `Trace` in production
- Keep unlimited log files (`RetainedFileCountLimit: null`)
- Write sensitive data in logs
- Forget to handle exceptions when logging

## Full Documentation

See [Rapidex-Logging-Configuration.md](./Rapidex-Logging-Configuration.md) for complete documentation.
