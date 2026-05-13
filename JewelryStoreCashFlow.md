# Jewelry Store Cash Flow Tracker

## Software Specification Document

---

# 1. Project Overview

## Project Name

**Jewelry Cash Flow Tracker**

## Goal

Create a lightweight desktop application for jewelry stores to track daily cash movement.

The software focuses only on:

* cash entering the store,
* cash leaving the store,
* automatic balance calculation,
* end-of-day cash verification.

This is intentionally a simple MVP and not a full accounting system.

---

# 2. Target Users

* Small jewelry stores
* Single-owner shops
* Small teams
* Businesses currently using paper or spreadsheets

---

# 3. Technology Stack

## Frontend UI

* Avalonia UI
* [Avalonia UI Official Website](https://avaloniaui.net/?utm_source=chatgpt.com)

## MVVM Framework

* CommunityToolkit.Mvvm
* [CommunityToolkit.Mvvm Documentation](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/?utm_source=chatgpt.com)

## UI Styling

* Avalonia Material Design
* [Avalonia UI GitHub](https://github.com/AvaloniaUI/Avalonia?utm_source=chatgpt.com)

## Language

* C#

## Database

### MVP

* SQLite

### ORM (Optional)

* Entity Framework Core

---

# 4. Application Objectives

The application must allow the user to:

* Start the day with an initial cash amount
* Record sales (money in)
* Record purchases/expenses (money out)
* Automatically calculate current balance
* View transaction history
* Verify end-of-day cash
* Detect differences between expected and counted cash

---

# 5. Functional Requirements

# 5.1 Start Day

## Description

The user enters the starting cash amount at the beginning of the day.

## Fields

| Field        | Type    |
| ------------ | ------- |
| Start Date   | Date    |
| Initial Cash | Decimal |

## Rules

* Only one start balance per day
* Required before adding transactions

---

# 5.2 Add Transaction

## Description

The user records cash movement.

## Transaction Types

| Type     | Effect        |
| -------- | ------------- |
| Sale     | Increase cash |
| Purchase | Decrease cash |
| Expense  | Decrease cash |

---

## Transaction Fields

| Field       | Type              |
| ----------- | ----------------- |
| Id          | Integer           |
| DateTime    | DateTime          |
| Description | String            |
| Type        | Enum              |
| Amount      | Decimal           |
| Notes       | String (Optional) |

---

## Balance Logic

For each transaction:

```text id="4k5p7d"
Balance = Previous Balance + MoneyIn - MoneyOut
```

---

# 5.3 Transaction List

## Features

* Display all transactions
* Sort by date
* Filter by type
* Search by description

## Columns

| Column          |
| --------------- |
| Time            |
| Description     |
| Type            |
| Money In        |
| Money Out       |
| Running Balance |

---

# 5.4 Dashboard

## Information Displayed

| Metric          |
| --------------- |
| Initial Cash    |
| Total Sales     |
| Total Purchases |
| Total Expenses  |
| Current Balance |

---

# 5.5 End-of-Day Verification

## Description

At closing time, the employee counts physical cash.

## Fields

| Field             |
| ----------------- |
| Expected Cash     |
| Real Counted Cash |
| Difference        |

---

## Difference Logic

```text id="u2v9tx"
Difference = RealCountedCash - ExpectedCash
```

---

# 5.6 Daily Closing

## Features

* Lock the day
* Prevent further modifications
* Generate daily report

---

# 6. Non-Functional Requirements

## Performance

* Application should load under 3 seconds
* Transactions should save instantly

## Offline Support

* Fully offline desktop application

## Usability

* Simple UI
* Large buttons
* Minimal training required

## Reliability

* Automatic database save
* Data persistence after shutdown

---

# 7. UI/UX Specification

# 7.1 Design Style

Use:

* Material Design principles
* Clean layout
* Minimal interface

---

# 7.2 Main Window Layout

## Top Section

* Current balance card
* Today's sales
* Today's expenses

## Center Section

* Transaction table

## Right/Bottom Section

Action buttons:

* Add Sale
* Add Purchase
* Add Expense
* Close Day

---

# 7.3 Colors

| Action   | Suggested Color |
| -------- | --------------- |
| Sale     | Green           |
| Purchase | Red             |
| Expense  | Orange          |

---

# 8. Architecture

# 8.1 Pattern

* MVVM

---

# 8.2 Suggested Folder Structure

```text id="t9w1aq"
/Models
/ViewModels
/Views
/Services
/Repositories
/Database
/Helpers
/Assets
```

---

# 8.3 Suggested Models

## Transaction Model

```csharp
public class Transaction
{
    public int Id { get; set; }

    public DateTime DateTime { get; set; }

    public string Description { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string? Notes { get; set; }
}
```

---

## TransactionType Enum

```csharp
public enum TransactionType
{
    Sale,
    Purchase,
    Expense
}
```

---

# 9. Suggested Services

| Service            | Responsibility      |
| ------------------ | ------------------- |
| TransactionService | CRUD operations     |
| BalanceService     | Balance calculation |
| ReportService      | Daily reports       |
| DatabaseService    | SQLite access       |

---

# 10. Suggested Screens

| Screen                 |
| ---------------------- |
| Dashboard              |
| Add Transaction Dialog |
| Daily Report           |
| Settings               |

---

# 11. MVP Scope

## Included

* Daily cash tracking
* Transaction history
* Automatic balance
* Daily closing
* SQLite persistence

## Not Included Yet

* Multi-user support
* Authentication
* Barcode scanning
* Inventory management
* Cloud synchronization
* Accounting integration

---

# 12. Future Improvements

Possible future versions:

* Inventory tracking
* Jewelry weight tracking
* Gold price integration
* Invoice printing
* PDF reports
* Backup system
* Export to Excel/PDF
* Multi-store support
* Analytics dashboard

---

# 13. Development Plan

## Phase 1 — Foundation

* Setup Avalonia project
* Configure MVVM
* Setup SQLite
* Create models

## Phase 2 — Core Features

* Add transaction screen
* Balance calculation
* Dashboard

## Phase 3 — Persistence

* Save/load data
* Daily reports

## Phase 4 — UI Polish

* Material styling
* Responsive layout
* Animations

## Phase 5 — Testing

* Manual testing
* Edge cases
* Data validation

---

# 14. Success Criteria

The software is considered successful if:

* Employees can use it without training
* Daily balance matches physical cash
* Transactions are recorded quickly
* End-of-day verification becomes easier
* Paper tracking is reduced or eliminated


This is actually a very good review. The reviewer identified the most important architectural issue: the missing `DaySession` aggregate/root entity.

The biggest hidden problem in the original specification was that transactions had no ownership boundary. Once you introduce daily opening/closing logic, the “day” itself becomes a first-class entity.

The recommendation to add `DaySession` is correct and important.

I also agree strongly with these points:

* validation rules,
* startup UX,
* closed-day restrictions,
* defining report contents.

Those are the kinds of details that prevent architecture drift during implementation.

I would refine the architecture like this:

# Recommended Core Domain Model

## DaySession

```csharp
public class DaySession
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public decimal InitialCash { get; set; }

    public bool IsClosed { get; set; }

    public decimal? CountedCash { get; set; }

    public decimal? Difference { get; set; }

    public DateTime OpenedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public ICollection<Transaction> Transactions { get; set; }
        = new List<Transaction>();
}
```

---

# Updated Transaction Model

```csharp
public class Transaction
{
    public int Id { get; set; }

    public int DaySessionId { get; set; }

    public DaySession DaySession { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Description { get; set; }

    public TransactionType Type { get; set; }

    public decimal Amount { get; set; }

    public string? Notes { get; set; }
}
```

---

# Recommended Balance Strategy

The reviewer asked an important question:
should running balance be stored or calculated?

For your MVP:

* DO NOT store running balance in database.
* Calculate dynamically.

Reason:

* simpler architecture,
* fewer synchronization bugs,
* easier corrections,
* SQLite + small transaction volume makes recalculation cheap.

So the spec should explicitly say:

> Running balances are calculated dynamically in the application layer and are not persisted in the database.

That is the correct decision for this project stage.

---

# Recommended Validation Rules Section

You should add a dedicated section:

## Validation Rules

| Rule                          | Behavior                   |
| ----------------------------- | -------------------------- |
| InitialCash must be ≥ 0       | Reject invalid input       |
| Amount must be > 0            | Reject invalid transaction |
| Description required          | Disable save               |
| No transaction on closed day  | Block operation            |
| Only one open session per day | Prevent duplicate opening  |
| Cannot close empty session    | Optional business rule     |

---

# Recommended Startup UX

This is important.

## Recommended Flow

### Case 1 — No Open Session Today

Application opens directly to:

* “Start New Day” dialog

Fields:

* Initial Cash

User cannot access dashboard before creating session.

---

### Case 2 — Existing Open Session

Application loads dashboard immediately.

---

### Case 3 — Closed Session Today

Application offers:

* View report
* Start next day

---

# Recommended Architecture Clarification

The system naturally fits a lightweight layered architecture:

```text id="t9z4bf"
Views
↓
ViewModels
↓
Services
↓
Repositories
↓
SQLite Database
```

This is cleaner than letting ViewModels directly access EF Core.

---

# Repository Recommendation

Instead of generic repositories, I recommend explicit repositories.

Example:

```csharp
public interface IDaySessionRepository
{
    Task<DaySession?> GetTodayOpenSessionAsync();

    Task AddAsync(DaySession session);

    Task CloseSessionAsync(DaySession session);

    Task SaveChangesAsync();
}
```

And:

```csharp
public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);

    Task<List<Transaction>> GetBySessionIdAsync(int sessionId);

    Task SaveChangesAsync();
}
```

For business software, explicit repositories are usually clearer than generic CRUD abstractions.

---

# Another Missing Detail: Audit Safety

Even for MVP, jewelry stores deal with money.

I strongly recommend adding:

## Soft Protection Rule

After transaction creation:

* editing allowed,
* deletion restricted,
* or log deletions.

Otherwise balance history becomes unreliable.

Even a simple field:

```csharp
public bool IsDeleted { get; set; }
```

can help later.

---

# Final Assessment

The original specification was already solid for MVP scope.

The review improved it significantly by:

* introducing a proper aggregate root (`DaySession`),
* forcing lifecycle thinking,
* clarifying state transitions,
* identifying implementation ambiguities.

The architecture is now becoming closer to a real production-ready business application rather than just a spreadsheet replacement.

And honestly:
Avalonia + MVVM Toolkit + SQLite is a very good stack for this kind of local business desktop software.


Your stack choice is very good for this project. It is modern, maintainable, and still lightweight enough for an MVP.

Here’s how I would evaluate each technology for your jewelry cash tracking software.

---

# Recommended Stack

| Technology            | Recommendation |
| --------------------- | -------------- |
| Entity Framework Core | Yes            |
| xUnit                 | Yes            |
| Serilog               | Yes            |
| FluentValidation      | Yes            |
| Dependency Injection  | Yes            |

This is honestly a professional stack.

---

# 1. Entity Framework Core

Very good choice here.

Why EF Core fits this project:

* SQLite support is excellent
* Easy migrations
* Simple CRUD
* Strong typing
* Good LINQ support
* Fast development

Your domain is relational:

* DaySession → Transactions

EF Core handles this naturally.

Example relationship:

```csharp id="r99dgm"
modelBuilder.Entity<Transaction>()
    .HasOne(t => t.DaySession)
    .WithMany(s => s.Transactions)
    .HasForeignKey(t => t.DaySessionId);
```

For a local business app:
EF Core + SQLite is probably the sweet spot.

I would NOT use:

* Dapper,
* CQRS,
* MediatR,
* event sourcing,
  for this MVP.

That would overcomplicate things.

---

# 2. xUnit

Good decision.

Even small business apps benefit from tests because:

* balance logic is critical,
* money calculations must be reliable,
* regressions are dangerous.

Important areas to test:

* balance calculations,
* day closing logic,
* validation,
* transaction restrictions.

Example:

```csharp id="upixev"
[Fact]
public void Sale_Should_Increase_Balance()
{
}
```

You do not need massive test coverage for MVP.

Focus on:

* business rules,
* calculations,
* state transitions.

---

# 3. Serilog

Excellent choice.

Especially for:

* debugging customer issues,
* crash diagnostics,
* audit troubleshooting.

Very useful in desktop apps.

Recommended sinks:

* File sink only for MVP.

Example logs:

```text id="7ajq8m"
[INFO] Day session opened
[INFO] Transaction added
[WARNING] Attempt to modify closed session
[ERROR] Database connection failed
```

This becomes VERY valuable later.

---

# 4. FluentValidation

Very good fit with MVVM.

Cleaner than:

* giant if-statements,
* validation inside ViewModels.

Example:

```csharp id="6ttc8p"
RuleFor(x => x.Amount)
    .GreaterThan(0);
```

This keeps validation:

* reusable,
* testable,
* centralized.

Excellent choice.

---

# 5. Dependency Injection

Absolutely yes.

Even for small apps.

Benefits:

* cleaner ViewModels,
* easier testing,
* better architecture,
* easier future growth.

Avalonia works well with DI.

You can register:

```csharp id="2mkww0"
services.AddSingleton<ITransactionService, TransactionService>();
services.AddTransient<MainViewModel>();
```

This is enough.

---

# What I Would NOT Add Yet

For this MVP, avoid:

* CQRS
* MediatR
* AutoMapper
* Microservices
* Event Bus
* Domain Events
* Redis
* Message Queues

You are building:

* a local desktop business tool,
  not a distributed enterprise system.

Keep architecture proportional to the problem.

---

# Recommended Final Architecture

```text id="d6jv3q"
UI (Avalonia)
↓
ViewModels
↓
Services
↓
Repositories
↓
EF Core
↓
SQLite
```

Cross-cutting:

* Serilog
* FluentValidation
* Dependency Injection
* xUnit

This is clean and professional.

---

# One More Recommendation

Add:

| Technology  | Recommendation |
| ----------- | -------------- |
| Async/Await | YES            |

Even with SQLite.

Use async for:

* DB operations,
* loading transactions,
* reports.

It keeps UI responsive.

---

# My Overall Assessment

Your stack is:

* modern,
* maintainable,
* scalable enough,
* not overengineered,
* suitable for real business software.

This is the kind of architecture many professional internal business desktop applications use today.
