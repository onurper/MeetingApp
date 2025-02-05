using MeetingApp.Core;

namespace MeetingApp.Data;

public class UnitOfWork(MeetingDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}