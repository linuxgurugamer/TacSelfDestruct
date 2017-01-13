

set H=R:\KSP_1.2.2_dev
echo %H%

copy /Y "bin\Debug\TacSelfDestruct.dll" "..\GameData\TacSelfDestruct\Plugins"
copy /Y TacSelfDestruct.version ..\GameData\TacSelfDestruct

cd ..\GameData
mkdir "%H%\GameData\TacSelfDestruct"
xcopy /y /s TacSelfDestruct "%H%\GameData\TacSelfDestruct"
