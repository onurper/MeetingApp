using MeetingApp.Core;
using MeetingApp.Core.IRepositories;

namespace MeetingApp.Service.Services;

public class MeetingCleanupService(IMeetingRepository meetingRepository, IUnitOfWork unitOfWork)
{
    public void DeleteCancelledMeetings()
    {
        var cancelledMeetings = meetingRepository
            .Where(m => m.Status == false)
            .ToList();

        meetingRepository.RemoveRange(cancelledMeetings);
        unitOfWork.SaveChangesAsync();
    }
}