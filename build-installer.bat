@echo off
echo Building Chitti Installer...
echo.

REM Check if WiX Toolset is installed
where candle.exe >nul 2>&1
if %errorlevel% neq 0 (
    echo WiX Toolset not found. Please install WiX Toolset from https://wixtoolset.org/
    echo Download and install WiX Toolset, then add it to your PATH.
    pause
    exit /b 1
)

REM Clean previous builds
echo Cleaning previous builds...
if exist "bin\Release" rmdir /s /q "bin\Release"
if exist "ChittiInstaller.msi" del "ChittiInstaller.msi"
if exist "ChittiInstaller.wixobj" del "ChittiInstaller.wixobj"

REM Build the application in Release mode
echo Building Chitti application...
dotnet build -c Release --nologo
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

REM Compile the WiX source
echo Compiling WiX source...
candle.exe ChittiInstaller.wxs -out ChittiInstaller.wixobj
if %errorlevel% neq 0 (
    echo WiX compilation failed!
    pause
    exit /b 1
)

REM Link the installer
echo Linking installer...
light.exe ChittiInstaller.wixobj -out ChittiInstaller.msi
if %errorlevel% neq 0 (
    echo WiX linking failed!
    pause
    exit /b 1
)

REM Create a simple banner and dialog images if they don't exist
if not exist "banner.bmp" (
    echo Creating placeholder banner image...
    REM Create a simple 493x58 banner image (you can replace this with a proper image)
    copy /y nul banner.bmp >nul 2>&1
)

if not exist "dialog.bmp" (
    echo Creating placeholder dialog image...
    REM Create a simple 493x312 dialog image (you can replace this with a proper image)
    copy /y nul dialog.bmp >nul 2>&1
)

echo.
echo ========================================
echo Installer build completed successfully!
echo ========================================
echo.
echo Output: ChittiInstaller.msi
echo Size: 
dir ChittiInstaller.msi | findstr "ChittiInstaller.msi"
echo.
echo The installer is ready for distribution.
echo.
pause
