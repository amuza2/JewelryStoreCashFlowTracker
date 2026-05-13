# Jewelry Store Cash Flow Tracker - Implementation Plan

## Overview

This document contains the step-by-step implementation plan for the Jewelry Store Cash Flow Tracker desktop application. The plan follows the phases outlined in the specification document.

**Technology Stack:**
- **Frontend:** Avalonia UI with Material Design
- **Framework:** .NET with CommunityToolkit.Mvvm
- **Language:** C#
- **Database:** SQLite with Entity Framework Core
- **Logging:** Serilog
- **Validation:** FluentValidation
- **Testing:** xUnit

---

## Phase 1: Foundation

### 1.1 Project Setup
**Status:** Pending | **Priority:** High

Create the Avalonia project structure with .NET.

**Steps:**
- Create new Avalonia MVVM Application project
- Configure solution file
- Set up project references

---

### 1.2 Install NuGet Packages
**Status:** Pending | **Priority:** High

Install all required dependencies.

**Packages to Install:**
```
Avalonia
Avalonia.Desktop
Avalonia.Themes.Fluent
Avalonia.Fonts.Inter
Avalonia.Controls.Material
CommunityToolkit.Mvvm
Microsoft.EntityFrameworkCore.Sqlite
Microsoft.EntityFrameworkCore.Design
Serilog
Serilog.Sinks.File
FluentValidation
FluentValidation.DependencyInjectionExtensions
Microsoft.Extensions.DependencyInjection
xunit
xunit.runner.visualstudio
Microsoft.NET.Test.Sdk
```

---

### 1.3 Create Folder Structure
**Status:** Pending | **Priority:** High

Organize project with clean architecture folders:

```
/Models          - Domain entities (Transaction, DaySession, TransactionType)
/ViewModels      - MVVM ViewModels (Dashboard, Dialogs)
/Views           - Avalonia XAML views
/Services        - Business logic services (TransactionService, BalanceService, DaySessionService)
/Repositories    - Data access layer (IDaySessionRepository, ITransactionRepository)
/Database        - DbContext, migrations, configurations
/Helpers         - Utility classes
/Assets          - Icons, images, styles
/Validators      - FluentValidation validators
```

---

### 1.4 Create Domain Models
**Status:** Pending | **Priority:** High

Implement the core domain entities.

**Files to Create:**

**`/Models/TransactionType.cs`**
```csharp
public enum TransactionType
{
    Sale,       // Increases cash (Green)
    Purchase,   // Decreases cash (Red)
    Expense     // Decreases cash (Orange)
}
```

**`/Models/DaySession.cs`**
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
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
```

**`/Models/Transaction.cs`**
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
    public bool IsDeleted { get; set; }  // Soft delete for audit safety
}
```

---

### 1.5 Setup EF Core DbContext
**Status:** Pending | **Priority:** High

Configure Entity Framework with SQLite.

**Files to Create:**

**`/Database/AppDbContext.cs`**
```csharp
public class AppDbContext : DbContext
{
    public DbSet<DaySession> DaySessions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=jewelry_cashflow.db");
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.DaySession)
            .WithMany(s => s.Transactions)
            .HasForeignKey(t => t.DaySessionId);
    }
}
```

**Migrations:**
- Create initial migration
- Apply to database

---

### 1.6 Configure Dependency Injection
**Status:** Pending | **Priority:** High

Setup DI container for services and ViewModels.

**Files to Create/Update:**

**`App.axaml.cs`** - Register services:
```csharp
services.AddSingleton<AppDbContext>();
services.AddSingleton<IDaySessionRepository, DaySessionRepository>();
services.AddSingleton<ITransactionRepository, TransactionRepository>();
services.AddSingleton<IDaySessionService, DaySessionService>();
services.AddSingleton<ITransactionService, TransactionService>();
services.AddSingleton<IBalanceService, BalanceService>();
services.AddTransient<MainViewModel>();
services.AddTransient<DashboardViewModel>();
services.AddTransient<StartDayDialogViewModel>();
services.AddTransient<AddTransactionDialogViewModel>();
services.AddTransient<CloseDayDialogViewModel>();
```

---

## Phase 2: Core Logic

### 2.1 Create Repository Layer
**Status:** Pending | **Priority:** High

Implement explicit repositories for data access.

**Files to Create:**

**`/Repositories/IDaySessionRepository.cs`**
```csharp
public interface IDaySessionRepository
{
    Task<DaySession?> GetTodayOpenSessionAsync();
    Task<DaySession?> GetByDateAsync(DateOnly date);
    Task<bool> HasOpenSessionForDateAsync(DateOnly date);
    Task AddAsync(DaySession session);
    Task UpdateAsync(DaySession session);
    Task CloseSessionAsync(DaySession session, decimal countedCash);
    Task SaveChangesAsync();
}
```

**`/Repositories/ITransactionRepository.cs`**
```csharp
public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction);
    Task<List<Transaction>> GetBySessionIdAsync(int sessionId);
    Task<Transaction?> GetByIdAsync(int id);
    Task UpdateAsync(Transaction transaction);
    Task SoftDeleteAsync(int id);
    Task SaveChangesAsync();
}
```

**Implementation Classes:**
- `/Repositories/DaySessionRepository.cs`
- `/Repositories/TransactionRepository.cs`

---

### 2.2 Create Service Layer
**Status:** Pending | **Priority:** High

Implement business logic services.

**Files to Create:**

**`/Services/IBalanceService.cs`**
```csharp
public interface IBalanceService
{
    decimal CalculateCurrentBalance(DaySession session);
    decimal CalculateExpectedCash(DaySession session);
    (decimal totalSales, decimal totalPurchases, decimal totalExpenses) GetDailySummary(DaySession session);
}
```

**`/Services/IDaySessionService.cs`**
```csharp
public interface IDaySessionService
{
    Task<DaySession?> GetTodaySessionAsync();
    Task<DaySession> StartNewDayAsync(decimal initialCash);
    Task CloseDayAsync(decimal countedCash);
    Task<bool> CanAddTransactionAsync();
}
```

**`/Services/ITransactionService.cs`**
```csharp
public interface ITransactionService
{
    Task<Transaction> AddTransactionAsync(string description, TransactionType type, decimal amount, string? notes);
    Task<List<Transaction>> GetTodayTransactionsAsync();
    Task<List<Transaction>> GetTransactionsBySessionIdAsync(int sessionId);
}
```

**Implementation Classes:**
- `/Services/BalanceService.cs`
- `/Services/DaySessionService.cs`
- `/Services/TransactionService.cs`

**Balance Calculation Logic:**
```
Current Balance = InitialCash + Sum(Sales) - Sum(Purchases) - Sum(Expenses)
Difference = CountedCash - ExpectedCash
```

---

### 2.3 Implement FluentValidation
**Status:** Pending | **Priority:** Medium

Add validation for user inputs.

**Files to Create:**

**`/Validators/StartDayValidator.cs`**
```csharp
public class StartDayValidator : AbstractValidator<StartDayDialogViewModel>
{
    public StartDayValidator()
    {
        RuleFor(x => x.InitialCash)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial cash must be 0 or greater");
    }
}
```

**`/Validators/TransactionValidator.cs`**
```csharp
public class TransactionValidator : AbstractValidator<AddTransactionDialogViewModel>
{
    public TransactionValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");
        
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");
    }
}
```

**Validation Rules Summary:**
| Rule | Behavior |
|------|----------|
| InitialCash >= 0 | Reject invalid input |
| Amount > 0 | Reject invalid transaction |
| Description required | Disable save |
| No transaction on closed day | Block operation |
| Only one open session per day | Prevent duplicate opening |

---

### 2.4 Create ViewModels
**Status:** Pending | **Priority:** High

Implement MVVM ViewModels for UI binding.

**Files to Create:**

**`/ViewModels/MainViewModel.cs`**
- Manages navigation between views
- Handles startup flow detection
- Coordinates dialog opening

**`/ViewModels/DashboardViewModel.cs`**
- Properties: InitialCash, TotalSales, TotalPurchases, TotalExpenses, CurrentBalance
- Commands: StartDayCommand, AddSaleCommand, AddPurchaseCommand, AddExpenseCommand, CloseDayCommand
- Methods: RefreshDashboardAsync()

**`/ViewModels/StartDayDialogViewModel.cs`**
- Property: InitialCash (decimal)
- Command: SaveCommand, CancelCommand
- Validation: InitialCash >= 0

**`/ViewModels/AddTransactionDialogViewModel.cs`**
- Properties: Description, TransactionType, Amount, Notes
- Command: SaveCommand, CancelCommand
- Validation: Amount > 0, Description required

**`/ViewModels/CloseDayDialogViewModel.cs`**
- Properties: ExpectedCash (calculated), CountedCash (user input), Difference (calculated)
- Command: ConfirmCommand, CancelCommand
- Validation: CountedCash >= 0

---

## Phase 3: UI Implementation

### 3.1 Create MainWindow
**Status:** Pending | **Priority:** High

Main application window with navigation.

**File:** `/Views/MainWindow.axaml`

**Layout:**
```
┌─────────────────────────────────────────────┐
│  Jewelry Cash Flow Tracker          [_][X]  │
├─────────────────────────────────────────────┤
│                                             │
│  [Current Balance: $XXX.XX]                 │
│  [Today's Sales: $XXX.XX]                   │
│  [Today's Expenses: $XXX.XX]                │
│                                             │
├─────────────────────────────────────────────┤
│  Transaction Table                          │
│  ┌─────────┬─────────────┬──────┬────────┐  │
│  │ Time    │ Description │ Type │ Amount │  │
│  └─────────┴─────────────┴──────┴────────┘  │
├─────────────────────────────────────────────┤
│  [Add Sale] [Add Purchase] [Add Expense] [Close Day] │
└─────────────────────────────────────────────┘
```

**Features:**
- Startup flow detection (open session / closed session / no session)
- Navigation to appropriate dialogs
- Hosts DashboardView

---

### 3.2 Create DashboardView
**Status:** Pending | **Priority:** High

Main dashboard with Material Design cards.

**File:** `/Views/DashboardView.axaml`

**Components:**
1. **Balance Card** - Large display of current balance
2. **Metrics Cards** (3 cards in row):
   - Total Sales (Green accent)
   - Total Purchases (Red accent)
   - Total Expenses (Orange accent)
3. **Transaction List** - DataGrid with columns:
   - Time (DateTime)
   - Description (string)
   - Type (Sale/Purchase/Expense with color coding)
   - Money In (decimal)
   - Money Out (decimal)
   - Running Balance (decimal)

**Features:**
- Sort by date (default)
- Filter by type
- Search by description
- Real-time balance updates

---

### 3.3 Create StartDayDialog
**Status:** Pending | **Priority:** High

Dialog for entering initial cash at day start.

**File:** `/Views/StartDayDialog.axaml`

**Fields:**
- Initial Cash (Numeric input, decimal)
- [Start Day] [Cancel]

**Behavior:**
- Modal dialog
- Blocks access to dashboard until completed
- Validates InitialCash >= 0
- Saves to DaySession table

---

### 3.4 Create AddTransactionDialog
**Status:** Pending | **Priority:** High

Dialog for recording cash movements.

**File:** `/Views/AddTransactionDialog.axaml`

**Fields:**
- Type (ComboBox: Sale/Purchase/Expense)
- Description (TextBox)
- Amount (Numeric input, decimal)
- Notes (TextBox, optional)
- [Save] [Cancel]

**Type Color Coding:**
- Sale: Green (#4CAF50)
- Purchase: Red (#F44336)
- Expense: Orange (#FF9800)

**Behavior:**
- Validates Amount > 0
- Validates Description is not empty
- Saves with current timestamp
- Refreshes dashboard on save

---

### 3.5 Create CloseDayDialog
**Status:** Pending | **Priority:** High

Dialog for end-of-day verification.

**File:** `/Views/CloseDayDialog.axaml`

**Fields:**
- Expected Cash (readonly, calculated from balance)
- Counted Cash (Numeric input, user-entered)
- Difference (readonly, calculated: Counted - Expected)
- [Confirm Close] [Cancel]

**Difference Display:**
- Positive (green): More cash than expected
- Negative (red): Less cash than expected
- Zero (neutral): Balanced

**Behavior:**
- Sets DaySession.IsClosed = true
- Sets DaySession.CountedCash and Difference
- Prevents further transactions
- Generates daily report

---

### 3.6 Implement Startup UX Flow
**Status:** Pending | **Priority:** High

Handle different startup scenarios.

**Logic:**
```
On Application Startup:
├── Check today's DaySession
│   ├── No session exists → Show StartDayDialog
│   │   └── User enters InitialCash → Create DaySession → Load Dashboard
│   ├── Open session exists → Load Dashboard immediately
│   └── Closed session exists → Show options:
│       ├── View Report → Show DailyReport
│       └── Start Next Day → Show StartDayDialog
```

**Implementation:**
- Modify `MainViewModel` constructor or `OnInitialized`
- Check `IDaySessionService.GetTodaySessionAsync()`
- Route to appropriate view based on session state

---

## Phase 4: UI Polish

### 4.1 Apply Material Design Styling
**Status:** Pending | **Priority:** Medium

Implement Material Design principles with color coding.

**Files:**
- `/Assets/MaterialStyles.axaml` - Shared styles
- Update all Views with Material Design controls

**Color Scheme:**
| Element | Color |
|---------|-------|
| Sale buttons/badges | Green (#4CAF50) |
| Purchase buttons/badges | Red (#F44336) |
| Expense buttons/badges | Orange (#FF9800) |
| Primary | Indigo (#3F51B5) |
| Background | Light gray (#FAFAFA) |

**Components to Style:**
- Floating Action Buttons for main actions
- Cards for dashboard metrics
- DataGrid with alternating rows
- TextBoxes with Material underline
- Dialogs with rounded corners and shadows

---

### 4.2 Add Responsive Layout
**Status:** Pending | **Priority:** Low

Ensure UI adapts to window resizing.

**Features:**
- Grid columns that resize proportionally
- ScrollViewer for transaction list
- StackPanel wrapping for action buttons on small screens
- Min/Max window size constraints

---

### 4.3 Add UI Animations
**Status:** Pending | **Priority:** Low

Subtle animations for better UX.

**Animations:**
- Dialog open/close fade + scale
- Button hover effects
- Balance update number transition
- Card loading fade-in

---

## Phase 5: Testing & Logging

### 5.1 Unit Tests - BalanceService
**Status:** Pending | **Priority:** Medium

Test balance calculation logic.

**Test Cases:**
```csharp
[Fact]
public void CalculateCurrentBalance_WithOnlyInitialCash_ReturnsInitialCash()

[Fact]
public void CalculateCurrentBalance_WithSales_IncreasesBalance()

[Fact]
public void CalculateCurrentBalance_WithPurchases_DecreasesBalance()

[Fact]
public void CalculateCurrentBalance_WithExpenses_DecreasesBalance()

[Fact]
public void CalculateCurrentBalance_WithMixedTransactions_CalculatesCorrectly()

[Fact]
public void GetDailySummary_ReturnsCorrectTotals()
```

---

### 5.2 Unit Tests - DaySession State Transitions
**Status:** Pending | **Priority:** Medium

Test day opening and closing logic.

**Test Cases:**
```csharp
[Fact]
public void StartNewDay_WithValidInitialCash_CreatesOpenSession()

[Fact]
public void StartNewDay_WhenSessionAlreadyExists_ThrowsException()

[Fact]
public void CloseDay_WithCorrectCount_SetsIsClosed()

[Fact]
public void CloseDay_CalculatesCorrectDifference()

[Fact]
public void AddTransaction_ToClosedSession_ThrowsException()
```

---

### 5.3 Unit Tests - Validation Rules
**Status:** Pending | **Priority:** Medium

Test input validation logic.

**Test Cases:**
```csharp
[Fact]
public void ValidateInitialCash_NegativeValue_Fails()

[Fact]
public void ValidateInitialCash_ZeroOrPositive_Passes()

[Fact]
public void ValidateTransactionAmount_ZeroOrNegative_Fails()

[Fact]
public void ValidateTransactionAmount_Positive_Passes()

[Fact]
public void ValidateDescription_Empty_Fails()

[Fact]
public void ValidateDescription_NonEmpty_Passes()
```

---

### 5.4 Configure Serilog Logging
**Status:** Pending | **Priority:** Medium

Setup logging for key operations.

**File:** Update `App.axaml.cs`

**Configuration:**
```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("logs/cashflow-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

**Log Events:**
| Level | Event |
|-------|-------|
| INFO | Day session opened |
| INFO | Transaction added (type, amount, description) |
| INFO | Day session closed |
| WARNING | Attempt to modify closed session |
| WARNING | Validation failure |
| ERROR | Database connection failed |
| ERROR | Unexpected exception |

---

### 5.5 Manual Testing & Edge Cases
**Status:** Pending | **Priority:** Medium

Comprehensive manual testing.

**Test Scenarios:**

**Startup Flows:**
- [ ] First launch (no database) - should prompt for initial cash
- [ ] App reopen during same day - should load dashboard
- [ ] App reopen after closing day - should show report options

**Transaction Operations:**
- [ ] Add Sale - balance increases
- [ ] Add Purchase - balance decreases
- [ ] Add Expense - balance decreases
- [ ] Add multiple transactions - running balance correct
- [ ] Delete (soft) transaction - balance updates

**Day Closing:**
- [ ] Close with exact count - difference = 0
- [ ] Close with surplus - difference positive
- [ ] Close with shortage - difference negative
- [ ] Attempt transaction after close - blocked

**Validation:**
- [ ] Negative initial cash - rejected
- [ ] Zero transaction amount - rejected
- [ ] Empty description - rejected
- [ ] Duplicate day start - rejected

**Data Persistence:**
- [ ] Close app, reopen - data intact
- [ ] Restart computer - data intact
- [ ] Multiple days in database - can view history

---

## Appendix: Architecture Overview

### Layered Architecture

```
UI (Avalonia + Material Design)
          ↓
ViewModels (CommunityToolkit.Mvvm)
          ↓
Services (Business Logic)
          ↓
Repositories (Data Access)
          ↓
EF Core (ORM)
          ↓
SQLite (Database)
```

### Cross-Cutting Concerns
- **Serilog** - Logging throughout all layers
- **FluentValidation** - Input validation in ViewModels
- **Dependency Injection** - Service registration and resolution
- **xUnit** - Unit testing for Services

### Data Flow
1. User interacts with **View** (XAML)
2. **ViewModel** handles command and calls **Service**
3. **Service** applies business logic and calls **Repository**
4. **Repository** uses **EF Core** to persist to **SQLite**
5. Results flow back up to update **ViewModel** properties
6. **View** updates via data binding

---

## Summary

| Phase | Tasks | Focus |
|-------|-------|-------|
| Phase 1 | 6 | Project setup, models, database, DI |
| Phase 2 | 7 | Repositories, services, validation, ViewModels |
| Phase 3 | 7 | Views, dialogs, startup flow |
| Phase 4 | 2 | Material Design, styling |
| Phase 5 | 5 | Unit tests, logging, manual testing |
| **Total** | **27** | |

**Estimated Timeline:** 2-3 weeks for full implementation

**Next Steps:**
1. Begin Phase 1.1 - Project Setup
2. Follow tasks in order within each phase
3. Mark tasks as completed in this document
4. Test thoroughly after each phase
