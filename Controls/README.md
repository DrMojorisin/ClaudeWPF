# Controls

This folder contains custom WPF UserControls and custom controls for reuse across the application.

## Purpose
- Reusable UI components
- Custom controls with specialized behavior
- User controls that encapsulate common UI patterns

## Files Expected
- `ValidationSummary.xaml/.cs` - Control to display validation errors
- Other custom controls

## Usage
Custom controls can be used in XAML like any other WPF control:

```xml
<local:ValidationSummary ValidationErrors="{Binding Errors}" />
```

## Creating New Controls
1. Add new UserControl to this folder
2. Define any dependency properties
3. Add styling in Themes folder if needed