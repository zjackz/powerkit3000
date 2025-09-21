#!/bin/bash
set -euo pipefail

project_path="${PROJECT_PATH:-$(find backend -maxdepth 2 -type f -name 'PowerKit3000.Api.csproj' -o -name 'powerkit3000.api.csproj' | head -n1)}"

if [[ -z "$project_path" ]]; then
  echo "Unable to locate PowerKit3000 API project file." >&2
  exit 1
fi

echo "Restoring and running PowerKit3000.Api..."

local_packages="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
restore_cmd=(dotnet restore "$project_path" --verbosity quiet)
if [[ -d "$local_packages" ]]; then
  restore_cmd+=(--source "$local_packages" --ignore-failed-sources)
fi
if ! "${restore_cmd[@]}" 2>&1 | awk '!/MSB4011/ && !/^MSBuild version/'; then
  echo "Restore step failed; attempting to continue with existing assets." >&2
fi

run_cmd=(dotnet run --no-restore --project "$project_path" --verbosity quiet)
if [[ $# -gt 0 ]]; then
  run_cmd+=("$@")
fi

if ! "${run_cmd[@]}" 2>&1 | awk '!/MSB4011/ && !/^MSBuild version/'; then
  dotnet run --project "$project_path" --verbosity quiet "$@" 2>&1 | awk '!/MSB4011/ && !/^MSBuild version/'
fi
