@echo off

copy /Y "bin\Release\TacSelfDestruct.dll" "..\GameData\TacSelfDestruct\Plugins"
copy /Y TacSelfDestruct.version ..\GameData\TacSelfDestruct
copy /Y ..\..\MiniAVC.dll ..\GameData\TacSelfDestruct

set DEFHOMEDRIVE=d:
set DEFHOMEDIR=%DEFHOMEDRIVE%%HOMEPATH%
set HOMEDIR=
set HOMEDRIVE=%CD:~0,2%

set RELEASEDIR=d:\Users\jbb\release
set ZIP="c:\Program Files\7-zip\7z.exe"
echo Default homedir: %DEFHOMEDIR%

rem set /p HOMEDIR= "Enter Home directory, or <CR> for default: "

if "%HOMEDIR%" == "" (
set HOMEDIR=%DEFHOMEDIR%
) 
echo %HOMEDIR%

SET _test=%HOMEDIR:~1,1%
if "%_test%" == ":" (
set HOMEDRIVE=%HOMEDIR:~0,2%
)


type TacSelfDestruct.version
set /p VERSION= "Enter version: "

copy ..\LICENSE.txt ..\GameData\TacSelfDestruct

copy /Y ..\README.md ..\GameData\TacSelfDestruct
 
cd ..

set FILE="%RELEASEDIR%\TacSelfDestruct-%VERSION%.zip"
IF EXIST %FILE% del /F %FILE%
%ZIP% a -tzip %FILE% GameData

