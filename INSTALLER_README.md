# Chitti Installer

This directory contains the files needed to create a professional Windows installer for Chitti.

## Files

- `ChittiInstaller.wxs` - WiX source file defining the installer
- `license.rtf` - Software license agreement
- `build-installer.bat` - Windows batch script to build the installer
- `build-installer.ps1` - PowerShell script to build the installer
- `banner.bmp` - Installer banner image (493x58 pixels)
- `dialog.bmp` - Installer dialog image (493x312 pixels)

## Prerequisites

1. **WiX Toolset** - Download and install from https://wixtoolset.org/
   - Add WiX Toolset to your system PATH
   - Required tools: `candle.exe` and `light.exe`

2. **.NET 8.0 SDK** - Already required for building Chitti

3. **Visual Studio** (optional) - For WiX Visual Studio extension

## Building the Installer

### Method 1: Batch Script (Windows)
```cmd
build-installer.bat
```

### Method 2: PowerShell Script
```powershell
.\build-installer.ps1
```

### Method 3: Manual Build
```cmd
# Build the application
dotnet build -c Release

# Compile WiX source
candle.exe ChittiInstaller.wxs -out ChittiInstaller.wixobj

# Link the installer
light.exe ChittiInstaller.wixobj -out ChittiInstaller.msi
```

## Installer Features

### What Gets Installed
- **Main Application**: Chitti.exe and all dependencies
- **Runtime Files**: .NET 8.0 runtime components
- **Resources**: Icons, images, and configuration files
- **Shortcuts**: Desktop and Start Menu shortcuts
- **Registry Entries**: Installation information and uninstall data

### Installation Locations
- **Program Files**: `C:\Program Files\Chitti\`
- **Start Menu**: `Start Menu\Programs\Chitti\`
- **Desktop**: Desktop shortcut (optional)

### System Requirements
- **OS**: Windows 10 or later (x64)
- **.NET**: .NET 8.0 Runtime (checked during installation)
- **Architecture**: 64-bit

### Installer Properties
- **Product Name**: Chitti - AI Assistant
- **Version**: 0.1.0
- **Manufacturer**: Pon Dinesh kumar M
- **Language**: English (1033)
- **Platform**: x64

## Customization

### Changing Version
Update the version in `ChittiInstaller.wxs`:
```xml
<Property Id="ProductVersion" Value="0.1.0" />
```

### Adding Files
Add new files to the `MainExecutable` component:
```xml
<File Id="NewFile" 
      Name="NewFile.txt" 
      Source="path\to\NewFile.txt" />
```

### Modifying Shortcuts
Update shortcut properties in the `Shortcut` elements:
```xml
<Shortcut Id="DesktopShortcut" 
          Directory="DesktopFolder" 
          Name="Chitti" 
          WorkingDirectory="INSTALLFOLDER" 
          Icon="icon.ico" 
          IconIndex="0" 
          Advertise="yes" />
```

### Custom Images
Replace the placeholder images:
- `banner.bmp` - 493x58 pixels, installer banner
- `dialog.bmp` - 493x312 pixels, installer dialog background

## Distribution

### Code Signing (Recommended)
For worldwide distribution, consider code signing the installer:

1. **Get a Code Signing Certificate** from a trusted CA
2. **Sign the MSI** using `signtool.exe`:
   ```cmd
   signtool sign /f certificate.pfx /p password ChittiInstaller.msi
   ```

### Virus Scanning
Before distribution:
1. Upload to VirusTotal for scanning
2. Test on clean Windows VMs
3. Ensure no false positives

### Distribution Platforms
- **GitHub Releases**: Upload MSI to GitHub releases
- **Microsoft Store**: Submit for Microsoft Store (requires additional steps)
- **Direct Download**: Host on your website
- **Package Managers**: Consider Chocolatey or winget

## Troubleshooting

### Common Issues

1. **WiX Toolset Not Found**
   - Install WiX Toolset from official website
   - Add to system PATH
   - Restart command prompt

2. **Build Failures**
   - Ensure .NET 8.0 SDK is installed
   - Check file paths in ChittiInstaller.wxs
   - Verify all source files exist

3. **Installer Issues**
   - Test on clean Windows VMs
   - Check Windows Event Log for errors
   - Verify .NET 8.0 runtime is installed

### Debug Mode
For debugging, add verbose output:
```cmd
candle.exe -v ChittiInstaller.wxs -out ChittiInstaller.wixobj
light.exe -v ChittiInstaller.wixobj -out ChittiInstaller.msi
```

## Security Considerations

- **Digital Signing**: Always sign installers for distribution
- **Virus Scanning**: Scan before distribution
- **Clean Builds**: Use clean VMs for testing
- **Dependencies**: Ensure all dependencies are legitimate

## Support

For installer issues:
- Check WiX Toolset documentation
- Review Windows Installer logs
- Test on different Windows versions
- Contact: [GitHub Issues](https://github.com/Pon-Dinesh-kumar/Chitti_2.0/issues)

## License

This installer is part of the Chitti project and follows the same MIT License.
