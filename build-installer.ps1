# Chitti Installer Build Script
# This script builds the Chitti application and creates an MSI installer

Write-Host "Building Chitti Installer..." -ForegroundColor Green
Write-Host ""

# Check if WiX Toolset is installed
try {
    $candlePath = Get-Command candle.exe -ErrorAction Stop
    $lightPath = Get-Command light.exe -ErrorAction Stop
    Write-Host "WiX Toolset found at: $($candlePath.Source)" -ForegroundColor Green
} catch {
    Write-Host "WiX Toolset not found!" -ForegroundColor Red
    Write-Host "Please install WiX Toolset from https://wixtoolset.org/" -ForegroundColor Yellow
    Write-Host "Download and install WiX Toolset, then add it to your PATH." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit 1
}

# Clean previous builds
Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
if (Test-Path "bin\Release") {
    Remove-Item -Recurse -Force "bin\Release"
}
if (Test-Path "ChittiInstaller.msi") {
    Remove-Item "ChittiInstaller.msi"
}
if (Test-Path "ChittiInstaller.wixobj") {
    Remove-Item "ChittiInstaller.wixobj"
}

# Build the application in Release mode
Write-Host "Building Chitti application..." -ForegroundColor Yellow
$buildResult = dotnet build -c Release --nologo
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Compile the WiX source
Write-Host "Compiling WiX source..." -ForegroundColor Yellow
$candleResult = & candle.exe ChittiInstaller.wxs -out ChittiInstaller.wixobj
if ($LASTEXITCODE -ne 0) {
    Write-Host "WiX compilation failed!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Link the installer
Write-Host "Linking installer..." -ForegroundColor Yellow
$lightResult = & light.exe ChittiInstaller.wixobj -out ChittiInstaller.msi
if ($LASTEXITCODE -ne 0) {
    Write-Host "WiX linking failed!" -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

# Create placeholder images if they don't exist
if (-not (Test-Path "banner.bmp")) {
    Write-Host "Creating placeholder banner image..." -ForegroundColor Yellow
    # Create a simple 1x1 pixel BMP file as placeholder
    [byte[]] $bmpData = @(0x42, 0x4D, 0x3E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00)
    [System.IO.File]::WriteAllBytes("banner.bmp", $bmpData)
}

if (-not (Test-Path "dialog.bmp")) {
    Write-Host "Creating placeholder dialog image..." -ForegroundColor Yellow
    # Create a simple 1x1 pixel BMP file as placeholder
    [byte[]] $bmpData = @(0x42, 0x4D, 0x3E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00)
    [System.IO.File]::WriteAllBytes("dialog.bmp", $bmpData)
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Installer build completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output: ChittiInstaller.msi" -ForegroundColor Cyan

if (Test-Path "ChittiInstaller.msi") {
    $fileInfo = Get-Item "ChittiInstaller.msi"
    $sizeKB = [math]::Round($fileInfo.Length / 1KB, 2)
    Write-Host "Size: $sizeKB KB" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "The installer is ready for distribution." -ForegroundColor Green
Write-Host ""
Read-Host "Press Enter to exit"
