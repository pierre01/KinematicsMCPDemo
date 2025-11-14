# .NET 10.0 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that an .NET 10.0 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10.0 upgrade.
3. Upgrade Biosero.Kinematics.Common\Biosero.Kinematics.Common.csproj
4. Upgrade RobotMcpClient\RobotMcpClient.csproj
5. Upgrade KinematicsDemo\KinematicsDemo.csproj


## Settings

This section contains settings and data used by execution steps.

### Excluded projects

Table below contains projects that do belong to the dependency graph for selected projects and should not be included in the upgrade.

| Project name                                   | Description                 |
|:-----------------------------------------------|:---------------------------:|


### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                             | Current Version            | New Version | Description                                   |
|:-----------------------------------------|:-------------------------:|:-----------:|:----------------------------------------------|
| Microsoft.Extensions.DependencyInjection | 9.0.10                    | 10.0.0      | Replace with Microsoft.Extensions.DependencyInjection 10.0.0 |
| Microsoft.Extensions.Hosting             | 9.0.10                    | 10.0.0      | Replace with Microsoft.Extensions.Hosting 10.0.0 |
| Microsoft.Extensions.Hosting.Abstractions| 9.0.10                    | 10.0.0      | Replace with Microsoft.Extensions.Hosting.Abstractions 10.0.0 |
| Microsoft.Extensions.Logging.Debug       | 9.0.10                    | 10.0.0      | Replace with Microsoft.Extensions.Logging.Debug 10.0.0 |
| Microsoft.Xaml.Behaviors.Wpf             | 1.1.135                   | 1.1.39      | Incompatible with .NET 10.0 — use 1.1.39 as recommended |
| SkiaSharp.Views.WPF                       | 3.119.2-preview.1         | 2.88.9      | Incompatible preview package — use 2.88.9 as recommended |


### Project upgrade details
This section contains details about each project upgrade and modifications that need to be done in the project.

#### Biosero.Kinematics.Common\Biosero.Kinematics.Common.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0` to `net10.0`

NuGet packages changes:
  - No NuGet package changes were reported for this project.

Feature upgrades:
  - None.

Other changes:
  - None.

#### RobotMcpClient\RobotMcpClient.csproj modifications

Project properties changes:
  - Target frameworks should be changed from `net9.0-android;net9.0-ios;net9.0-maccatalyst;net9.0-windows10.0.19041.0` to `net9.0-android;net9.0-ios;net9.0-maccatalyst;net9.0-windows10.0.19041.0;net10.0-windows` (add `net10.0-windows`).

NuGet packages changes:
  - `Microsoft.Extensions.Logging.Debug` should be updated from `9.0.10` to `10.0.0` (replace with package v10).

Feature upgrades:
  - Ensure any platform-specific code for windows target is compatible with .NET 10.0-windows.

Other changes:
  - None.

#### KinematicsDemo\KinematicsDemo.csproj modifications

Project properties changes:
  - Target framework should be changed from `net9.0-windows` to `net10.0-windows`.

NuGet packages changes:
  - `Microsoft.Xaml.Behaviors.Wpf` should be changed from `1.1.135` to `1.1.39` (incompatible — recommended downgrade).
  - `SkiaSharp.Views.WPF` should be changed from `3.119.2-preview.1` to `2.88.9` (incompatible preview package — recommended fallback).
  - `Microsoft.Extensions.DependencyInjection` should be updated from `9.0.10` to `10.0.0` (replace with package v10).
  - `Microsoft.Extensions.Hosting` should be updated from `9.0.10` to `10.0.0` (replace with package v10).
  - `Microsoft.Extensions.Hosting.Abstractions` should be updated from `9.0.10` to `10.0.0` (replace with package v10).

Feature upgrades:
  - Review any APIs or behaviors from the Microsoft.Extensions.* packages that may have breaking changes in v10.

Other changes:
  - Verify WPF-specific behaviors and third-party libraries (SkiaSharp, Behaviors) for compatibility after package changes.
