using JewelryStoreCashFlowTracker.Database;
using JewelryStoreCashFlowTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.Repositories;

/// <summary>
/// Repository implementation for Transaction entity.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }

    public async Task<List<Transaction>> GetBySessionIdAsync(int sessionId)
    {
        return await _context.Transactions
            .Where(t => t.DaySessionId == sessionId && !t.IsDeleted)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public Task UpdateAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(int id)
    {
        var transaction = await _context.Transactions.FindAsync(id);
        if (transaction != null)
        {
            transaction.IsDeleted = true;
            _context.Transactions.Update(transaction);
        }
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
