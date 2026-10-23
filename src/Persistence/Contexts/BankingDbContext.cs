using System.Reflection;
using System.Text.Json;
using Core.DomainEvents;
using Core.Repositories;
using Core.Security.Entities;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Contexts;

public class BankingDbContext : DbContext
{
    private static readonly JsonSerializerOptions OutboxJsonOptions = new(JsonSerializerDefaults.Web);

    public BankingDbContext(DbContextOptions<BankingDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<IndividualCustomer> IndividualCustomers { get; set; }
    public DbSet<CorporateCustomer> CorporateCustomers { get; set; }
    public DbSet<CreditType> CreditTypes { get; set; }
    public DbSet<CreditSubType> CreditSubTypes { get; set; }
    public DbSet<CreditApplication> CreditApplications { get; set; }
    public DbSet<CreditApplicationSaga> CreditApplicationSagas { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<OperationClaim> OperationClaims { get; set; }
    public DbSet<UserOperationClaim> UserOperationClaims { get; set; }

    public override int SaveChanges()
    {
        List<AggregateRoot> aggregates = GetAggregatesWithEvents();
        List<IDomainEvent> domainEvents = CollectEvents(aggregates);
        AppendOutboxMessages(domainEvents);

        int result = base.SaveChanges();
        ClearEvents(aggregates);
        return result;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<AggregateRoot> aggregates = GetAggregatesWithEvents();
        List<IDomainEvent> domainEvents = CollectEvents(aggregates);
        AppendOutboxMessages(domainEvents);

        int result = await base.SaveChangesAsync(cancellationToken);
        ClearEvents(aggregates);
        return result;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }

    private List<AggregateRoot> GetAggregatesWithEvents() =>
        ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.GetDomainEvents().Count > 0)
            .Select(e => e.Entity)
            .ToList();

    private static List<IDomainEvent> CollectEvents(IEnumerable<AggregateRoot> aggregates) =>
        aggregates.SelectMany(a => a.GetDomainEvents()).ToList();

    private static void ClearEvents(IEnumerable<AggregateRoot> aggregates)
    {
        foreach (AggregateRoot aggregate in aggregates)
            aggregate.ClearDomainEvents();
    }

    private void AppendOutboxMessages(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            OutboxMessages.Add(new OutboxMessage
            {
                Type = domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                Payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), OutboxJsonOptions),
                OccurredOnUtc = domainEvent.OccurredOnUtc
            });
        }
    }
}
