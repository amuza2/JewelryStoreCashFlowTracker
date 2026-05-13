using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.Services;

/// <summary>
/// Service implementation for transaction management.
/// </summary>
public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IDaySessionRepository _daySessionRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IDaySessionRepository daySessionRepository)
    {
        _transactionRepository = transactionRepository;
        _daySessionRepository = daySessionRepository;
    }

    public async Task<Transaction> AddTransactionAsync(
        string description,
        TransactionType type,
        decimal amount,
        string? notes)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
        }

        // Get today's open session
        var session = await _daySessionRepository.GetTodayOpenSessionAsync();
        if (session == null)
        {
            throw new InvalidOperationException("No open session found for today. Please start the day first.");
        }

        // Create transaction
        var transaction = new Transaction
        {
            DaySessionId = session.Id,
            CreatedAt = DateTime.Now,
            Description = description.Trim(),
            Type = type,
            Amount = amount,
            Notes = notes?.Trim(),
            IsDeleted = false
        };

        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        Log.Information("Transaction added: {Type} - {Description} - {Amount:C}",
            type, description, amount);

        return transaction;
    }

    public async Task<List<Transaction>> GetTodayTransactionsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var session = await _daySessionRepository.GetByDateAsync(today);
        if (session == null)
        {
            return new List<Transaction>();
        }

        return session.Transactions
            .Where(t => !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }

    public async Task<List<Transaction>> GetTransactionsBySessionIdAsync(int sessionId)
    {
        return await _transactionRepository.GetBySessionIdAsync(sessionId);
    }

    public async Task SoftDeleteTransactionAsync(int transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        if (transaction == null)
        {
            throw new InvalidOperationException("Transaction not found.");
        }

        // Check if session is closed
        var session = await _daySessionRepository.GetByDateAsync(
            DateOnly.FromDateTime(DateTime.Now));
        if (session?.IsClosed == true)
        {
            throw new InvalidOperationException("Cannot delete transactions from a closed session.");
        }

        await _transactionRepository.SoftDeleteAsync(transactionId);
        await _transactionRepository.SaveChangesAsync();

        Log.Information("Transaction {TransactionId} soft deleted", transactionId);
    }

    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        // Validate inputs
        if (string.IsNullOrWhiteSpace(transaction.Description))
        {
            throw new ArgumentException("Description is required.", nameof(transaction));
        }

        if (transaction.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.", nameof(transaction));
        }

        // Check if the session is closed
        var session = await _daySessionRepository.GetByIdAsync(transaction.DaySessionId);
        if (session?.IsClosed == true)
        {
            throw new InvalidOperationException("Cannot edit transactions from a closed session.");
        }

        // Update the transaction
        transaction.Description = transaction.Description.Trim();
        transaction.UpdatedAt = DateTime.UtcNow;

        await _transactionRepository.UpdateAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        Log.Information("Transaction updated: {Id} - {Description} - {Amount:C}",
            transaction.Id, transaction.Description, transaction.Amount);
    }
}
