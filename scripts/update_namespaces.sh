#!/bin/bash
# update_namespaces.sh - Bash script to update namespaces when copying WPFBase to a new project

# Default values
OLD_NAMESPACE="WPFBase"
NEW_NAMESPACE=""
SOURCE_PATH="."
WHATIF=false

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Function to display usage
usage() {
    echo "Usage: $0 -n NEW_NAMESPACE [-o OLD_NAMESPACE] [-p SOURCE_PATH] [--whatif]"
    echo "  -n NEW_NAMESPACE    : The new namespace to replace with (required)"
    echo "  -o OLD_NAMESPACE    : The old namespace to replace (default: WPFBase)"
    echo "  -p SOURCE_PATH      : Path to search for files (default: current directory)"
    echo "  --whatif           : Show what would be changed without making changes"
    echo ""
    echo "Example:"
    echo "  $0 -n MyNewApp"
    echo "  $0 -n MyCompany.MyApp -o WPFBase -p ./src --whatif"
    exit 1
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -n|--new-namespace)
            NEW_NAMESPACE="$2"
            shift 2
            ;;
        -o|--old-namespace)
            OLD_NAMESPACE="$2"
            shift 2
            ;;
        -p|--path)
            SOURCE_PATH="$2"
            shift 2
            ;;
        --whatif)
            WHATIF=true
            shift
            ;;
        -h|--help)
            usage
            ;;
        *)
            echo "Unknown option $1"
            usage
            ;;
    esac
done

# Validate required parameters
if [ -z "$NEW_NAMESPACE" ]; then
    echo -e "${RED}Error: New namespace is required${NC}"
    usage
fi

if [ ! -d "$SOURCE_PATH" ]; then
    echo -e "${RED}Error: Source path '$SOURCE_PATH' does not exist${NC}"
    exit 1
fi

echo -e "${GREEN}WPFBase Namespace Update Script${NC}"
echo -e "${GREEN}================================${NC}"
echo -e "Source Path: ${CYAN}$SOURCE_PATH${NC}"
echo -e "Old Namespace: ${CYAN}$OLD_NAMESPACE${NC}"
echo -e "New Namespace: ${CYAN}$NEW_NAMESPACE${NC}"
echo -e "What-If Mode: ${CYAN}$WHATIF${NC}"
echo ""

FILES_UPDATED=0
TOTAL_CHANGES=0

# Function to update file content
update_file_content() {
    local file_path="$1"
    local file_type="$2"
    local temp_file="${file_path}.tmp"
    
    if [ ! -f "$file_path" ]; then
        echo -e "${YELLOW}Warning: File not found: $file_path${NC}"
        return
    fi
    
    # Create temporary file for changes
    cp "$file_path" "$temp_file"
    
    # Common replacements for all file types
    sed -i "s/namespace $OLD_NAMESPACE/namespace $NEW_NAMESPACE/g" "$temp_file"
    sed -i "s/using $OLD_NAMESPACE\./using $NEW_NAMESPACE./g" "$temp_file"
    
    # File type specific replacements
    case "$file_type" in
        "xaml")
            sed -i "s/x:Class=\"$OLD_NAMESPACE\./x:Class=\"$NEW_NAMESPACE./g" "$temp_file"
            sed -i "s/xmlns:local=\"clr-namespace:$OLD_NAMESPACE/xmlns:local=\"clr-namespace:$NEW_NAMESPACE/g" "$temp_file"
            sed -i "s/xmlns:vm=\"clr-namespace:$OLD_NAMESPACE/xmlns:vm=\"clr-namespace:$NEW_NAMESPACE/g" "$temp_file"
            sed -i "s/clr-namespace:$OLD_NAMESPACE\./clr-namespace:$NEW_NAMESPACE./g" "$temp_file"
            ;;
        "csproj")
            sed -i "s/<StartupObject>$OLD_NAMESPACE\.App<\/StartupObject>/<StartupObject>$NEW_NAMESPACE.App<\/StartupObject>/g" "$temp_file"
            sed -i "s/<AssemblyTitle>$OLD_NAMESPACE<\/AssemblyTitle>/<AssemblyTitle>$NEW_NAMESPACE<\/AssemblyTitle>/g" "$temp_file"
            sed -i "s/<RootNamespace>$OLD_NAMESPACE<\/RootNamespace>/<RootNamespace>$NEW_NAMESPACE<\/RootNamespace>/g" "$temp_file"
            ;;
        "cs")
            sed -i "s/$OLD_NAMESPACE\./$NEW_NAMESPACE./g" "$temp_file"
            ;;
    esac
    
    # Check if file was modified
    if ! diff -q "$file_path" "$temp_file" > /dev/null 2>&1; then
        local changes=$(diff "$file_path" "$temp_file" | grep -c "^[<>]" || echo "0")
        local filename=$(basename "$file_path")
        
        if [ "$WHATIF" = true ]; then
            echo -e "${YELLOW}WOULD UPDATE: $filename [$changes changes]${NC}"
        else
            if mv "$temp_file" "$file_path"; then
                echo -e "${GREEN}UPDATED: $filename [$changes changes]${NC}"
                ((FILES_UPDATED++))
                ((TOTAL_CHANGES += changes))
            else
                echo -e "${RED}ERROR: Failed to update $filename${NC}"
            fi
        fi
    else
        rm -f "$temp_file"
    fi
    
    # Clean up temp file if it still exists
    [ -f "$temp_file" ] && rm -f "$temp_file"
}

# Directories to exclude from processing
EXCLUDE_DIRS=("bin" "obj" ".git" ".vs" "packages" "TestResults" "node_modules")

# Function to check if path should be excluded
should_exclude() {
    local file_path="$1"
    for exclude_dir in "${EXCLUDE_DIRS[@]}"; do
        if [[ "$file_path" == *"/$exclude_dir/"* ]] || [[ "$file_path" == *"\\$exclude_dir\\"* ]]; then
            return 0  # true - should exclude
        fi
    done
    return 1  # false - should not exclude
}

echo -e "${CYAN}Processing files...${NC}"

# Update C# files
echo -e "\n${BLUE}Processing C# files (.cs)...${NC}"
while IFS= read -r -d '' file; do
    if ! should_exclude "$file"; then
        update_file_content "$file" "cs"
    fi
done < <(find "$SOURCE_PATH" -name "*.cs" -type f -print0)

# Update XAML files  
echo -e "\n${BLUE}Processing XAML files (.xaml)...${NC}"
while IFS= read -r -d '' file; do
    if ! should_exclude "$file"; then
        update_file_content "$file" "xaml"
    fi
done < <(find "$SOURCE_PATH" -name "*.xaml" -type f -print0)

# Update project files
echo -e "\n${BLUE}Processing project files (.csproj)...${NC}"
while IFS= read -r -d '' file; do
    if ! should_exclude "$file"; then
        update_file_content "$file" "csproj"
    fi
done < <(find "$SOURCE_PATH" -name "*.csproj" -type f -print0)

# Update solution files if present
echo -e "\n${BLUE}Processing solution files (.sln)...${NC}"
while IFS= read -r -d '' file; do
    if ! should_exclude "$file"; then
        update_file_content "$file" "sln"
    fi
done < <(find "$SOURCE_PATH" -name "*.sln" -type f -print0)

echo ""
echo -e "${GREEN}Summary:${NC}"
echo -e "${GREEN}========${NC}"

if [ "$WHATIF" = true ]; then
    echo -e "${YELLOW}WHAT-IF MODE: No files were actually modified${NC}"
    echo -e "${YELLOW}Run without --whatif to perform the actual updates${NC}"
else
    echo -e "${GREEN}Files updated: $FILES_UPDATED${NC}"
    echo -e "${GREEN}Total changes: $TOTAL_CHANGES${NC}"
    
    if [ $FILES_UPDATED -gt 0 ]; then
        echo ""
        echo -e "${GREEN}Namespace update completed successfully!${NC}"
        echo -e "${GREEN}You should now be able to build and run your project with the new namespace.${NC}"
        echo ""
        echo -e "${CYAN}Next steps:${NC}"
        echo -e "1. Update your .csproj file name if needed"
        echo -e "2. Update appsettings.json with your application details"
        echo -e "3. Update AssemblyInfo.cs with your project information"
        echo -e "4. Run 'dotnet restore' and 'dotnet build' to verify"
    else
        echo -e "${YELLOW}No files needed updating. Either the namespace is already correct or no matching files were found.${NC}"
    fi
fi

# Optionally rename the main project file
OLD_PROJECT_FILE="$SOURCE_PATH/$OLD_NAMESPACE.csproj"
NEW_PROJECT_FILE="$SOURCE_PATH/$NEW_NAMESPACE.csproj"

if [ -f "$OLD_PROJECT_FILE" ] && [ "$OLD_PROJECT_FILE" != "$NEW_PROJECT_FILE" ]; then
    if [ "$WHATIF" = true ]; then
        echo -e "${YELLOW}WOULD RENAME: $OLD_NAMESPACE.csproj -> $NEW_NAMESPACE.csproj${NC}"
    else
        if mv "$OLD_PROJECT_FILE" "$NEW_PROJECT_FILE" 2>/dev/null; then
            echo -e "${GREEN}RENAMED: $OLD_NAMESPACE.csproj -> $NEW_NAMESPACE.csproj${NC}"
        else
            echo -e "${YELLOW}Warning: Could not rename project file${NC}"
        fi
    fi
fi

echo ""