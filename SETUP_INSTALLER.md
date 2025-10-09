# Chitti Installer Setup Guide

This guide will help you set up the environment needed to build the Chitti installer.

## 🛠️ Prerequisites Installation

### 1. Install WiX Toolset

#### Option A: Download from Official Website
1. Go to https://wixtoolset.org/releases/
2. Download **WiX Toolset v3.11.2** (latest stable)
3. Run the installer: `wix3112.exe`
4. Follow the installation wizard
5. **Important**: Add WiX to your system PATH

#### Option B: Install via Chocolatey (if you have it)
```cmd
choco install wixtoolset
```

#### Option C: Install via winget
```cmd
winget install Microsoft.WiXToolset
```

### 2. Verify Installation
Open a new Command Prompt and run:
```cmd
candle.exe -?
light.exe -?
```

If you see help text, WiX is properly installed.

### 3. Add to PATH (if needed)
If the commands above don't work, add WiX to your PATH:
1. Open System Properties → Advanced → Environment Variables
2. Edit the "Path" variable
3. Add: `C:\Program Files (x86)\WiX Toolset v3.11\bin`
4. Restart Command Prompt

## 🚀 Building the Installer

### Method 1: Automated Build (Recommended)
```cmd
# Run the batch script
build-installer.bat

# Or run the PowerShell script
.\build-installer.ps1
```

### Method 2: Manual Build
```cmd
# 1. Build the application
dotnet build -c Release

# 2. Compile WiX source
candle.exe ChittiInstaller.wxs -out ChittiInstaller.wixobj

# 3. Link the installer
light.exe ChittiInstaller.wixobj -out ChittiInstaller.msi
```

## 📁 File Structure

After setup, your project should have:
```
Chitti_2.0/
├── ChittiInstaller.wxs          # WiX source file
├── license.rtf                  # License agreement
├── banner.bmp                   # Installer banner (493x58)
├── dialog.bmp                   # Installer dialog (493x312)
├── build-installer.bat          # Windows build script
├── build-installer.ps1          # PowerShell build script
├── create-images.ps1            # Image creation script
├── INSTALLER_README.md          # Installer documentation
├── DISTRIBUTION_GUIDE.md        # Distribution guide
├── SETUP_INSTALLER.md           # This file
└── bin/Release/net8.0-windows/  # Built application files
    ├── Chitti.exe
    ├── Chitti.dll
    └── ... (other files)
```

## 🔧 Troubleshooting

### Common Issues

#### "candle.exe is not recognized"
- WiX Toolset not installed or not in PATH
- Solution: Install WiX and add to PATH

#### "WiX compilation failed"
- Check file paths in ChittiInstaller.wxs
- Ensure all source files exist
- Verify .NET 8.0 SDK is installed

#### "WiX linking failed"
- Check for missing files
- Verify all components are properly defined
- Check for duplicate GUIDs

#### "Build failed"
- Ensure .NET 8.0 SDK is installed
- Check that the application builds successfully
- Verify all dependencies are available

### Debug Mode
For detailed error information:
```cmd
candle.exe -v ChittiInstaller.wxs -out ChittiInstaller.wixobj
light.exe -v ChittiInstaller.wixobj -out ChittiInstaller.msi
```

## 📋 Pre-Build Checklist

Before building the installer:

- [ ] WiX Toolset installed and in PATH
- [ ] .NET 8.0 SDK installed
- [ ] Chitti application builds successfully
- [ ] All required files exist:
  - [ ] chitti.ico
  - [ ] chitti.png
  - [ ] personality.json
  - [ ] All .dll files in bin/Release/net8.0-windows/
- [ ] Placeholder images created (banner.bmp, dialog.bmp)

## 🎯 Next Steps

After successful build:

1. **Test the Installer**
   - Install on clean Windows VM
   - Test all features
   - Verify shortcuts work
   - Test uninstaller

2. **Code Signing** (for distribution)
   - Get code signing certificate
   - Sign the MSI file
   - Verify signature

3. **Distribution**
   - Upload to GitHub Releases
   - Share on social media
   - Submit to package managers

## 📞 Support

If you encounter issues:

1. **Check the logs** - WiX provides detailed error messages
2. **Verify prerequisites** - Ensure all tools are installed
3. **Check file paths** - Ensure all source files exist
4. **GitHub Issues** - Report bugs at https://github.com/Pon-Dinesh-kumar/Chitti_2.0/issues

## 🎉 Success!

Once you see "Installer build completed successfully!", you have:
- ✅ A professional MSI installer
- ✅ Ready for worldwide distribution
- ✅ Proper installation/uninstallation
- ✅ System integration
- ✅ Legal compliance

**Your Chitti installer is ready to go! 🚀**
