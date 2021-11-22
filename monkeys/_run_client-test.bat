set prompt=$G

dotnet run --project Client.csproj < nul
pause

dotnet run --project Client.csproj < tt-test.txt
pause

dotnet run --project Client.csproj -- 8102 < nul
pause
