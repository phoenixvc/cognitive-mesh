$pat = Read-Host -Prompt "Enter your GitHub MCP PAT" -AsSecureString
$env:GITHUB_MCP_PAT = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($pat))

Set-Location "C:\Users\smitj\cognitive-mesh"
& "C:\Users\smitj\AppData\Local\Programs\Microsoft VS Code\Code.exe" . --new-window
