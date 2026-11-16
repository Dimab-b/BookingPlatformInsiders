using BookingPlatformInsiders.Application.Common.Exceptions;
using BookingPlatformInsiders.Application.Common.Persistence;
using BookingPlatformInsiders.Domain.Bookings;
using BookingPlatformInsiders.Domain.Rooms;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookingPlatformInsiders.Infrastructure.Persistence;

internal sealed class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException();
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ExclusionViolation
            })
        {
            throw new BookingOverlapException();
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
