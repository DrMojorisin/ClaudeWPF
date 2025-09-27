# WPFBase Project Template Guide

## 📋 Template Files for New Projects

When copying WPFBase to create a new project, use these template configurations:

### 1. **New Project Structure Template**

```
MyNewApp/
├── MyNewApp.csproj                    # Copy from WPFBase.csproj, update names
├── App.xaml                           # Copy from WPFBase, update namespaces  
├── App.xaml.cs                        # Copy from WPFBase, update namespaces
├── MainWindow.xaml                    # Copy from WPFBase, update namespaces
├── MainWindow.xaml.cs                 # Copy from WPFBase, update namespaces
├── [All WPFBase framework files]      # Copy with namespace updates
└── appsettings.json                   # Create new (template below)
```

### 2. **Project File Template (.csproj)**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
    <StartupObject>MyNewApp.App</StartupObject>
    
    <!-- Update these for your project -->
    <AssemblyTitle>My New Application</AssemblyTitle>
    <AssemblyDescription>Description of your application</AssemblyDescription>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
    <Copyright>Copyright © Your Company 2024</Copyright>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core Framework Dependencies -->
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="9.0.0" />
    
    <!-- Logging -->
    <PackageReference Include="Serilog" Version="4.1.0" />
    <PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
    <PackageReference Include="Serilog.Sinks.Debug" Version="3.0.0" />
    <PackageReference Include="Serilog.Sinks.File" Version="6.0.0" />
    
    <!-- UI Framework -->
    <PackageReference Include="Dirkster.AvalonDock" Version="4.72.1" />
    <PackageReference Include="Dirkster.AvalonDock.Themes.VS2013" Version="4.72.1" />
    <PackageReference Include="ValueConverters" Version="3.0.26" />
    <PackageReference Include="WPF-UI" Version="3.0.5" />
    
    <!-- Validation -->
    <PackageReference Include="FluentValidation" Version="11.10.0" />
    
    <!-- Testing (Development only - can be conditional) -->
    <PackageReference Include="xunit" Version="2.9.2" Condition="'$(Configuration)' == 'Debug'" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" Condition="'$(Configuration)' == 'Debug'" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" Condition="'$(Configuration)' == 'Debug'" />
    <PackageReference Include="Moq" Version="4.20.72" Condition="'$(Configuration)' == 'Debug'" />
    <PackageReference Include="coverlet.collector" Version="6.0.2" Condition="'$(Configuration)' == 'Debug'" />
  </ItemGroup>

  <!-- Optional: Exclude certain files from build -->
  <ItemGroup>
    <!-- Exclude documentation files from build -->
    <None Remove="*.md" />
    <None Remove="docs\**\*" />
    
    <!-- Include documentation as content -->
    <Content Include="CLAUDE*.md">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
    <Content Include="README.md">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>
```

### 3. **Configuration Template (appsettings.json)**

```json
{
  "ApplicationSettings": {
    "Name": "MyNewApp",
    "Version": "1.0.0",
    "Company": "Your Company Name",
    "Description": "Description of your application"
  },
  
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "LogFilePath": "logs/app.log",
    "EnableFileLogging": true,
    "MaxFileSizeMB": 10,
    "MaxLogFiles": 5
  },
  
  "Features": {
    "EnableDeveloperMode": true,
    "EnableTelemetry": false,
    "EnableCrashReporting": true,
    "EnableAutoSave": true,
    "AutoSaveIntervalMinutes": 5
  },
  
  "UI": {
    "Theme": "Dark",
    "FontFamily": "Segoe UI",
    "FontSize": 12,
    "StartMaximized": false,
    "RememberWindowState": true,
    "EnableAnimations": true
  },
  
  "Performance": {
    "EnableVirtualization": true,
    "MaxRecentFiles": 10,
    "EnableCaching": true,
    "CacheExpirationMinutes": 30
  },
  
  "Data": {
    "ConnectionString": "Data Source=app.db",
    "DatabaseProvider": "SQLite",
    "EnableMigrations": true,
    "CommandTimeout": 30
  }
}
```

### 4. **App.xaml Template**

```xml
<Application x:Class="MyNewApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             StartupUri="MainWindow.xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Framework Theme Resources -->
                <ResourceDictionary Source="Themes/LightTheme.xaml"/>
                
                <!-- Value Converters -->
                <ResourceDictionary Source="pack://application:,,,/ValueConverters;component/Converters.xaml"/>
                
                <!-- Custom Application Resources -->
                <ResourceDictionary>
                    <!-- Your custom application-wide resources here -->
                    <Style TargetType="Button" x:Key="PrimaryButton">
                        <Setter Property="Background" Value="#007ACC"/>
                        <Setter Property="Foreground" Value="White"/>
                        <Setter Property="Padding" Value="12,6"/>
                        <Setter Property="Margin" Value="5"/>
                    </Style>
                    
                    <Style TargetType="Button" x:Key="SecondaryButton">
                        <Setter Property="Background" Value="#F0F0F0"/>
                        <Setter Property="Foreground" Value="#333"/>
                        <Setter Property="Padding" Value="12,6"/>
                        <Setter Property="Margin" Value="5"/>
                    </Style>
                </ResourceDictionary>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

### 5. **MainWindow.xaml Template**

```xml
<Window x:Class="MyNewApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d"
        Title="My New Application" 
        Height="600" Width="900"
        WindowStartupLocation="CenterScreen">
    
    <Window.DataContext>
        <!-- Framework handles ViewModel injection -->
    </Window.DataContext>
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Menu Bar -->
        <Menu Grid.Row="0">
            <MenuItem Header="_File">
                <MenuItem Header="_New" Command="{Binding NewCommand}"/>
                <MenuItem Header="_Open" Command="{Binding OpenCommand}"/>
                <MenuItem Header="_Save" Command="{Binding SaveCommand}"/>
                <Separator/>
                <MenuItem Header="E_xit" Command="{Binding ExitCommand}"/>
            </MenuItem>
            <MenuItem Header="_Edit">
                <MenuItem Header="_Undo" Command="{Binding UndoCommand}"/>
                <MenuItem Header="_Redo" Command="{Binding RedoCommand}"/>
            </MenuItem>
            <MenuItem Header="_View">
                <MenuItem Header="_Theme">
                    <MenuItem Header="_Light" Command="{Binding SetLightThemeCommand}"/>
                    <MenuItem Header="_Dark" Command="{Binding SetDarkThemeCommand}"/>
                </MenuItem>
            </MenuItem>
            <MenuItem Header="_Help">
                <MenuItem Header="_About" Command="{Binding AboutCommand}"/>
            </MenuItem>
        </Menu>
        
        <!-- Main Content Area -->
        <ContentControl Grid.Row="1" 
                       Content="{Binding CurrentView}" 
                       Margin="5"/>
        
        <!-- Status Bar -->
        <StatusBar Grid.Row="2">
            <StatusBarItem>
                <TextBlock Text="{Binding StatusMessage}"/>
            </StatusBarItem>
            <StatusBarItem HorizontalAlignment="Right">
                <StackPanel Orientation="Horizontal">
                    <ProgressBar Width="100" Height="16" 
                               IsIndeterminate="{Binding IsBusy}"
                               Visibility="{Binding IsBusy, Converter={StaticResource BooleanToVisibilityConverter}}"/>
                    <TextBlock Text="{Binding ProgressMessage}" Margin="10,0,0,0"/>
                </StackPanel>
            </StatusBarItem>
        </StatusBar>
    </Grid>
</Window>
```

### 6. **AssemblyInfo Template**

```csharp
// AssemblyInfo.cs
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: AssemblyTitle("My New Application")]
[assembly: AssemblyDescription("Description of your WPF application")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Your Company Name")]
[assembly: AssemblyProduct("My New Application")]
[assembly: AssemblyCopyright("Copyright © Your Company 2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
```

## 🔧 Namespace Update Automation

### PowerShell Script for Namespace Updates

```powershell
# UpdateNamespaces.ps1
param(
    [string]$SourcePath = ".",
    [string]$OldNamespace = "WPFBase",
    [string]$NewNamespace = "MyNewApp"
)

Write-Host "Updating namespaces from '$OldNamespace' to '$NewNamespace'..."

# Update all C# files
Get-ChildItem -Path $SourcePath -Filter "*.cs" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updated = $content -replace "namespace $OldNamespace", "namespace $NewNamespace"
    $updated = $updated -replace "using $OldNamespace\.", "using $NewNamespace."
    
    if ($content -ne $updated) {
        Set-Content $_.FullName $updated -NoNewline
        Write-Host "Updated: $($_.Name)"
    }
}

# Update all XAML files
Get-ChildItem -Path $SourcePath -Filter "*.xaml" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updated = $content -replace "x:Class=""$OldNamespace\.", "x:Class=""$NewNamespace."
    $updated = $updated -replace "xmlns:local=""clr-namespace:$OldNamespace", "xmlns:local=""clr-namespace:$NewNamespace"
    
    if ($content -ne $updated) {
        Set-Content $_.FullName $updated -NoNewline
        Write-Host "Updated: $($_.Name)"
    }
}

# Update project files
Get-ChildItem -Path $SourcePath -Filter "*.csproj" -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $updated = $content -replace "<StartupObject>$OldNamespace\.App</StartupObject>", "<StartupObject>$NewNamespace.App</StartupObject>"
    
    if ($content -ne $updated) {
        Set-Content $_.FullName $updated -NoNewline
        Write-Host "Updated: $($_.Name)"
    }
}

Write-Host "Namespace update completed!"
```

### Bash Script for Namespace Updates

```bash
#!/bin/bash
# update_namespaces.sh

OLD_NAMESPACE="WPFBase"
NEW_NAMESPACE="MyNewApp"
SOURCE_PATH="."

echo "Updating namespaces from '$OLD_NAMESPACE' to '$NEW_NAMESPACE'..."

# Update C# files
find "$SOURCE_PATH" -name "*.cs" -type f | while read file; do
    sed -i "s/namespace $OLD_NAMESPACE/namespace $NEW_NAMESPACE/g" "$file"
    sed -i "s/using $OLD_NAMESPACE\./using $NEW_NAMESPACE./g" "$file"
    echo "Updated: $(basename "$file")"
done

# Update XAML files
find "$SOURCE_PATH" -name "*.xaml" -type f | while read file; do
    sed -i "s/x:Class=\"$OLD_NAMESPACE\./x:Class=\"$NEW_NAMESPACE./g" "$file"
    sed -i "s/xmlns:local=\"clr-namespace:$OLD_NAMESPACE/xmlns:local=\"clr-namespace:$NEW_NAMESPACE/g" "$file"
    echo "Updated: $(basename "$file")"
done

# Update project files
find "$SOURCE_PATH" -name "*.csproj" -type f | while read file; do
    sed -i "s/<StartupObject>$OLD_NAMESPACE\.App<\/StartupObject>/<StartupObject>$NEW_NAMESPACE.App<\/StartupObject>/g" "$file"
    echo "Updated: $(basename "$file")"
done

echo "Namespace update completed!"
```

## 📝 Project Setup Checklist

### Initial Setup
- [ ] Copy all WPFBase files to new project directory
- [ ] Rename WPFBase.csproj to YourProject.csproj
- [ ] Update project file with your application details
- [ ] Run namespace update script
- [ ] Create appsettings.json with your configuration
- [ ] Update AssemblyInfo.cs with your details

### Framework Configuration
- [ ] Update App.xaml.cs ConfigureServices with your business services
- [ ] Customize MainWindow.xaml for your application
- [ ] Add your custom themes to Themes/ directory
- [ ] Configure logging settings in appsettings.json

### Business Logic
- [ ] Add your domain models to Models/ directory
- [ ] Create your business services in Services/ directory
- [ ] Add your ViewModels following framework patterns
- [ ] Create your Views with proper data binding

### Testing
- [ ] Run framework tests to validate integration
- [ ] Add your business logic tests
- [ ] Configure test settings and mocks
- [ ] Validate all functionality works as expected

### Production Preparation
- [ ] Remove example files (HomeView, ValidationExample, etc.)
- [ ] Configure release build settings
- [ ] Set up logging for production
- [ ] Test deployment and installation

## 🎯 Quick Start Command Sequence

```bash
# 1. Copy framework
cp -r WPFBase/ MyNewApp/
cd MyNewApp/

# 2. Rename project file
mv WPFBase.csproj MyNewApp.csproj

# 3. Update namespaces
./update_namespaces.sh

# 4. Configure project
# Edit appsettings.json, AssemblyInfo.cs, etc.

# 5. Build and test
dotnet restore
dotnet build
dotnet test
dotnet run
```

Following this template ensures your new project inherits all the framework benefits while being properly configured for your specific application needs.