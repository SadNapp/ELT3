# 📈 Stock Quote ETL Service

## 🌟 Project Overview

This small console application demonstrates the implementation of a basic **ETL (Extract, Transform, Load)** process, designed to regularly retrieve current stock prices and load them into a relational database.

**ETL stands for:**
* **E**xtract: Retrieving data from a source (in this case, a financial API).
* **T**ransform: Cleaning, standardizing, and unifying the data format (e.g., standardizing timestamps to UTC).
* **L**oad: Writing the processed data into a target system (PostgreSQL).

The project was developed to practice skills with **.NET Core, EF Core, and PostgreSQL** integration, and to showcase an understanding of concepts like **Dependency Injection (DI)** and service lifecycle management.

### Core Features:

1.  **Extraction:** Fetches current stock quotes (price, percentage change, last update time) for a defined list of symbols (`AAPL`, `MSFT`, `GOOGL`, etc.) via a third-party API.
2.  **Transformation:** Normalizes the incoming data, crucially standardizing all timestamps to **UTC** before persistence.
3.  **Loading:** Persists the transformed data into the **PostgreSQL** database.
4.  **Analysis (Add-on):** Performs a basic comparison of the latest prices against historical data (e.g., prices recorded one hour prior).

---

## 🛠️ Technology Stack

The stack was chosen to demonstrate proficiency with a modern, production-ready .NET ecosystem.

| Category | Technology / Library | Description and Purpose |
| :--- | :--- | :--- |
| **Backend / Runtime** | **.NET 9.0 (Console App)** | The primary framework for executing the standalone ETL process. |
| **Database** | **PostgreSQL** | A robust relational database used for persistent and historical storage of quote data. |
| **ORM** | **Entity Framework Core (EF Core)** | Used for database interaction, model configuration, and schema management. |
| **PostgreSQL Provider** | **Npgsql.EntityFrameworkCore.PostgreSQL** | The official EF Core provider for connecting and working with PostgreSQL. |
| **API Client** | **YahooFinanceApi** | A dedicated library for reliable and simplified retrieval of public stock data. |
| **Architecture** | **Dependency Injection (DI)** | Utilization of the built-in .NET Core container to manage service lifetimes (`YahooApiClient`, `AppDbContext`) within a confined execution scope. |

---

## 💡 Engineering Decisions

* **Schema Management Strategy:** Instead of complex migrations, the project uses **`dbContext.Database.EnsureCreatedAsync()`** for initial database setup. This is a pragmatic choice for smaller, self-contained ETL projects, ensuring the schema exists without needing manual `dotnet ef` commands.
* **Time Standardization:** All timestamp values (`RecordedAt`) are explicitly stored in the **UTC** format (`DateTime.UtcNow`). This prevents data integrity issues and inconsistencies related to server time zones and daylight savings.
* **Service Lifecycle Control:** The database context and service providers are retrieved within a clearly defined **`using (var scope = ...)`** block. This guarantees that `AppDbContext` is correctly scoped and disposed of after the ETL run, ensuring proper resource management.

***

Would you like me to help you set up an automated scheduler (like a Cron job or a Windows Task Scheduler) to run this script every hour?
