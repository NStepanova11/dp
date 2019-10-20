cd Frontend
start dotnet Frontend.dll

cd ../Backend
start dotnet Backend.dll

cd ../TextListener
start dotnet TextListener.dll

cd ../TextRankCalc
start dotnet TextRankCalc.dll

cd ..

set file=%CD%\config\run_params.txt
for /f "tokens=1,2 delims=:" %%a in (%file%) do (
for /l %%i in (1, 1, %%b) do start /d %%a dotnet %%a.dll
)