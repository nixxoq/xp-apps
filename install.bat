@echo off
setlocal enabledelayedexpansion

:: base settings
title xp-apps - installation

if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (set "os_bit=x64") else (set "os_bit=x32")

set "current_path=%~dp0"
set "CURL_PATH=%current_path%\tools\curl\curl.exe"
set "ZIP_PATH=%current_path%\tools\7z.exe"

set "latest_OCA=3.0.4.?anary.b2"
set "program_version=0.0.3"

set "oca_is_installed=0"
set "installed_oca_version=0"

set "need_update_OCA=0"

for /f "delims=" %%a in ('ver ^| findstr 5.1') do set "result=%%a"

IF errorlevel 0 (
    goto menu    
) ELSE IF errorlevel 1 (
    for /f "delims=" %%a in ('ver ^| findstr 5.2') do set "result=%%a"
    if errorlevel 1 (
       goto menu 
    ) else (
       echo This program doesn't work on Windows 2000, Windows Vista and later
       pause
       goto :eof
    )
)


:menu
:: check if oca is installed
for /f "delims=" %%a in ('wmic qfe list full ^| findstr OCAB') do set "result=%%a"
if "%result%" == "ServicePackInEffect=OCAB" (
   set "need_update_OCA=1"
   set "oca_is_installed=1"

   FOR /F "tokens=2*" %%A IN ('REG QUERY "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\OCAB" /v DisplayVersion 2^>nul') DO SET "installed_oca_version=%%B"
)


echo checking internet connection..
Ping www.google.com -n 1 -w 1000 >NUL
    
if errorlevel 1 (set "internet=not_connected") else (set "internet=connected")
if "%internet%"=="connected" (
   "%CURL_PATH%" -# -L -o currentversion.txt https://raw.githubusercontent.com/Snaky1a/xp-apps/main/currentversion.txt
)
cls

echo                                 XP-tool
echo    Internet: %internet%

if "%oca_is_installed%"=="1" (
    echo    Is OCA installed? Yes
    echo    Installed OCA version: %installed_oca_version%
) else (
    echo    Is OCA installed? No
)

if not "%installed_oca_verrsion%" == "%latest_OCA%" (
    echo.
    echo    MESSAGE: A new version of One-Core-API has been released. To update, select option 1
) 

FOR /F %%i IN (currentversion.txt) DO (set new_version=%%i)

echo %new_version% | findstr %program_version% >NUL
if errorlevel 1 (
   echo.
   "%CURL_PATH%" -# -L -o update.bat https://raw.githubusercontent.com/Snaky1a/xp-apps/main/update.bat
   echo    MESSAGE: A new version of installer has been released. To update, exit from this program and run the update.bat script
)


echo.

echo [1] Download and install Important components (One-Core-API and Visual C++ redists)
echo [2] Open the browser category
echo [3] Open the Windows Vista Applications category
echo [4] Open the Windows 7 Applications menu
echo [5] Open the Codec/Audio/Video category
echo [6] Open the Utilities category
echo [7] Open the Other category
echo [8] Open the Office category
echo [9] Open the Programming/Code editors category
echo.
echo [0] Exit

set /P option=Select Option: 

cls
if "%option%"=="1" (
   call :DoInstall %option%  
) else if "%option%"=="2" (
   goto :BrowserMenu
) else if "%option%"=="3" (
   goto :VistaApps
) else if "%option%"=="4" (
   goto :WinSevenApps
) else if "%option%"=="5" (
   goto :CodecandAudioMenu
) else if "%option%"=="6" (
   goto :UtilitiesMenu
) else if "%option%"=="7" (
   goto :OtherMenu
) else if "%option%"=="8" (
   goto :OfficeMenu
) else if "%option%"=="9" (
   goto :Progrmenu
) else if "%option%"=="0" (
   goto :eof
) else (
   echo Not implemented. Check new updates...
   pause
   goto menu
)

:: Installer
:DoInstall

set selected=%~1
cls
echo Installer
echo.

if "%selected%"=="1" (
   echo Downloading archive, please wait...
   "%CURL_PATH%" -# -L -o Important.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Important.zip
   
   echo Extracting, please wait...
   "%ZIP_PATH%" x Important.zip -y -bsp2 -bso0

   echo Installing One-Core-API, please wait...
   cls
   call "%current_path%Important\Install One-Core-API.bat"
   shutdown /a
   echo Installing Visual C++ 2005 x86
   start /wait "installing" "%current_path%Important\vcredist_x86_2005.exe" /Q
   echo Installing Visual C++ 2008 x86
   start /wait "installing" "%current_path%Important\vcredist_x86_2008.exe" /Q
   echo Installing Visual C++ 2010 x86
   start /wait "installing" "%current_path%Important\vcredist_x86_2010.exe" /q /norestart
   echo Installing Visual C++ 2012 x86
   start /wait "installing" "%current_path%Important\vcredist_x86_2012.exe" /quiet /norestart
   echo Installing Visual C++ 2013 x86
   start /wait "installing" "%current_path%Important\vcredist_x86_2013.exe" /quiet /norestart
   echo Installing Visual C++ 2015 x86
   start /wait "installing" "%current_path%Important\vcredist_x86_2015.exe" /quiet /norestart
   echo Installing Visual C++ 2015-2019 x86
   start /wait "installing" "%current_path%Important\vcredist_x86_2015_2019.exe" /quiet /norestart

   if "%os_bit%"=="x64" (
       echo Installing Visual C++ 2005 x64
       start /wait "installing" "%current_path%Important\vcredist_x64_2005.exe" /Q
       echo Installing Visual C++ 2008 x64
       start /wait "installing" "%current_path%Important\vcredist_x64_2008.exe" /Q
       echo Installing Visual C++ 2010 x64
       start /wait "installing" "%current_path%Important\vcredist_x64_2010.exe" /q /norestart
       echo Installing Visual C++ 2012 x64
       start /wait "installing" "%current_path%Important\vcredist_x64_2012.exe" /quiet /norestart
       echo Installing Visual C++ 2013 x64
       start /wait "installing" "%current_path%Important\vcredist_x64_2013.exe" /quiet /norestart
       echo Installing Visual C++ 2015-2019 x64
       start /wait "installing" "%current_path%Important\vcredist_x64_2015_2019.exe" /quiet /norestart
   )

   echo Installing .NET Framework 4.8...
   call "%current_path%Important\Install dot net 4.8.bat"

   echo.
   echo ========================================================
   echo - One-Core-API %latest_OCA% Was installed!
   echo - Your OS will be restarted in 30 seconds
   echo ========================================================
   echo.
    
    
   shutdown /r /t 30 /c "One-Core-API %latest_OCA% was installed! Your OS will be restarted in 30 seconds"
   pause
    
   goto :menu

) else if "%selected%"=="catsxp117" (
    echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o CatsXP117.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/catsxp.chrome.117.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x CatsXP117.zip -y -bsp2 -bso0
    
    echo Path to program: "%current_path%\catsxp (chrome 117)". Do you want run this program now?
    tools\choice.exe /N /C:YN /M "[Y] Yes or [N] No"

    if %errorlevel% equ 0 (
       cls
       call "%current_path%catsxp (chrome 117)\RunCatsxp.bat"
    ) else if %errorlevel% equ 7 (
      goto :menu
    )
    pause
) else if "%selected%"=="catsxp118" (
    echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o CatsXP118.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/catsxp.chrome.118.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x CatsXP118.zip -y -bsp2 -bso0
    
    echo Path to program: "%current_path%\catsxp (chrome 118)". Do you want run this program now?
    tools\choice.exe /N /C:YN /M "[Y] Yes or [N] No"

    if %errorlevel% equ 0 (
       cls
       call "%current_path%catsxp (chrome 118)\RunCatsxp.bat"
    ) else if %errorlevel% equ 7 (
      goto :menu
    )
    pause
) else if "%selected%"=="brave101" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o Brave101.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/brave.101.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x Brave101.zip -y -bsp2 -bso0
    
    echo Path to program: "%current_path%\brave 101\Brave Browser 101 Portable 32bits\BRAVE 101 By Tortilla5\". Do you want run this program now?
    tools\choice.exe /N /C:YN /M "[Y] Yes or [N] No"

    if %errorlevel% equ 0 (
       cls
       start "asdasd" "%current_path%brave 101\Brave Browser 101 Portable 32bits\BRAVE 101 By Tortilla5\brave.exe"
    ) else if %errorlevel% equ 7 (
      goto :menu
    )
    pause
) else if "%selected%"=="msedge109" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o medge109.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Microsoft.Edge.109.zip 
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x msedge109.zip -y -bsp2 -bso0
    
    echo Path to program: "%current_path%\Microsoft Edge (109)\". Do you want run this program now?
    tools\choice.exe /N /C:YN /M "[Y] Yes or [N] No"

    if %errorlevel% equ 0 (
       cls
       call "%current_path%Microsoft Edge (109)\RunMSEdge.bat"
    ) else if %errorlevel% equ 7 (
      goto :menu
    )
    pause
) else if "%selected%"=="idm" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o idm.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Internet.Download.Manager.6.40.build.11.zip 
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x idm.zip -y -bsp2 -bso0
    
    echo Path to program: "%current_path%\Internet Download Manager 6.40 build 11\Internet Download Manager\". Do you want run this program now?
    tools\choice.exe /N /C:YN /M "[Y] Yes or [N] No"

    if %errorlevel% equ 0 (
       cls
       start "dasfasdf" "%current_path%Internet Download Manager 6.40 build 11\Internet Download Manager\IDMan.exe"
    ) else if %errorlevel% equ 7 (
      goto :menu
    )
    pause
) else if "%selected%"=="epic91" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o epic91.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Epic.Browser.91.zip 
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x epic91.zip -y -bsp2 -bso0
    
    echo Path to program: "%current_path%\Epic Browser 91\". Do you want run this program now?
    tools\choice.exe /N /C:YN /M "[Y] Yes or [N] No"
    if %errorlevel% equ 0 (
       cls
       call "%current_path%Epic Browser 91\RunEpicBrowser.bat"
    ) else if %errorlevel% equ 7 (
      goto :menu
    )
    pause
) else if "%selected%"=="epic104" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o epic104.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Epic.Browser.104.zip 
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x epic104.zip -y -bsp2 -bso0
    
    echo Path to program: "%current_path%\Epic Browser 104\". Do you want run this program now?
    tools\choice.exe /N /C:YN /M "[Y] Yes or [N] No"
    if %errorlevel% equ 0 (
       cls
       call "%current_path%Epic Browser 104\RunEpicBrowser.bat"
    ) else if %errorlevel% equ 7 (
      goto :menu
    )
    pause
) else if "%selected%"=="firefox79" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o firefox79.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Firefox.79.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x firefox79.zip -y -bsp2 -bso0
    
    echo Path to Firefox 79: "%current_path%\Firefox 79\"
    pause
    goto :menu
) else if "%selected%"=="msgamesvista" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o msgamesvista.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Microsoft.Games.Windows.Vista.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x msgamesvista.zip -y -bsp2 -bso0
    
    echo Path to games: "%current_path%\Microsoft Games (Windows Vista)\".
    pause
    goto :menu
) else if "%selected%"=="msmoviemaker" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o msmovie.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Windows.Movie.Maker.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x msmovie.zip -y -bsp2 -bso0
    
    echo Path to Windows Movie Maker: "%current_path%\Windows Movie Maker\".
    pause
    goto :menu
) else if "%selected%"=="mssidebar" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o sidebars.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Windows.Sidebars.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x sidebars.zip -y -bsp2 -bso0
    
    echo Path to Windows Sidebars: "%current_path%\Windows Sidebars\".
    pause
    goto :menu
) else if "%selected%"=="klite" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o klite.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/k-lite.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x klite.zip -y -bsp2 -bso0
    
    echo Path to Kodec Lite: "%current_path%\k-lite\".
    pause
    goto :menu
) else if "%selected%"=="aimp5" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o aimp5.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/AIMP.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x aimp5.zip -y -bsp2 -bso0
    
    echo Path to Aimp 5: "%current_path%\AIMP\".
    pause
    goto :menu
) else if "%selected%"=="dotnet472" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o dotnet472.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/dot.net.472.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x dotnet472.zip -y -bsp2 -bso0
    
    echo Path to .NET Framework 4.7.2: "%current_path%\dot net 472\".
    pause
    goto :menu
) else if "%selected%"=="dotnet452" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o dotnet452.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/dot.net.452.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x dotnet452.zip -y -bsp2 -bso0
    
    echo Path to .NET Framework 4.5.2: "%current_path%\dot.net.452\".
    pause
    goto :menu
) else if "%selected%"=="jdk21" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o jdk21.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/jdk21.java.21.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x jdk21.zip -y -bsp2 -bso0
    
    echo Path to JDK 21/Java 21: "%current_path%\jdk21 (java 21)\".
    pause
    goto :menu
) else if "%selected%"=="jdk11" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o jdk11.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/jdk11.java.11.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x jdk11.zip -y -bsp2 -bso0
    
    echo Path to JDK 11/Java 11: "%current_path%\jdk11 (java 11)\".
    pause
    goto :menu
) else if "%selected%"=="jdk18_17" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o openjdk1_8_17.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/openjdk-1.8_openjdk-17.zip 
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x openjdk1_8_17.zip -y -bsp2 -bso0
    
    echo Path to OpenJDK 1.8 and OpenJDK 17: "%current_path%\openjdk-1.8_openjdk-17\".
    pause
    goto :menu
) else if "%selected%"=="jdk21x64" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o jdk21x64.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/jdk-21.0.1_x64.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x jdk21x64.zip -y -bsp2 -bso0
    
    echo Path to JDK 21/Java 21 [x64]: "%current_path%\jdk-21.0.1_x64\jdk-21.0.1\".
    pause
    goto :menu
) else if "%selected%"=="jdk11x64" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o jdk11x64.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/jdk-11.0.21_x64.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x jdk11x64.zip -y -bsp2 -bso0
    
    echo Path to JDK 11/Java 11 [x64]: "%current_path%\jdk-11.0.21_x64\jdk-11.0.21\".
    pause
    goto :menu
) else if "%selected%"=="photoshop2018" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o photoshop.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/photoshop.cc.2018.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x photoshop.zip -y -bsp2 -bso0
    
    echo Path to Photoshop CC 2018 [x64]: "%current_path%\photoshop cc 2018\".
    pause
    goto :menu
) else if "%selected%"=="sharex" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o sharex.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/ShareX.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x sharex.zip -y -bsp2 -bso0
    
    echo Path to ShareX 15.0: "%current_path%\ShareX\".
    echo WARNING: If you haven't installed .NET Framework 4.8, please install it from option 1
    pause
    goto :menu
) else if "%selected%"=="freeoffice" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o freeoffice.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/freeoffice.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x freeoffice.zip -y -bsp2 -bso0
    
    echo Path to FreeOffice: "%current_path%\freeoffice\".
    pause
    goto :menu
) else if "%selected%"=="python39" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o python39.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Python.3.9.13+.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x python39.zip -y -bsp2 -bso0
    
    echo Path to Python 3.9.13+: "%current_path%\Python 3.9.13+\".
    pause
    goto :menu
) else if "%selected%"=="vscode1_70" (
    echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o vscode1_70.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Visual.Studio.Code.1.70.3.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x vscode1_70.zip -y -bsp2 -bso0
    
    echo Path to Visual Studio Code 1.70.3: "%current_path%\Visual Studio Code 1.70.3\".
    echo Do you want run this program now?
    tools\choice.exe /N /C:YN /M "[Y] Yes or [N] No"

    if %errorlevel% equ 0 (
       cls
       call "%current_path%Visual Studio Code 1.70.3\RunVSCode.bat"
    ) else if %errorlevel% equ 7 (
      goto :menu
    )
    pause
) else if "%selected%"=="vscode1_83" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o vscode1_83.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Visual.Studio.Code.1.83.1.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x vscode1_83.zip -y -bsp2 -bso0
    
    echo Path to Visual Studio Code 1.83.1: "%current_path%\Visual Studio Code 1.83.1\".
    echo Do you want run this program now?
    tools\choice.exe /N /C:YN /M "[Y] Yes or [N] No"

    if %errorlevel% equ 0 (
       cls
       call "%current_path%Visual Studio Code 1.83.1\RunVSCode.bat"
    ) else if %errorlevel% equ 7 (
      goto :menu
    )
    pause
) else if "%selected%"=="pycharm2017" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o pycharm2017.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/pycharm2017.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x pycharm2017.zip -y -bsp2 -bso0
    
    echo Path to Pycharm Community 2017: "%current_path%\pycharm2017\".
    pause
    goto :menu
) else if "%selected%"=="pycharm2018" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o pycharm2018.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/pycharm2018.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x pycharm2018.zip -y -bsp2 -bso0
    
    echo Path to Pycharm Community 2018: "%current_path%\pycharm2018.3.7\".
    pause
    goto :menu
) else if "%selected%"=="pycharm2023" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o pycharm2023.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/pycharm2023.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x pycharm2023.zip -y -bsp2 -bso0
    
    echo Path to Pycharm Community 2023: "%current_path%\pycharm2023\".
    pause
    goto :menu
) else if "%selected%"=="clion_2021" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o clion2021.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/clion_2021_3_4.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x clion2021.zip -y -bsp2 -bso0
    
    echo Path to CLion 2021.3.4: "%current_path%\clion_2021_3_4\".
    pause
    goto :menu
) else if "%selected%"=="clion_2023" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o clion2023.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/clion2023_2_2.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x clion2023.zip -y -bsp2 -bso0
    
    echo Path to CLion 2023.2.2: "%current_path%\clion2023_2_2\".
    pause
    goto :menu
) else if "%selected%"=="msgames7" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o win7games.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Windows.7.Games.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x win7games.zip -y -bsp2 -bso0
    
    echo Path to Microsoft Windows 7 Games: "%current_path%\Windows 7 Games\".
    pause
    goto :menu
) else if "%selected%"=="wordpad" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o wordpad.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/wordpad.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x wordpad.zip -y -bsp2 -bso0
    
    echo Path to Wordpad from Windows 7: "%current_path%\wordpad\".
    pause
    goto :menu
) else if "%selected%"=="chromium121_x64" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o chromium121.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/chromium121_x64.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x chromium121.zip -y -bsp2 -bso0
    
    echo Path to Chromium 121: "%current_path%\chromium121_x64\".
    pause
    goto :menu
) else if "%selected%"=="chromium121_x86" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o chromium121.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/chromium121_x86.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x chromium121.zip -y -bsp2 -bso0
    
    echo Path to Chromium 121: "%current_path%\chromium121_x86\".
    pause
    goto :menu
) else if "%selected%"=="flstudio20" (
    echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o flstudio20.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Image-Line.FL.Studio.Producer.Edition.v20.9.2.2963.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x flstudio20.zip -y -bsp2 -bso0
    
    echo Path to FL Studio 20.9.2: "%current_path%\Image-Line FL Studio Producer Edition v20.9.2.2963\".
    pause
    goto :menu
) else if "%selected%"=="telegram" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o telegram.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/Telegram.zip
       
    echo Extracting, please wait...
    "%ZIP_PATH%" x flstudio20.zip -y -bsp2 -bso0
    
    echo Path to Telegram Desktop: "%current_path%\Telegram\".
    pause
    goto :menu
) else if "%selected%"=="libreoffice6003" (
   echo Downloading archive, please wait...
    "%CURL_PATH%" -# -L -o libreoffice6003.zip https://github.com/Snaky1a/xp-apps/releases/download/2023_10_11_17_52/LibreOfficePortable.zip
       
    echo  Extracting, please wait...
    "%ZIP_PATH%" x libreoffice6003.zip -y -bsp2 -bso0
    
    echo Path to LibreOffice 6.0.0.3 Portable: "%current_path%\LibreOfficePortable\".
    pause
    goto :menu
) else (
   goto :eof
)


:BrowserMenu
echo                                 XP-tool
echo    Internet: %internet%

echo.
echo [1] CatsXP (Chromium 117)
echo [2] CatsXP (Chromium 118)
echo [3] Brave 101
echo [4] Microsoft Edge 109 (109.0.1518.140)
echo [5] Internet Download Manager 6.40 build 11
echo [6] Epic Privacy Browser version 91
echo [7] Epic Privacy Browser version 104
echo [8] Firefox 79
echo [9] Chromium 121.0.6138.0 (latest dev) x64
echo [10] Chromium 121.0.6138.0 (latest dev) x86
echo.
echo [0] Back to the main menu
echo.

set /p output="Input number: " 

if "%output%"=="1" (
   call :DoInstall catsxp117
) else if "%output%"=="2" (
   call :DoInstall catsxp118
) else if "%output%"=="3" (
   call :DoInstall brave101
) else if "%output%"=="4" (
   call :DoInstall msedge109
) else if "%output%"=="5" (
   call :DoInstall idm
) else if "%output%"=="6" (
   call :DoInstall epic91
) else if "%output%"=="7" (
   call :DoInstall epic104 
) else if "%output%"=="8" (
   call :DoInstall firefox79
) else if "%output%"=="9" (
   call :DoInstall chromium121_x64
) else if "%output%"=="10" (
   call :DoInstall chromium121_x86
) else if "%output%"=="0" (
   goto :menu
) else (
   echo Please enter a number!
   pause
   goto BrowserMenu
)

:VistaApps
echo                                 XP-tool
echo    Internet: %internet%

echo.
echo [1.] Microsoft Games from Windows Vista build 5259 and 5270
echo [2.] Windows Movie Maker from Windows Vista build 5270
echo [3.] Windows Sidebar from Windows Vista build 5744 and RTM
echo.
echo [0.] Back to the main menu

echo.

set /p output="Input number: " 

if "%output%"=="1" (
   call :DoInstall msgamesvista
) else if "%output%"=="2" (
   call :DoInstall msmoviemaker
) else if "%output%"=="3" (
   call :DoInstall mssidebar
) else if "%output%"=="0" (
   goto :menu
) else (
   echo Please enter a number!
   pause
   goto VistaApps
)
rem ) else if "%output%"=="4" (
rem    call :DoInstall inkball
rem ) else (

:WinSevenApps
echo                                 XP-tool
echo    Internet: %internet%

echo.
echo [1] Windows 7 Games
echo [2] Wordpad
echo [3] Paint
echo.
echo [0] Back to the main menu
echo.

set /p output="Input number: " 

if "%output%"=="1" (
   call :DoInstall msgames7
) else if "%output%"=="2" (
   call :DoInstall wordpad
) else if "%output%"=="3" (
   call :Doinstall mspaint
) else if "%output%"=="0" (
   goto :menu
) else (
   echo Please enter a number!
   pause
   goto WinSevenApps
)

:CodecandAudioMenu
echo                                 XP-tool
echo    Internet: %internet%

echo.
echo [1] K-Lite Codec Pack 17.8.0 Full
echo [2] AIMP 5.1.1.2436
echo [3] FL Studio 20 (Windows XP x64 only)
echo.
echo [0] Back to the main menu
echo.

set /p output="Input number: " 

if "%output%"=="1" (
   call :DoInstall klite
) else if "%output%"=="2" (
   call :DoInstall aimp5
) else if "%output%"=="3" (
   call :DoInstall flstudio20  
) else if "%output%"=="0" (
   goto :menu
) else (
   echo Please enter a number!
   pause
   goto CodecandAudioMenu
)

:UtilitiesMenu
echo                                 XP-tool
echo    Internet: %internet%

echo.
echo [1] .NET Framework 4.7.2
echo [2] .NET Framework 4.5.2
echo [3] JDK 21/Java 21
echo [4] JDK 11/Java 11
echo [5] OpenJDK 1.8 and OpenJDK 17
echo [6] JDK 21/Java 21 [x64]
echo [7] JDK 11/Java 11 [x64]
echo.
echo [0] Back to the main menu

echo.

set /p output="Input number: " 

if "%output%"=="1" (
   call :DoInstall dotnet472
) else if "%output%"=="2" (
   call :DoInstall dotnet452
) else if "%output%"=="3" (
   call :DoInstall jdk21
) else if "%output%"=="4" (
   call :DoInstall jdk11
) else if "%output%"=="5" (
   call :DoInstall jdk18_17
) else if "%output%"=="6" (
   call :DoInstall jdk21x64
) else if "%output%"=="7" (
   call :DoInstall jdk11x64
) else if "%output%"=="0" (
   goto :menu
) else (
   echo Please enter a number!
   pause
   goto UtilitiesMenu
)

:OtherMenu
echo                                 XP-tool
echo    Internet: %internet%

echo.
echo [1] Adobe Photoshop CC 2018
echo [2] ShareX 15.0
echo [3] Telegram Desktop
echo.
echo [0] Back to the main menu

echo.

set /p output="Input number: " 

if "%output%"=="1" (
   call :DoInstall photoshop2018
) else if "%output%"=="2" (
   call :DoInstall sharex
) else if "%output%"=="3" (
   call :DoInstall telegram
) else if "%output%"=="0" (
   goto :menu
) else (
   echo Please enter a number!
   pause
   goto OtherMenu
)

:OfficeMenu
echo                                 XP-tool
echo    Internet: %internet%

echo.
echo [1] FreeOffice
echo [2] LibreOffice 6.0.0.3 Portable
echo.
echo [0] Back to the main menu

echo.

set /p output="Input number: " 

if "%output%"=="1" (
   call :DoInstall freeoffice
) else if "%selected%"=="2" (
   call :DoInstall libreoffice6003
) else if "%output%"=="0" (
   goto :menu
) else (
   echo Please enter a number!
   pause
   goto OfficeMenu
)

:Progrmenu
echo                                 XP-tool
echo    Internet: %internet%

echo.
echo [1] Python 3.9.13+
echo [2] Visual Studio Code 1.70.3
echo [3] Visual Studio Code 1.83.1
echo [4] JetBrains PyCharm Community 2017.3.4 Portable
echo [5] JetBrains PyCharm Community 2018.3.7
echo [6] JetBrains CLion 2021.3.4
echo [7] JetBrains CLion 2023.2.2
echo [8] JetBrains PyCharm Community 2023.2.2 [x64 only]
echo.
echo [0] Back to the main menu


echo.

set /p output="Input number: " 

if "%output%"=="1" (
   call :DoInstall python39
) else if "%output%"=="2" (
   call :DoInstall vscode1_70
) else if "%output%"=="3" (
   call :DoInstall vscode1_83
) else if "%output%"=="4" (
   call :DoInstall pycharm2017
) else if "%output%"=="5" (
   call :DoInstall pycharm2018
) else if "%output%"=="6" (
   call :DoInstall clion_2021
) else if "%output%"=="7" (
   call :DoInstall clion_2023 
) else if "%output%"=="8" (
   call :DoInstall pycharm2023
) else if "%output%"=="0" (
   goto :menu
) else (
   echo Please enter a number!
   pause
   goto Progrmenu
)