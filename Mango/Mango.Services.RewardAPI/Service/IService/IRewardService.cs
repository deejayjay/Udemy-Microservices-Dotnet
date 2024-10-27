using Mango.Services.RewardAPI.Message;

namespace Mango.Services.RewardAPI.Service.IService;

public interface IRewardService
{
    Task<bool> UpdateRewardsAsync(RewardMessage rewardMessage);
}
