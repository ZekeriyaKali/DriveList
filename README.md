# VehicleAdvertAI (DriveList)

VehicleAdvertAI is a full-stack vehicle price prediction platform built with .NET 8 and Python. The system estimates car prices based on user input and stores prediction results in MSSQL for tracking and analysis.

This project demonstrates backend development, API integration, authentication, and machine learning service communication in a real-world scenario.

---

## Project Overview

The application allows users to:

- Register and authenticate securely
- Predict vehicle prices using a machine learning model
- Store and view prediction history
- Use a credit-based system for predictions
- Interact with AI-powered diagnosis features (text, audio, image)

The system is designed using a layered architecture and integrates a separate Python-based ML service.

---

## Technologies Used

Backend:
- .NET 8 (ASP.NET Core MVC and Web API)
- Entity Framework Core
- MSSQL Server
- ASP.NET Identity

Machine Learning Service:
- Python (Flask API)

Other:
- IHttpClientFactory for API communication
- Rate Limiting middleware
- reCAPTCHA integration
- QRCoder for 2FA

---

## Architecture

The project follows a clean layered architecture:

- DriveListApi: Presentation layer (MVC + API)
- DriveList.Application: Business logic
- DriveList.Domain: Core entities
- DriveList.Infrastructure: Data access and persistence
- Python API: Machine learning service

---

## Key Features

- Secure authentication system with ASP.NET Identity
- Email confirmation and password reset
- Login audit logging (IP address, device info, success/failure tracking)
- Rate limiting and account lockout protection
- Two-factor authentication (2FA)
- External login support
- Credit-based usage system
- Vehicle price prediction via ML API
- Prediction history stored in database
- AI-based diagnosis module with multimedia input support

---

## How It Works

1. User registers and confirms email
2. User logs in to the system
3. User submits vehicle information
4. Backend sends request to Python ML API
5. Prediction result is returned
6. Result is stored in MSSQL
7. User can view previous predictions

---

## Sample Request

```json
{
  "brand": "BMW",
  "model": "3 Series",
  "year": 2018,
  "km": 85000,
  "gearType": "Automatic",
  "fuelType": "Diesel",
  "city": "Istanbul"
}

---

Security
reCAPTCHA validation during registration
Login rate limiting
Account lockout mechanism
Email verification requirement
Two-factor authentication
Login audit tracking
Credit System
Each prediction consumes 1 credit
Users can purchase credits (mock implementation)
All transactions are stored in the database

---

Project Status
This project is currently under active development.

---

Planned improvements:

Integration with real payment providers (Stripe or Iyzico)
Improved machine learning model accuracy
Docker support
Cloud deployment (Azure or AWS)
Mobile client (Flutter)
Advanced analytics dashboard


Setup
Clone the repository

git clone https://github.com/yourusername/VehicleAdvertAI.git

Configure database

Update appsettings.json and run migrations

Run .NET project

dotnet run

Run Python API

python app.py

---

Notes
Python API runs on localhost:5000
.NET communicates with Python via HTTP requests
Both services must be running at the same time
