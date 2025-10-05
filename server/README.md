# Chinese Auction Management System

A full-stack web application for managing Chinese (Silent) Auctions. Supports the full lifecycle: setup, sales, lottery drawing, and results—with secure server-side payments and operational reporting.

Overview

Multi-role access (Administrators, Customers)

Email notifications and reporting (including Excel export)

Cryptographically secure lottery draws

Emphasis on correctness, security, and performance

Technology Stack

Backend (.NET 9): ASP.NET Core Web API, EF Core, SQL Server, JWT, SMTP, structured logging, Stripe .NET SDK (server-side only)
Frontend (React 18): TypeScript, PrimeReact/PrimeFlex, React Router, Axios, Vite (no Stripe client SDK)

Quick Start

Prerequisites: .NET 9 SDK, Node.js 18+, SQL Server (LocalDB/Express/Full), optional SMTP

Database

cd server
dotnet ef database update


Backend

cd server
dotnet restore
dotnet run
# API: https://localhost:5234


Frontend

cd client
npm install
npm run dev
# Web: http://localhost:5173

Key Features

Auction Management: donors, categories, gifts, availability, pricing

Payments (Server-Side Only): Stripe integration handled by the ASP.NET Core API (no client-side Stripe SDK); receipts and purchase history exposed via API

Lottery: unbiased, cryptographically secure selection; multiple tickets per user

Results & Notifications: winner display and email delivery

Reporting: summaries by gift/donor/category; Excel export

Security

JWT-based authentication and role authorization

Secure RNG for lottery (RandomNumberGenerator)

Input validation/sanitization; EF-based SQL injection safeguards

Configurable CORS

Performance

Aggregate queries to eliminate N+1 patterns

Indexing of frequently accessed columns

Optimized EF queries with appropriate includes

(Optional) caching strategies

Configuration

Environment variables / settings:

DefaultConnection — SQL Server connection string

JwtConfig:Secret — JWT signing key

Stripe:SecretKey — Stripe API key (used by server only)

EmailSettings:* — SMTP configuration (host/port/user/password/from)

Documentation

docs/PERFORMANCE.md — performance strategy and indexes

docs/SECURE_RANDOM.md — secure RNG design and rationale

Swagger is available when the backend is running

Testing
# Backend
cd server
dotnet test

# Frontend
cd client
npm test

License

MIT License. See LICENSE.
