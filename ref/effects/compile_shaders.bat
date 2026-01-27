@echo off
set "MGFXC_PATH=%userprofile%\.nuget\packages\monogame.content.builder.task\3.8.0.1641\tools\netcoreapp3.1\any\mgfxc.exe"

echo Starting shader compilation...
echo.

for %%f in (*.hlsl) do (
    echo Compiling: %%f
    "%MGFXC_PATH%" "%%f" "%%~nf.mgfx" /Profile:DirectX_11
)

echo.
echo Done!
pause