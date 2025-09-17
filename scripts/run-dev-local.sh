#!/bin/bash

# Function to kill all background processes
cleanup() {
    echo "Stopping services..."
    # Kill all processes in the same process group
    kill 0
}

# Trap EXIT signal to run cleanup function
trap cleanup EXIT

# Start backend
echo "Starting backend..."
(cd backend/HappyTools.WebApp && dotnet run) &

# Start frontend
echo "Starting frontend..."
(cd frontend && npm install && npm run dev) &

# Wait for all background processes to finish
wait
