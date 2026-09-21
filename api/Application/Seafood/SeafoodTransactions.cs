using Microsoft.EntityFrameworkCore;
using SqlServ4r.EntityFramework;

namespace Application.Seafood;

internal static class SeafoodTransactions
{
    public static Task ExecuteAsync(DreamContext db, Func<Task> work)
        => ExecuteAsync(db, async () =>
        {
            await work();
            return true;
        });

    public static Task<T> ExecuteAsync<T>(DreamContext db, Func<Task<T>> work)
    {
        var strategy = db.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(
            work,
            async (context, operation, ct) =>
            {
                await using var tx = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
                var result = await operation();
                await tx.CommitAsync(ct);
                return result;
            },
            verifySucceeded: null);
    }
}
