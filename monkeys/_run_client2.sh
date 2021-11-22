#!/bin/bash

function pause () {
    read -n1 --project "Press any key to continue . . ."
    echo
}
set -v

cd "`dirname "$0"`"

dotnet run --project Monkeys-Standalone.csproj < /dev/null
pause

dotnet run --project Monkeys-Standalone.csproj < tt-test.txt
pause

dotnet run --project Monkeys-Standalone.csproj < tt-hamlet.txt
pause

dotnet run --project Monkeys-Standalone.csproj < tt-casablanca.txt
pause

dotnet run --project Monkeys-Standalone.csproj < tt-dylan.txt
pause

dotnet run --project Monkeys-Standalone.csproj < tt-leonard.txt
pause

dotnet run --project Monkeys-Standalone.csproj < tt-dante.txt
pause

pause

