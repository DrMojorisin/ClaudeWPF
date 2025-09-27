# Scripts

This folder contains utility scripts for development and deployment.

## Available Scripts

### Development Scripts
- `run.bat` - Quick run script for Windows
- `UpdateNamespaces.ps1` - PowerShell script to update namespace references
- `update_namespaces.sh` - Bash script to update namespace references

## Usage

### Windows
```cmd
# Run the application
scripts\run.bat

# Update namespaces
powershell -ExecutionPolicy Bypass -File scripts\UpdateNamespaces.ps1
```

### Linux/macOS
```bash
# Update namespaces
bash scripts/update_namespaces.sh
```

## Script Purposes

### Namespace Update Scripts
These scripts help when renaming the project or updating namespace references throughout the codebase:
- Updates .cs files
- Updates .xaml files
- Updates project references
- Updates documentation

Run these after:
- Cloning the project with a new name
- Changing the root namespace
- Updating project structure