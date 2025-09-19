#!/bin/bash
set -euo pipefail

project_path="${PROJECT_PATH:-$(find backend -maxdepth 2 -type f -name 'PowerKit3000.ConsoleApp.csproj' -o -name 'powerkit3000.consoleapp.csproj' | head -n1)}"

if [[ -z "$project_path" ]]; then
  echo "Unable to locate PowerKit3000 console app project file." >&2
  exit 1
fi

echo "Cleaning and Running PowerKit3000.ConsoleApp..."
dotnet clean "$project_path" --verbosity quiet 2>&1 | awk '!/MSB4011/ && !/^MSBuild version/'
dotnet run --project "$project_path" --verbosity quiet "$@" 2>&1 | awk '!/MSB4011/ && !/^MSBuild version/'
