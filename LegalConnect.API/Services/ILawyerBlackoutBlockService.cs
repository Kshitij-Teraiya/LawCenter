using LegalConnect.API.DTOs.TimeSlots;

namespace LegalConnect.API.Services;

public interface ILawyerBlackoutBlockService
{
    Task<List<BlackoutBlockDto>> GetBlackoutBlocksAsync(int lawyerProfileId);
    Task<(bool Success, string Message, int? Id)> CreateBlackoutBlockAsync(int lawyerProfileId, CreateBlackoutBlockDto dto);
    Task<(bool Success, string Message)> DeleteBlackoutBlockAsync(int lawyerProfileId, int blockId);
    Task<bool> IsInBlackoutBlockAsync(int lawyerProfileId, DateTime dateTime, TimeSpan duration);
}
