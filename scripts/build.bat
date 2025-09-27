@echo off
echo ========================================
echo Building WPF MVVM Application
echo ========================================
echo.

echo Restoring packages...
dotnet restore
echo.

echo Building Debug configuration...
dotnet build
echo.

echo Building Release configuration...
dotnet build -c Release
echo.

echo ========================================
echo Build complete!
echo.
echo To run the application:
echo   Debug:   dotnet run
echo   Release: dotnet run -c Release
echo.
echo To publish for deployment:
echo   dotnet publish -c Release -r win-x64 --self-contained true
echo ========================================
pause