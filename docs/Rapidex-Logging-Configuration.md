# Rapidex Logging Configuration Guide

## Overview

Rapidex provides a flexible logging infrastructure built on top of Serilog that supports both standard Serilog configuration and advanced Rapidex-specific features. The logging system is designed to be generic (using Microsoft.Extensions.Logging) while offering powerful capabilities for production environments.

## Key Features

- ✅ **Microsoft.Extensions.Logging Compatible**: Standard ILogger<T> interface
- ✅ **Dual Configuration**: Support for both Serilog standard and Rapidex custom configuration
- ✅ **Runtime Level Control**: Change log levels dynamically at runtime
- ✅ **Category-Based Filtering**: Control log levels per namespace/category
- ✅ **Separate Error/Warning Files**: Automatic separation of error and warning logs
- ✅ **Buffered Logging**: Reduce disk I/O for non-critical logs
- ✅ **Immediate Error Logging**: Critical logs written immediately without buffering
- ✅ **Category-Based File Separation**: Route specific categories to separate files

## Installation

Add the required NuGet packages to your project:

```xml
<PackageReference Include="Rapidex.Base.Common.Logging.Serilog.Core8" Version="x.x.x" />
```

Or via .NET CLI:

```bash
dotnet add package Rapidex.Base.Common.Logging.Serilog.Core8
```

## Quick Start

### 1. Basic Configuration (Code-Based)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Rapidex Serilog with default settings
builder.UseRapidexSerilog();

var app = builder.Build();
app.Run();
```

### 2. Code-Based Configuration with Options

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.UseRapidexSerilog(config =>
{
    config.LogDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Logs");
    config.LogFilePrefix = "myapp";
    config.DefaultMinimumLevel = LogLevel.Debug;
    config.UseSeperateErrorLogFile = true;
    config.UseSeperateWarningLogFile = true;
    config.UseBufferForNonErrors = true;
    config.WriteToConsole = true;
    
    // Category-specific levels
    config.CategoryMinimumLevels["Microsoft"] = LogLevel.Warning;
    config.CategoryMinimumLevels["Microsoft.AspNetCore"] = LogLevel.Warning;
    config.CategoryMinimumLevels["System"] = LogLevel.Warning;
    config.CategoryMinimumLevels["Rapidex"] = LogLevel.Debug;
});

var app = builder.Build();
app.Run();
```

### 3. Configuration from appsettings.json

**appsettings.json:**

```json
{
  "RapidexLogging": {
    "LogDirectory": "Logs",
    "LogFilePrefix": "myapp",
    "DefaultMinimumLevel": "Debug",
    "UseSeperateErrorLogFile": true,
    "UseSeperateWarningLogFile": true,
    "UseBufferForNonErrors": true,
    "BufferFlushIntervalSeconds": 5,
    "MaxLogFileSize": 104857600,
    "RetainedFileCountLimit": 31,
    "WriteToConsole": true,
    
    "CategoryMinimumLevels": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "System": "Warning",
      "System.Net.Http.HttpClient": "Warning",
      "Rapidex": "Debug",
      "MyApp.Services": "Debug"
    },
    
    "CategorySeparateFiles": {
      "MyApp.Services.CriticalService": true
    },
    
    "OutputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
  }
}
```

**Program.cs:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuration will be read from appsettings.json automatically
builder.UseRapidexSerilog();

var app = builder.Build();
app.Run();
```

## Configuration Options

### RapidexLoggingConfiguration Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `LogDirectory` | `string` | `"Logs"` | Directory where log files will be written |
| `LogFilePrefix` | `string` | `"app"` | Prefix for log file names (e.g., "app-20240101.log") |
| `DefaultMinimumLevel` | `LogLevel` | `LogLevel.Debug` | Default minimum log level |
| `UseSeperateErrorLogFile` | `bool` | `true` | Create separate file for Error level logs |
| `UseSeperateWarningLogFile` | `bool` | `true` | Create separate file for Warning level logs |
| `UseBufferForNonErrors` | `bool` | `true` | Use buffering for non-error logs (reduces disk I/O) |
| `BufferSize` | `int` | `32768` | Buffer size in bytes (32KB) |
| `BufferFlushIntervalSeconds` | `int` | `5` | Buffer flush interval in seconds |
| `MaxLogFileSize` | `long` | `104857600` | Maximum log file size in bytes (100MB) |
| `RetainedFileCountLimit` | `int?` | `31` | Maximum number of log files to keep (null = unlimited) |
| `CategoryMinimumLevels` | `Dictionary<string, LogLevel>` | `new()` | Minimum log levels per category/namespace |
| `CategorySeparateFiles` | `Dictionary<string, bool>` | `new()` | Create separate files for specific categories |
| `OutputTemplate` | `string` | See below | Serilog output template format |
| `UseStandardSerilogConfiguration` | `bool` | `false` | Use standard Serilog configuration from appsettings.json |
| `WriteToConsole` | `bool` | `false` | Write logs to console output |

**Default Output Template:**
```
[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}
```

### Log Levels

Available log levels (from `Microsoft.Extensions.Logging.LogLevel`):

- `Trace` (0): Most detailed logging
- `Debug` (1): Debug information
- `Information` (2): General informational messages
- `Warning` (3): Warning messages
- `Error` (4): Error messages
- `Critical` (5): Critical failures
- `None` (6): No logging

## Advanced Scenarios

### Scenario 1: Suppress Framework Logs, Keep Application Logs

**Problem:** Third-party libraries (Microsoft, HttpClient, MudBlazor, etc.) generate too many debug logs.

**Solution:**

```csharp
builder.UseRapidexSerilog(config =>
{
    config.DefaultMinimumLevel = LogLevel.Debug;
    
    // Suppress framework logs
    config.CategoryMinimumLevels["Microsoft"] = LogLevel.Warning;
    config.CategoryMinimumLevels["Microsoft.AspNetCore"] = LogLevel.Warning;
    config.CategoryMinimumLevels["Microsoft.EntityFrameworkCore"] = LogLevel.Warning;
    config.CategoryMinimumLevels["System"] = LogLevel.Warning;
    config.CategoryMinimumLevels["System.Net.Http.HttpClient"] = LogLevel.Warning;
    
    // Keep application logs at Debug
    config.CategoryMinimumLevels["Rapidex"] = LogLevel.Debug;
    config.CategoryMinimumLevels["MyApp"] = LogLevel.Debug;
});
```

Or in appsettings.json:

```json
{
  "RapidexLogging": {
    "DefaultMinimumLevel": "Debug",
    "CategoryMinimumLevels": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "System": "Warning",
      "System.Net.Http.HttpClient": "Warning",
      "Rapidex": "Debug",
      "MyApp": "Debug"
    }
  }
}
```

### Scenario 2: Runtime Log Level Control

**Use Case:** Change log levels at runtime without restarting the application.

```csharp
public class LogManagementController : ControllerBase
{
    private readonly ILogLevelController _logLevelController;

    public LogManagementController(ILogLevelController logLevelController)
    {
        _logLevelController = logLevelController;
    }

    [HttpPost("set-global-level")]
    public IActionResult SetGlobalLevel([FromBody] string level)
    {
        var logLevel = Enum.Parse<LogLevel>(level);
        _logLevelController.SetMinimumLevel(logLevel);
        return Ok($"Global log level set to {logLevel}");
    }

    [HttpPost("set-category-level")]
    public IActionResult SetCategoryLevel([FromBody] CategoryLevelRequest request)
    {
        var logLevel = Enum.Parse<LogLevel>(request.Level);
        _logLevelController.SetMinimumLevel(request.Category, logLevel);
        return Ok($"Log level for {request.Category} set to {logLevel}");
    }

    [HttpGet("get-global-level")]
    public IActionResult GetGlobalLevel()
    {
        var level = _logLevelController.GetMinimumLevel();
        return Ok(level.ToString());
    }

    [HttpGet("get-category-level/{category}")]
    public IActionResult GetCategoryLevel(string category)
    {
        var level = _logLevelController.GetMinimumLevel(category);
        return Ok(level?.ToString() ?? "Not set");
    }
}

public class CategoryLevelRequest
{
    public string Category { get; set; }
    public string Level { get; set; }
}
```

### Scenario 3: Critical Service Separate Logging

**Use Case:** Route logs from a critical service to a separate file for easier monitoring.

```csharp
builder.UseRapidexSerilog(config =>
{
    config.CategorySeparateFiles["MyApp.Services.PaymentService"] = true;
    config.CategorySeparateFiles["MyApp.Services.OrderProcessingService"] = true;
});
```

This will create separate log files:
- `MyApp.Services.PaymentService-20240101.log`
- `MyApp.Services.OrderProcessingService-20240101.log`

### Scenario 4: Hybrid Configuration (Standard Serilog + Rapidex Features)

**Use Case:** Use standard Serilog configuration but add Rapidex's advanced features.

**appsettings.json:**

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss}] {Message:lj}{NewLine}"
        }
      }
    ]
  },
  
  "RapidexLogging": {
    "UseStandardSerilogConfiguration": true,
    "UseSeperateErrorLogFile": true,
    "UseSeperateWarningLogFile": true,
    "LogDirectory": "Logs",
    "LogFilePrefix": "myapp"
  }
}
```

### Scenario 5: Production Configuration

**Recommended production setup with optimal performance and minimal disk I/O:**

```json
{
  "RapidexLogging": {
    "LogDirectory": "Logs",
    "LogFilePrefix": "myapp",
    "DefaultMinimumLevel": "Information",
    
    "UseSeperateErrorLogFile": true,
    "UseSeperateWarningLogFile": true,
    
    "UseBufferForNonErrors": true,
    "BufferFlushIntervalSeconds": 10,
    
    "MaxLogFileSize": 104857600,
    "RetainedFileCountLimit": 31,
    
    "CategoryMinimumLevels": {
      "Microsoft": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.Hosting": "Warning",
      "Microsoft.AspNetCore.Mvc": "Warning",
      "Microsoft.AspNetCore.Routing": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "System": "Warning",
      "System.Net.Http": "Warning"
    },
    
    "CategorySeparateFiles": {
      "MyApp.Services.Payment": true,
      "MyApp.Services.OrderProcessing": true
    },
    
    "WriteToConsole": false
  }
}
```

### Scenario 6: Development Configuration

**Recommended development setup with verbose logging:**

```json
{
  "RapidexLogging": {
    "LogDirectory": "App_Data/Logs",
    "LogFilePrefix": "dev",
    "DefaultMinimumLevel": "Debug",
    
    "UseSeperateErrorLogFile": true,
    "UseSeperateWarningLogFile": true,
    "UseBufferForNonErrors": false,
    
    "CategoryMinimumLevels": {
      "Microsoft.AspNetCore.Hosting": "Information",
      "Microsoft.AspNetCore.Mvc": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information",
      "Rapidex": "Debug",
      "MyApp": "Debug"
    },
    
    "WriteToConsole": true
  }
}
```

## Using ILogger in Your Code

### Basic Usage

```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void ProcessOrder(Order order)
    {
        _logger.LogInformation("Processing order {OrderId}", order.Id);
        
        try
        {
            // Process order...
            _logger.LogDebug("Order {OrderId} validated successfully", order.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", order.Id);
            throw;
        }
    }
}
```

### Structured Logging

```csharp
_logger.LogInformation("User {UserId} purchased {ProductCount} items for {TotalAmount:C}", 
    userId, productCount, totalAmount);

// This will be logged as:
// [2024-01-01 10:30:15.123] [INF] [MyApp.Services.OrderService] User 12345 purchased 3 items for $150.00
```

### Log Levels Guidelines

| Level | When to Use | Example |
|-------|-------------|---------|
| `Trace` | Very detailed diagnostic info | Variable values in loops |
| `Debug` | Development debugging | Method entry/exit, intermediate values |
| `Information` | General application flow | User logged in, order created |
| `Warning` | Unexpected but recoverable | Using deprecated API, retrying operation |
| `Error` | Error that doesn't crash app | Failed to send email, database timeout |
| `Critical` | Critical failures | Database unavailable, out of memory |

## Log File Structure

With default configuration, the following log files will be created:

```
Logs/
├── app-20240101.log           # All logs (buffered)
├── app-error-20240101.log     # Error and Critical logs only (immediate)
├── app-warning-20240101.log   # Warning, Error, and Critical logs (immediate)
└── MyApp.Services.Payment-20240101.log  # Category-specific logs (if configured)
```

### File Naming Convention

- **Main log**: `{LogFilePrefix}-{Date}.log`
- **Error log**: `{LogFilePrefix}-error-{Date}.log`
- **Warning log**: `{LogFilePrefix}-warning-{Date}.log`
- **Category log**: `{CategoryName}-{Date}.log`

### File Rotation

- Files are rotated daily (based on `RollingInterval.Day`)
- Old files are kept according to `RetainedFileCountLimit`
- Files are split when reaching `MaxLogFileSize`

## Performance Considerations

### Buffering Strategy

| Log Level | Buffering | Flush Behavior |
|-----------|-----------|----------------|
| Trace, Debug, Information | Yes (if `UseBufferForNonErrors = true`) | Every 5 seconds (default) |
| Warning | No | Immediate write |
| Error, Critical | No | Immediate write |

This ensures:
- ✅ Critical logs are never lost
- ✅ Reduced disk I/O for routine logs
- ✅ Better application performance

### Recommended Buffer Settings

| Environment | BufferFlushIntervalSeconds | UseBufferForNonErrors |
|-------------|---------------------------|----------------------|
| Development | 1-2 | false |
| Staging | 5 | true |
| Production | 10 | true |

## Integration with Other Rapidex Components

### Using with Rapidex.Data

```csharp
var builder = WebApplication.CreateBuilder(args);

// Setup logging first
builder.UseRapidexSerilog(config =>
{
    config.DefaultMinimumLevel = LogLevel.Debug;
    config.CategoryMinimumLevels["Rapidex.Data"] = LogLevel.Debug;
    config.CategoryMinimumLevels["Microsoft.EntityFrameworkCore"] = LogLevel.Warning;
});

// Then setup Rapidex.Data
builder.Services.AddRapidexDataLevel();

var app = builder.Build();
app.Services.StartRapidexDataLevel();
app.Run();
```

## Troubleshooting

### Problem: Logs not appearing

**Check:**
1. Log level configuration (might be too high)
2. File permissions on log directory
3. Category overrides blocking your logs

**Solution:**
```csharp
// Temporarily set to Trace to see all logs
config.DefaultMinimumLevel = LogLevel.Trace;
config.WriteToConsole = true;
```

### Problem: Too many logs from frameworks

**Solution:** Add category-specific filters:
```csharp
config.CategoryMinimumLevels["Microsoft"] = LogLevel.Warning;
config.CategoryMinimumLevels["System"] = LogLevel.Warning;
```

### Problem: Log files too large

**Solution:** Adjust file size limits:
```csharp
config.MaxLogFileSize = 50 * 1024 * 1024; // 50MB
config.RetainedFileCountLimit = 7; // Keep only 7 days
```

### Problem: Logs not flushed immediately

**Solution:** Disable buffering or reduce flush interval:
```csharp
config.UseBufferForNonErrors = false;
// OR
config.BufferFlushIntervalSeconds = 1;
```

## Migration Guide

### From int-based to LogLevel-based Configuration

**Old Code:**
```csharp
config.DefaultMinimumLevel = 1; // Debug
config.CategoryMinimumLevels["Microsoft"] = 3; // Warning
```

**New Code:**
```csharp
config.DefaultMinimumLevel = LogLevel.Debug;
config.CategoryMinimumLevels["Microsoft"] = LogLevel.Warning;
```

### From appsettings.json (Old):
```json
{
  "RapidexLogging": {
    "DefaultMinimumLevel": 1,
    "CategoryMinimumLevels": {
      "Microsoft": 3
    }
  }
}
```

**To appsettings.json (New):**
```json
{
  "RapidexLogging": {
    "DefaultMinimumLevel": "Debug",
    "CategoryMinimumLevels": {
      "Microsoft": "Warning"
    }
  }
}
```

## Complete Example

Here's a complete example combining all features:

**Program.cs:**
```csharp
using Microsoft.Extensions.Logging;
using Rapidex;
using Rapidex.Base.Common.Logging.Serilog.Core8;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.UseRapidexSerilog(config =>
{
    config.LogDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Logs");
    config.LogFilePrefix = "myapp";
    config.DefaultMinimumLevel = LogLevel.Information;
    
    // Performance optimization
    config.UseBufferForNonErrors = true;
    config.BufferFlushIntervalSeconds = 10;
    
    // Separate critical logs
    config.UseSeperateErrorLogFile = true;
    config.UseSeperateWarningLogFile = true;
    
    // Framework noise reduction
    config.CategoryMinimumLevels["Microsoft"] = LogLevel.Warning;
    config.CategoryMinimumLevels["Microsoft.AspNetCore"] = LogLevel.Warning;
    config.CategoryMinimumLevels["System"] = LogLevel.Warning;
    
    // Application debug logging
    config.CategoryMinimumLevels["MyApp"] = LogLevel.Debug;
    
    // Critical service separate logging
    config.CategorySeparateFiles["MyApp.Services.Payment"] = true;
    
    // Development console output
    if (builder.Environment.IsDevelopment())
    {
        config.WriteToConsole = true;
        config.DefaultMinimumLevel = LogLevel.Debug;
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## Best Practices

1. **Use structured logging**: Always use structured logging with named parameters
   ```csharp
   _logger.LogInformation("User {UserId} logged in", userId); // ✅ Good
   _logger.LogInformation($"User {userId} logged in"); // ❌ Bad
   ```

2. **Set appropriate levels per environment**:
   - Development: `Debug`
   - Staging: `Information`
   - Production: `Information` or `Warning`

3. **Use category filters aggressively**: Framework logs can be overwhelming
   ```csharp
   config.CategoryMinimumLevels["Microsoft"] = LogLevel.Warning;
   config.CategoryMinimumLevels["System"] = LogLevel.Warning;
   ```

4. **Enable buffering in production**: Reduces disk I/O significantly
   ```csharp
   config.UseBufferForNonErrors = true;
   config.BufferFlushIntervalSeconds = 10;
   ```

5. **Separate critical service logs**: Makes troubleshooting easier
   ```csharp
   config.CategorySeparateFiles["MyApp.Services.Payment"] = true;
   ```

6. **Use runtime control for troubleshooting**: Inject `ILogLevelController` to change levels without restart

7. **Manage log retention**: Don't let logs consume all disk space
   ```csharp
   config.RetainedFileCountLimit = 31; // Keep 31 days
   config.MaxLogFileSize = 100 * 1024 * 1024; // 100MB per file
   ```

## Additional Resources

- [Serilog Documentation](https://github.com/serilog/serilog/wiki)
- [Microsoft.Extensions.Logging Documentation](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging)
- [Rapidex.Data Documentation](https://github.com/ArionWM/Rapidex.Data)

## Support

For issues, questions, or contributions, please visit:
- GitHub: https://github.com/ArionWM/Rapidex.Data
- Issues: https://github.com/ArionWM/Rapidex.Data/issues
