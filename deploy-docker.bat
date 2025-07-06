echo off
REM ----------------------------------------
REM Build all images
REM ----------------------------------------
echo.
echo [1/2] Building Docker Compose services...
docker-compose build

REM ----------------------------------------
REM Start containers in detached mode
REM ----------------------------------------
echo.
echo [2/2] Starting Docker Compose services...
docker-compose up -d
IF %ERRORLEVEL% NEQ 0 (
    echo.
    echo *** docker-compose up failed with error %ERRORLEVEL%. Exiting.
    exit /b %ERRORLEVEL%
)
docker-compose restart
echo.
echo All services built and started successfully.
pause