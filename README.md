# Avae.Windows.Essentials

A port of Microsoft.Maui.Essentials tailored for Avalonia.Windows.

# Features

Cross-Platform Essentials: Leverage APIs from Microsoft.Maui.Essentials adapted for windows environments.

MIT Licensed: Freely use, modify, and distribute under the permissive MIT License.

# Getting Started

Follow these steps to integrate Avae.Windows.Essentials into your Avalonia.Windows project.

# Prerequisites

An Avalonia.Windows project set up with .NET. targetting net8.0-windows10.0.19041.0

# Installation

1. Add Microsoft.Maui.Essentials to Your Shared Project

In your shared project’s .csproj file, include the Microsoft.Maui.Essentials package. Use one of the following methods
````
<UseMauiEssentials>true</UseMauiEssentials>
````
OR
````
<PackageReference Include="Microsoft.Maui.Essentials">
    <PrivateAssets>all</PrivateAssets>
</PackageReference>
````

# Configuration

1. Initialize 

````
 public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .AfterSetup(builder =>
            {
                Microsoft.Maui.ApplicationModel.Platform.Initialize();
            })
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
````

# Usage

Once installed, you can use Microsoft.Maui.Essentials APIs within your Avalonia.Windows application. For example, access application information via AppInfo.

# Example: Accessing AppInfo
````
using Microsoft.Maui.Essentials;

string appName = AppInfo.Name;
string appVersion = AppInfo.VersionString;
````
# Built With

This package builds upon the excellent work of:

Microsoft.Maui.Essentials

AvaloniaUI

# License

Avae.Windows.Essentials is licensed under the MIT License.

# Contributing

Contributions are welcome! Please submit issues or pull requests to the GitHub repository. Ensure your code follows the project’s coding standards.

# Acknowledgments

Thanks to the Avalonia team for their robust UI framework.

Gratitude to the MAUI team for providing cross-platform essentials.