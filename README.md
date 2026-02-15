# 🔥 OmniCommerceProject – Microservice Architecture

Microservice-based e-commerce backend built with .NET 8, MassTransit + RabbitMQ, PostgreSQL, and Docker.

This project demonstrates event-driven communication between services using message brokers and modern backend practices.

## 🏗 Architecture
backend/
  ├── OmniCommerce.Contracts
  ├── services/
        ├── OrderService
        └── PaymentService
frontend/
  └── omni-ui (WIP)
infra/
  └── docker-compose.yml

## Service Communication

* OrderService publishes events
* PaymentService consumes events
* Communication via RabbitMQ using MassTransit

## 🚀 Tech Stack

* .NET 8
* ASP.NET Core Web API
* MassTransit
* RabbitMQ
* PostgreSQL
* Entity Framework Core
* Docker & Docker Compose
* Swagger / OpenAPI

## 🧠 What This Project Demonstrates

* Microservice architecture design
* Event-driven communication
* Message broker integration
* Clean folder structuring
* Dockerized infrastructure
* API documentation with Swagger

## ▶ Running Locally
#### 1️⃣ Start Infrastructure
cd infra
docker compose up -d

#### 2️⃣ Run Services

Run OrderService and PaymentService separately.

Swagger endpoints:

* OrderService → http://localhost:xxxx/swagger
* PaymentService → http://localhost:5018/swagger

## 📸 Screenshots

(Add swagger screenshots here)

## 📌 Future Improvements

* JWT Authentication
* API Gateway
* Centralized logging
* Health checks
* CI/CD pipeline
