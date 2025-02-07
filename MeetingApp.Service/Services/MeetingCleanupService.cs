using MeetingApp.Core;
using MeetingApp.Core.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace MeetingApp.Service.Services;

public class MeetingCleanupService(IMeetingRepository meetingRepository, IUnitOfWork unitOfWork, MeetingDbContext _context)
{
    //public async Task DeleteCancelledMeetings()
    //{

    //    var cancelledMeetings = await meetingRepository
    //        .Where(m => m.Status == false)
    //        .ToListAsync();

    //    if (cancelledMeetings.Any())
    //    {
    //        meetingRepository.RemoveRange(cancelledMeetings);
    //        await unitOfWork.SaveChangesAsync();
    //    }
    //}
    public Task DeleteCancelledMeetings()
    {
        return _context.Database.ExecuteSqlRawAsync("DELETE FROM Meetings WHERE Status = 0;");
    }

}