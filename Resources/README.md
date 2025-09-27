# Resources

This folder contains static resources used by the application.

## Purpose
- Store images, icons, and other static assets
- Organize resources for easy access
- Support theming and localization

## Folder Structure
- `/Icons` - Application icons and UI icons
- `/Images` - Pictures, logos, background images
- `/Sounds` - Audio files for notifications
- `/Localization` - Language resource files (if applicable)

## Current Contents
- `/Icons/app.ico` - Main application icon

## Usage in XAML

### Icons and Images
```xml
<!-- Pack URI for embedded resources -->
<Image Source="pack://application:,,,/Resources/Icons/save.png" />

<!-- Using resource dictionary -->
<Image Source="{StaticResource SaveIcon}" />
```

### Resource Dictionary Registration
Add to App.xaml or theme files:

```xml
<Application.Resources>
    <ResourceDictionary>
        <BitmapImage x:Key="SaveIcon" UriSource="pack://application:,,,/Resources/Icons/save.png" />
    </ResourceDictionary>
</Application.Resources>
```

## Best Practices

### File Organization
- Use descriptive file names
- Group by functionality or size
- Include multiple sizes for icons (16x16, 32x32, etc.)

### Performance
- Optimize image sizes for UI use
- Use vector formats (SVG) when possible
- Consider lazy loading for large images

### Theming Support
- Use consistent naming conventions
- Provide light/dark variants when needed
- Store theme-specific resources in theme folders

## Build Action Settings
Ensure resources have correct build actions:
- **Resource** - Embedded in assembly (most common)
- **Content** - Separate files (larger assets)
- **None** - Not included in build