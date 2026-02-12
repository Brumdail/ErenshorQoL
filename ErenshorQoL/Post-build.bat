@echo on
:: Ensure the log directory exists
if not exist ".\log" (
    mkdir ".\log"
)

:: Log the start time
echo -------------- Build started at %date% %time% in %CD% -------------- >> ".\log\Post-build.log"

:: Set the variables and paths
set ModName=ErenshorQoL
set TargetDir=.\bin\Release\netstandard2.1
echo Targeted DLL: %TargetDir%\%ModName%.dll  >> ".\log\Post-build.log"
set ModTestLocation=%USERPROFILE%\AppData\Roaming\Thunderstore Mod Manager\DataFolder\Erenshor\profiles\Default\BepInEx\plugins\Brumdail-%ModName%
echo ModTestLocation=%ModTestLocation% >> ".\log\Post-build.log"

:: Copy the DLL to the current directory and log the output
echo Copy the DLL to the folder directory and log the output >> ".\log\Post-build.log"
xcopy /y "%TargetDir%\%ModName%.dll" "." /d >> ".\log\Post-build.log"
if %errorlevel% neq 0 (
    echo !!! ---Error--- !!! %errorlevel% during copying %ModName%.dll >> ".\log\Post-build.log"
    exit /b %errorlevel%
)

:: Copy the DLL to the test mod directory and log the output
echo Copy the DLL to the test mod directory and log the output >> ".\log\Post-build.log"
if exist "%ModTestLocation%" (
            xcopy /y "%TargetDir%\%ModName%.dll" "%ModTestLocation%" /d >> ".\log\Post-build.log"
        if %errorlevel% neq 0 (
            echo !!! ---Error--- !!! %errorlevel% during copying %ModName%.dll to Test Profile >> ".\log\Post-build.log"
            exit /b %errorlevel%
        )
    ) else (
    echo !!! SKIPPING Copying the DLL to the test mod directory because it doesn't exist >> ".\log\Post-build.log"
)

:: Copy the DLL to the Thunderstore directory and log the output
echo Copy the DLL to the Thunderstore directory and log the output >> ".\log\Post-build.log"
xcopy /y "%TargetDir%\%ModName%.dll" ".\Thunderstore" /d >> ".\log\Post-build.log"
if %errorlevel% neq 0 (
    echo !!! ---Error--- !!! %errorlevel% during copying %ModName%.dll to Thunderstore directory >> ".\log\Post-build.log"
    exit /b %errorlevel%
)

:: Copy the LICENSE.txt to the Thunderstore directory and log the output
echo Copy the LICENSE.txt to the Thunderstore directory and log the output >> ".\log\Post-build.log"
xcopy /y "..\LICENSE.txt" ".\Thunderstore" /d >> ".\log\Post-build.log"
if %errorlevel% neq 0 (
    echo !!! ---Error--- !!! %errorlevel% during copying LICENSE.txt to Thunderstore directory >> ".\log\Post-build.log"
    exit /b %errorlevel%
)


:: Copy the README.md to the Thunderstore directory and log the output
echo Copy the README.md to the Thunderstore directory and log the output >> ".\log\Post-build.log"
xcopy /y "..\README.md" ".\Thunderstore" /d >> ".\log\Post-build.log"
if %errorlevel% neq 0 (
    echo !!! ---Error--- !!! %errorlevel% during copying README.md to Thunderstore directory >> ".\log\Post-build.log"
    exit /b %errorlevel%
)

:: Create a zip archive of the specified files and log the output
echo Create a zip archive of the specified files and log the output >> ".\log\Post-build.log"
powershell -command "& { Compress-Archive -Path '.\Thunderstore\%ModName%.dll', '.\Thunderstore\icon.png', '.\Thunderstore\LICENSE.txt', '.\Thunderstore\manifest.json', '.\Thunderstore\README.md' -DestinationPath '.\Thunderstore\%ModName%.zip' -Force; if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE } }" >> ".\log\Post-build.log"
if %errorlevel% neq 0 (
    echo !!! ---Error--- !!! %errorlevel% during compression >> ".\log\Post-build.log"
    exit /b %errorlevel%
)

:: Log the end time
echo -------------- Build ended at %date% %time% -------------- >> ".\log\Post-build.log"

:: If everything was successful, exit with code 0
exit /b 0
