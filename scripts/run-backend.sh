#!/bin/bash
set -e

LOG_FILE="run.log"

echo "Starting backend service..." | tee -a $LOG_FILE
echo "Date: $(date)" | tee -a $LOG_FILE

# Navigate to the webapp directory and run the application
(cd backend/HappyTools.WebApp && dotnet run) 2>&1 | tee -a $LOG_FILE
