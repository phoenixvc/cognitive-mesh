@echo off
set /p GITHUB_MCP_PAT=Enter your GitHub MCP PAT:
setlocal
set GITHUB_MCP_PAT=%GITHUB_MCP_PAT%
cd /d "C:\Users\smitj\cognitive mesh"
start "" "C:\Users\smitj\AppData\Local\Programs\Microsoft VS Code\Code.exe" . --new-window
endlocal
