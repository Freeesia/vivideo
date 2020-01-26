@echo off
set BASE_DIR=%~dp0
popd %BASE_DIR%
Worker.exe
pushd
