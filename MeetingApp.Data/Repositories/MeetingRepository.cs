using MeetingApp.Core.IRepositories;
using MeetingApp.Core.Models;

namespace MeetingApp.Data.Repositories;

public class MeetingRepository(MeetingDbContext context) : GenericRepository<Meeting>(context), IMeetingRepository
{
}