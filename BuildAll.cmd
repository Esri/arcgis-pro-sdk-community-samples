rem build all solution files in this folder
@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
set buildfailed=0
set /a countbuild=0
set /a counterror=0
for /r %%x in (*.sln) do (
 echo. 
 echo Building: 
 echo %%x 
 set /a countbuild=!countbuild!+1
 devenv "%%x" /build "Debug" 
 if ERRORLEVEL 1 (
	set buildfailed=1 
	echo. 
	echo build solution failed
	echo.
	set /a counterror=!counterror!+1
 )
)

IF NOT %buildfailed% == 0 GOTO ERROR
GOTO DONE

:ERROR
echo.
echo --------------------------
echo Build failed with error(s)
echo Solutions built: !countbuild!
echo Error builds: !counterror!
echo --------------------------
GOTO END

:DONE
echo.
echo -------------------
echo Build successful
echo Solutions built: !countbuild!
echo Error builds: !counterror!
echo -------------------
:END


