# UpdateNamespaces.ps1 - PowerShell script to update namespaces when copying WPFBase to a new project
param(
    [string]$SourcePath = ".",
    [string]$OldNamespace = "WPFBase", 
    [string]$NewNamespace = $(throw "Please specify -NewNamespace parameter"),
    [switch]$WhatIf = $false
)

Write-Host "WPFBase Namespace Update Script" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host "Source Path: $SourcePath"
Write-Host "Old Namespace: $OldNamespace"
Write-Host "New Namespace: $NewNamespace"
Write-Host "What-If Mode: $WhatIf"
Write-Host ""

$filesUpdated = 0
$totalChanges = 0

function Update-FileContent {
    param($FilePath, $FileType)
    
    $content = Get-Content $FilePath -Raw -ErrorAction SilentlyContinue
    if (-not $content) {
        Write-Warning "Could not read file: $FilePath"
        return
    }
    
    $originalContent = $content
    
    # Common replacements for all file types
    $content = $content -replace "namespace $OldNamespace", "namespace $NewNamespace"
    $content = $content -replace "using $OldNamespace\.", "using $NewNamespace."
    
    # File type specific replacements
    switch ($FileType) {
        "xaml" {
            $content = $content -replace "x:Class=""$OldNamespace\.", "x:Class=""$NewNamespace."
            $content = $content -replace "xmlns:local=""clr-namespace:$OldNamespace", "xmlns:local=""clr-namespace:$NewNamespace"
            $content = $content -replace "xmlns:vm=""clr-namespace:$OldNamespace", "xmlns:vm=""clr-namespace:$NewNamespace"
            $content = $content -replace "clr-namespace:$OldNamespace\.", "clr-namespace:$NewNamespace."
        }
        "csproj" {
            $content = $content -replace "<StartupObject>$OldNamespace\.App</StartupObject>", "<StartupObject>$NewNamespace.App</StartupObject>"
            $content = $content -replace "<AssemblyTitle>$OldNamespace</AssemblyTitle>", "<AssemblyTitle>$NewNamespace</AssemblyTitle>"
            $content = $content -replace "<RootNamespace>$OldNamespace</RootNamespace>", "<RootNamespace>$NewNamespace</RootNamespace>"
        }
        "cs" {
            # Additional C# specific replacements if needed
            $content = $content -replace "$OldNamespace\.", "$NewNamespace."
        }
    }
    
    if ($originalContent -ne $content) {
        $changes = ($originalContent.Split("`n").Length - ($originalContent -split $OldNamespace).Length + 1)
        
        if ($WhatIf) {
            Write-Host "WOULD UPDATE: $($FilePath | Split-Path -Leaf) [$changes changes]" -ForegroundColor Yellow
        } else {
            try {
                Set-Content $FilePath $content -NoNewline -Encoding UTF8
                Write-Host "UPDATED: $($FilePath | Split-Path -Leaf) [$changes changes]" -ForegroundColor Green
                $script:filesUpdated++
                $script:totalChanges += $changes
            } catch {
                Write-Error "Failed to update $FilePath : $($_.Exception.Message)"
            }
        }
    }
}

# Exclude certain directories and files
$excludePatterns = @(
    "*/bin/*",
    "*/obj/*", 
    "*/.git/*",
    "*/.vs/*",
    "*/packages/*",
    "*/TestResults/*"
)

Write-Host "Processing files..." -ForegroundColor Cyan

# Update C# files
Write-Host "`nProcessing C# files (.cs)..." -ForegroundColor Blue
Get-ChildItem -Path $SourcePath -Filter "*.cs" -Recurse | Where-Object {
    $file = $_.FullName
    $shouldExclude = $false
    foreach ($pattern in $excludePatterns) {
        if ($file -like $pattern) {
            $shouldExclude = $true
            break
        }
    }
    -not $shouldExclude
} | ForEach-Object {
    Update-FileContent $_.FullName "cs"
}

# Update XAML files
Write-Host "`nProcessing XAML files (.xaml)..." -ForegroundColor Blue
Get-ChildItem -Path $SourcePath -Filter "*.xaml" -Recurse | Where-Object {
    $file = $_.FullName
    $shouldExclude = $false
    foreach ($pattern in $excludePatterns) {
        if ($file -like $pattern) {
            $shouldExclude = $true
            break
        }
    }
    -not $shouldExclude
} | ForEach-Object {
    Update-FileContent $_.FullName "xaml"
}

# Update project files
Write-Host "`nProcessing project files (.csproj)..." -ForegroundColor Blue
Get-ChildItem -Path $SourcePath -Filter "*.csproj" -Recurse | Where-Object {
    $file = $_.FullName
    $shouldExclude = $false
    foreach ($pattern in $excludePatterns) {
        if ($file -like $pattern) {
            $shouldExclude = $true
            break
        }
    }
    -not $shouldExclude
} | ForEach-Object {
    Update-FileContent $_.FullName "csproj"
}

# Update solution files if present
Write-Host "`nProcessing solution files (.sln)..." -ForegroundColor Blue
Get-ChildItem -Path $SourcePath -Filter "*.sln" -Recurse | ForEach-Object {
    Update-FileContent $_.FullName "sln"
}

Write-Host ""
Write-Host "Summary:" -ForegroundColor Green
Write-Host "========" -ForegroundColor Green

if ($WhatIf) {
    Write-Host "WHAT-IF MODE: No files were actually modified" -ForegroundColor Yellow
    Write-Host "Run without -WhatIf to perform the actual updates" -ForegroundColor Yellow
} else {
    Write-Host "Files updated: $filesUpdated" -ForegroundColor Green
    Write-Host "Total changes: $totalChanges" -ForegroundColor Green
    
    if ($filesUpdated -gt 0) {
        Write-Host ""
        Write-Host "Namespace update completed successfully!" -ForegroundColor Green
        Write-Host "You should now be able to build and run your project with the new namespace." -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Cyan
        Write-Host "1. Update your .csproj file name if needed" -ForegroundColor White
        Write-Host "2. Update appsettings.json with your application details" -ForegroundColor White
        Write-Host "3. Update AssemblyInfo.cs with your project information" -ForegroundColor White
        Write-Host "4. Run 'dotnet restore' and 'dotnet build' to verify" -ForegroundColor White
    } else {
        Write-Host "No files needed updating. Either the namespace is already correct or no matching files were found." -ForegroundColor Yellow
    }
}

# Optionally rename the main project file
$oldProjectFile = Join-Path $SourcePath "$OldNamespace.csproj"
$newProjectFile = Join-Path $SourcePath "$NewNamespace.csproj"

if ((Test-Path $oldProjectFile) -and ($oldProjectFile -ne $newProjectFile)) {
    if ($WhatIf) {
        Write-Host "WOULD RENAME: $OldNamespace.csproj -> $NewNamespace.csproj" -ForegroundColor Yellow
    } else {
        try {
            Rename-Item $oldProjectFile $newProjectFile
            Write-Host "RENAMED: $OldNamespace.csproj -> $NewNamespace.csproj" -ForegroundColor Green
        } catch {
            Write-Warning "Could not rename project file: $($_.Exception.Message)"
        }
    }
}

Write-Host ""