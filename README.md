# Full-Stack Development Framework

This project is a starter framework for full-stack applications, featuring a .NET backend and a Next.js frontend.

## Technology Stack

*   **Backend:** .NET 8, ASP.NET Core Web API
*   **Frontend:** Next.js, React, Tailwind CSS
*   **Database:** PostgreSQL (configured by default)

## Getting Started

### Prerequisites

*   .NET 8 SDK
*   Node.js (latest LTS version)
*   Docker (for running database)

### 1. Backend Setup

1.  Navigate to the `backend/AmazonTrends.WebApp` directory.
2.  Update the `DefaultConnection` string in `appsettings.Development.json` to point to your PostgreSQL database.
3.  Run `dotnet run` to start the backend server.
4.  The API will be available at `https://localhost:7031` (or a similar port), and a Swagger UI at the root.

### 2. Frontend Setup

1.  Navigate to the `frontend` directory.
2.  Run `npm install` to install dependencies.
3.  Run `npm run dev` to start the frontend development server.
4.  The application will be available at `http://localhost:3000`.

## Project Structure

*   `/backend`: Contains the .NET solution.
    *   `AmazonTrends.WebApp`: The main ASP.NET Core application.
    *   `AmazonTrends.Core`: For business logic.
    *   `AmazonTrends.Data`: For data access and models.
*   `/frontend`: Contains the Next.js application.
*   `/docs`: Contains project documentation.