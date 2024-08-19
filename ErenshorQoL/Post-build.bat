@echo on
:: Log the start time
echo Build started at %date% %time% -------------- >> ".\log\Post-build.log"

:: Set the target directory path manually for CMD testing
set TargetDir=C:\Users\brumd\Documents\GitHub\ErenshorQoL\ErenshorQoL\bin\Release\netstandard2.1
echo "Targeted DLL: %TargetDir%\ErenshorQoL.dll"
:: Copy the DLL to the current directory and log the output
xcopy /y "%TargetDir%\ErenshorQoL.dll" "." /d >> ".\log\Post-build.log"
if %errorlevel% neq 0 (
    echo Error %errorlevel% during copying ErenshorQoL.dll >> ".\log\Post-build.log"
    exit /b %errorlevel%
)

:: Copy the DLL to the test mod directory and log the output
xcopy /y "%TargetDir%\ErenshorQoL.dll" "C:\Users\brumd\AppData\Roaming\Thunderstore Mod Manager\DataFolder\Erenshor\profiles\Test1\BepInEx\plugins\Brumdail-ErenshorQoL" /d >> ".\log\Post-build.log"
if %errorlevel% neq 0 (
    echo Error %errorlevel% during copying ErenshorQoL.dll >> ".\log\Post-build.log"
    exit /b %errorlevel%
)

:: Remove the previous zip archive if it exists
if exist "ErenshorQoL.zip" (
    del /f /q "ErenshorQoL.zip"
    if %errorlevel% neq 0 (
        echo Error during deleting previous zip >> ".\log\Post-build.log"
        exit /b %errorlevel%
    )
)

:: Create a zip archive of the specified files and log the output
powershell -command "& { Compress-Archive -Path 'Thunderstore\ErenshorQoL.dll', 'Thunderstore\icon.png', 'Thunderstore\LICENSE', 'Thunderstore\manifest.json', 'Thunderstore\README.md' -DestinationPath 'Thunderstore\ErenshorQoL.zip'; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE } }" >> ".\log\Post-build.log"
if %errorlevel% neq 0 (
    echo Error %errorlevel% during compression >> ".\log\Post-build.log"
    exit /b %errorlevel%
)

:: Log the end time
echo Build ended at %date% %time% -------------- >> ".\log\Post-build.log"

:: If everything was successful, exit with code 0
exit /b 0
