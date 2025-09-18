#!/bin/bash
echo "Cleaning and Running PowerKit3000.ConsoleApp..."
dotnet clean backend/PowerKit3000.ConsoleApp/PowerKit3000.ConsoleApp.csproj -v q
dotnet run --project backend/PowerKit3000.ConsoleApp/PowerKit3000.ConsoleApp.csproj "$@"
