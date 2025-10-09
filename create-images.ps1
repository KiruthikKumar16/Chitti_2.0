# Create placeholder BMP images for the installer
Write-Host "Creating placeholder installer images..." -ForegroundColor Green

# Create a minimal BMP file (1x1 pixel, 24-bit)
$bmpData = @(
    0x42, 0x4D,  # BM signature
    0x3E, 0x00, 0x00, 0x00,  # File size (62 bytes)
    0x00, 0x00, 0x00, 0x00,  # Reserved
    0x36, 0x00, 0x00, 0x00,  # Data offset (54 bytes)
    0x28, 0x00, 0x00, 0x00,  # Header size (40 bytes)
    0x01, 0x00, 0x00, 0x00,  # Width (1 pixel)
    0x01, 0x00, 0x00, 0x00,  # Height (1 pixel)
    0x01, 0x00,              # Planes (1)
    0x18, 0x00,              # Bits per pixel (24)
    0x00, 0x00, 0x00, 0x00,  # Compression (none)
    0x08, 0x00, 0x00, 0x00,  # Image size (8 bytes)
    0x00, 0x00, 0x00, 0x00,  # X pixels per meter
    0x00, 0x00, 0x00, 0x00,  # Y pixels per meter
    0x00, 0x00, 0x00, 0x00,  # Colors in color table
    0x00, 0x00, 0x00, 0x00,  # Important color count
    0x00, 0x00, 0x00,        # Pixel data (black pixel)
    0x00                     # Padding
)

# Write banner.bmp
[System.IO.File]::WriteAllBytes("banner.bmp", $bmpData)
Write-Host "Created banner.bmp" -ForegroundColor Green

# Write dialog.bmp
[System.IO.File]::WriteAllBytes("dialog.bmp", $bmpData)
Write-Host "Created dialog.bmp" -ForegroundColor Green

Write-Host "Placeholder images created successfully!" -ForegroundColor Green
Write-Host "Note: These are minimal placeholder images. Replace with proper graphics for production." -ForegroundColor Yellow
