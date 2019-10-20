if "%~1" == "" (
    goto IncorrectParamsMessage
)

cd Frontend
start /wait dotnet publish --configuration Release
if %ERRORLEVEL% == 1 (
    goto BuildErrorMessage
)

cd ../Backend
start /wait dotnet publish --configuration Release
if %ERRORLEVEL% == 1 (
    goto BuildErrorMessage
)

cd ../TextListener
start /wait dotnet publish --configuration Release
if %ERRORLEVEL% == 1 (
    goto BuildErrorMessage
)

cd ../TextRankCalc
start /wait dotnet publish --configuration Release
if %ERRORLEVEL% == 1 (
    goto BuildErrorMessage
)

cd ../VowelConsCounter
start /wait dotnet publish --configuration Release
if %ERRORLEVEL% == 1 (
    goto BuildErrorMessage
)

cd ../VowelConsRater
start /wait dotnet publish --configuration Release
if %ERRORLEVEL% == 1 (
    goto BuildErrorMessage
)

cd ../..
if exist "%~1" (
    rmdir /s /q "%~1"
)

mkdir "%~1"\Frontend
mkdir "%~1"\Backend
mkdir "%~1"\TextListener
mkdir "%~1"\TextRankCalc
mkdir "%~1"\VowelConsCounter
mkdir "%~1"\VowelConsRater
mkdir "%~1"\config

xcopy src\Frontend\bin\Release\netcoreapp2.2\publish "%~1"\Frontend\
xcopy src\Backend\bin\Release\netcoreapp2.2\publish "%~1"\Backend\
xcopy src\TextListener\bin\Release\netcoreapp2.2\publish "%~1"\TextListener\
xcopy src\TextRankCalc\bin\Release\netcoreapp2.2\publish "%~1"\TextRankCalc\
xcopy src\VowelConsCounter\bin\Release\netcoreapp2.2\publish "%~1"\VowelConsCounter\
xcopy src\VowelConsRater\bin\Release\netcoreapp2.2\publish "%~1"\VowelConsRater\

xcopy config "%~1"\config
xcopy src\run.cmd "%~1"
xcopy src\stop.cmd "%~1"

echo BUILD SUCCESS
exit /b 0

:IncorrectParamsMessage
    echo Wrong call of buiding process. Please use: build.cmd <package_name-MAJOR.MINOR.PATCH>
    exit /b 1

:BuildErrorMessage
    echo The build process of the project failed
    exit /b 1