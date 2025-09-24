#!/bin/bash
set -euo pipefail

if [[ -n "${PROJECT_PATH:-}" ]]; then
  project_path="$PROJECT_PATH"
else
  if [[ -f backend/pk.api/pk.api.csproj ]]; then
    project_path="backend/pk.api/pk.api.csproj"
  else
    project_path="$(find backend -maxdepth 2 -type f -name 'pk.api.csproj' | head -n1)"
  fi
fi

if [[ -z "$project_path" ]]; then
  echo "Unable to locate pk.api project file." >&2
  exit 1
fi

echo "Restoring and running pk.api..."

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
