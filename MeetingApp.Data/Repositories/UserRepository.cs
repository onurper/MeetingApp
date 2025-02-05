using MeetingApp.Core.IRepositories;
using MeetingApp.Core.Models;

namespace MeetingApp.Data.Repositories
{
    public class UserRepository(MeetingDbContext context) : GenericRepository<User>(context), IUserRepository
    {
    }
}