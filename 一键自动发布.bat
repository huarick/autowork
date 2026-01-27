@echo off
chcp 65001 >nul
title 网站自动化发布批处理程序
setlocal enabledelayedexpansion

echo 正在执行网站自动化发布流程...
echo.

echo 请先选择操作模式:
echo   1 - PDM
echo   2 - PlatformProduct
echo   3 - PlatformSyn
echo.

:retry_choice
set /p user_choice=请输入选择 (1, 2 或 3): 
if "%user_choice%"=="1" goto start_process
if "%user_choice%"=="2" goto start_process
if "%user_choice%"=="3" goto start_process
echo 输入无效，请重新输入!
goto retry_choice

:start_process
echo 您选择了模式 %user_choice%，开始执行自动化流程...
echo.

echo 1. 开始自动更新SVN...
"D:\Rick\自动化项目\网站自动化发布-1.自动更新SVN\AutoSvnUpdate\bin\Debug\AutoSvnUpdate.exe"
echo AutoSvnUpdate 执行完成 ✓
echo.

echo 2. 开始自动发布网站...
echo 正在执行模式 %user_choice%...
echo %user_choice%| "D:\Rick\自动化项目\网站自动化发布-2.自动发布网站\AutoPublishProject\bin\Debug\AutoPublishProject.exe"
echo AutoPublishProject 执行完成 ✓
echo.

echo 3. 开始自动比较文件夹...
echo 自动应用之前的选择 (模式 %user_choice%)...
echo %user_choice%| "D:\Rick\自动化项目\网站自动化发布-3.自动比较文件夹\AutoCompareFolder\bin\Debug\AutoCompareFolder.exe"
echo AutoCompareFolder 执行完成 ✓
echo.

echo ==========================================
echo 所有自动化程序已成功执行完成!
echo 使用的模式: %user_choice%
echo ==========================================
pause