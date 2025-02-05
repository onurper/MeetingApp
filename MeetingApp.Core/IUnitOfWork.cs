namespace MeetingApp.Core
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}