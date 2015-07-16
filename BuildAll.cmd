rem build all solution files in this folder
@echo off
SETLOCAL ENABLEDELAYEDEXPANSION
set buildfailed=0
set /a countbuild=0
set /a counterror=0
for /r %%x in (*.sln) do (
 echo. 
 echo Building: "%%x"
 set /a countbuild=!countbuild!+1
 devenv.exe "%%x" /rebuild "Debug" 
 if ERRORLEVEL 1 (
	set buildfailed=1 
	echo solution build for "%%x" failed
	echo.
	set /a counterror=!counterror!+1
 )
)

IF NOT %buildfailed% == 0 GOTO ERROR
GOTO DONE

:ERROR
echo.
echo -------------------------------
echo Build failed with error(s)
echo Solutions built: !countbuild!
echo Solutions with broken builds: !counterror!
echo -------------------------------
GOTO END

:DONE
echo.
echo ------------------------
echo Build successful
echo Solutions built: !countbuild!
echo Solutions with broken builds: !counterror!
echo ------------------------
:END


