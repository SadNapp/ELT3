# 🚀 ELT3 Project
> Modern data processing pipeline for stock tracking and analytics.

**ELT3 (Extract, Load, Transform)** is a robust backend engine written in C#/.NET. It is designed to automatically acquire, persist, and analyze stock market data (AAPL, MSFT, TSLA, etc.) at scheduled intervals. This project showcases structured background processing, reliable containerization, and modern architecture best practices.

---

## 🛠 Tech Stack

- **Backend:** C# ASP.NET Core API, .NET Worker Services (`IHostedService`)
- **Database:** PostgreSQL, Entity Framework Core (EF Core)
- **DevOps & Infrastructure:** Docker, Docker Compose
- **Logging & Monitoring:** Serilog, Swagger/OpenAPI

---

## ✨ Core Features

1. **Continuous Data Extraction 🔄**  
   Utilizes a built-in `BackgroundService` (`StockBackgroundWorker`) to trigger data ingestion every 5 minutes automatically without external cron jobs. Fetches reliable market data securely via a dedicated `YahooApiClient`.
   
2. **Transform Logic & Analytics 🧮**  
   Raw Unix timestamps and API data are explicitly parsed and mapped into structured `StockQuote` domain models using precise LINQ data projection. Includes an automated internal transformation matrix (`AnalyzeChangesAsync`) that analyzes and generates absolute and percentage price fluctuations hour-over-hour.

3. **Database Management & Code-First 🗄️**  
   Fully leverages PostgreSQL interacting with EF Core. Features **Automated Code-First Migrations** injected on startup (`context.Database.MigrateAsync()`) within an isolated `IServiceScope`. This ensures the database is automatically kept in sync with code models flawlessly on boot without executing manual CLI commands.

4. **Resilience & Fault Tolerance 🛡️**  
   Engineered for maximum uptime. Gracefully handles external API rate-limiting gracefully, catching out-of-bounds exceptions, and skipping cycles efficiently. Furthermore, if the PostgreSQL database fails to run or connect during startup, the fallback mechanism drops a warning string but allows the app engine to stay alive.

5. **Infrastructure & Docker-First 🐳**  
   Environment agnostic deployment. Developed entirely around `compose.yaml` with explicit environment variable injection passing (`.env`), achieving secure, isolated container networking between internal API logic and the PostgreSQL layer.

6. **Rich Structured Logging 📝**  
   Serilog overrides native loggers persisting runtime events to both rolling files and the console with highly customized templates, explicitly filtering the background noise level produced by the Entity Framework.

---

## 🚀 How to Run Locally

1. Create a `.env` file in the root directory (based on `.env.example`).
2. Spin up the application leveraging Docker Compose:
   ```bash
   docker-compose up --build
   ```
3. Your database, background worker processes, and Web API will launch safely. Navigate to:  
   `http://localhost:<API_PORT>/swagger` to access the API exploration UI.

---
<hr>

# Українська Версія 🌿
> Сучасний пайплайн обробки даних для трекінгу та аналітики акцій.

**ELT3 (Extract, Load, Transform)** — це надійний бекенд-рушій, написаний на C#/.NET. Він призначений для автоматичного збору, збереження та аналізу даних фондового ринку (AAPL, MSFT, TSLA тощо) через задані інтервали часу. Проект демонструє навички роботи зі структурованими фоновими процесами, контейнеризацією та сучасними патернами архітектури рівня Junior+.

---

## 🛠 Технологічний Стек

- **Бекенд:** C# ASP.NET Core API, .NET Worker Services (`IHostedService`)
- **База Даних:** PostgreSQL, Entity Framework Core (EF Core)
- **DevOps та Інфраструктура:** Docker, Docker Compose
- **Логування та Моніторинг:** Serilog, Swagger/OpenAPI

---

## ✨ Ключові Фічі

1. **Вилучення Даних за Розкладом (Data Extraction) 🔄**  
   Використовує вбудований безперервний `BackgroundService` (`StockBackgroundWorker`) для автоматичного запуску процесу збору даних кожні 5 хвилин без залежності від зовнішніх cron-скриптів. Отримує котирування за допомогою локального `YahooApiClient`.
   
2. **Логіка Трансформації та Аналітика (Transform Logic) 🧮**  
   Сирі Unix-часові мітки та фінансові дані безпечно парсяться та мапляться у строго типізовані DTO-моделі (`StockQuote`) за допомогою проекції LINQ. Включає автоматизований алгоритм розрахунків (`AnalyzeChangesAsync`), який самостійно виявляє різницю цінових та відсоткових змін активу за останню годину.

3. **Управління Базою Даних (Database Management) 🗄️**  
   Повноцінне використання PostgreSQL та EF Core. Реалізовано **Автоматичні Code-First Міграції** при запуску додатку через `context.Database.MigrateAsync()` всередині ізольованого Service Scope. Це гарантує актуальність бази даних без необхідності ручних CLI команд в інфраструктурі.

4. **Відмовостійкість (Resilience) 🛡️**  
   Проект захищено від раптових падінь у рантаймі (Crash Prevented). Гнучко перехоплюються проблеми лімітів звернень до API або мережеві помилки. Крім того, при помилці підключення до БД інфраструктури на старті, додаток виводить Warning, але продовжує життєвий цикл без Fatal Error-блокування.

5. **Інфраструктура (Docker-First) 🐳**  
   Деплоймент, що не залежить від ОС. Вся архітектура розгортається лише однією командою завдяки `compose.yaml` із передачею секретів через файл `.env`. Створюється надійна внутрішня мережа між мікроконтейнером С# та PostgreSQL.

6. **Структуроване Логування 📝**  
   Впроваджено Serilog, який записує події у щоденні ротаційні файли (rolling files) та консоль. Активно застосовуються кастомні шаблони та гнучке фільтрування Entity Framework-повідомлень, щоб зберегти лише критичні логи продукту.

---

## 🚀 Як Запустити Локально

1. Створіть файл `.env` у кореневій папці (можна взяти за шаблон `.env.example`).
2. Запустіть додаток за допомогою Docker Compose:
   ```bash
   docker-compose up --build
   ```
3. API та Background Worker автоматично запустяться. Ви можете побачити Swagger UI за адресою:  
   `http://localhost:<API_PORT>/swagger`
