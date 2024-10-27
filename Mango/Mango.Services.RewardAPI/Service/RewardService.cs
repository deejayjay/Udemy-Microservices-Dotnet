using Microsoft.EntityFrameworkCore;
using Mango.Services.RewardAPI.Service.IService;
using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;

namespace Mango.Services.RewardAPI.Service;

public class RewardService : IRewardService
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;

    public RewardService(DbContextOptions<AppDbContext> dbOptions)
    {
        _dbOptions = dbOptions;
    }

    public async Task<bool> UpdateRewardsAsync(RewardMessage rewardMessage)
    {
        try
        {
            Reward reward = new()
            { 
                OrderId = rewardMessage.OrderId,
                RewardActivity = rewardMessage.RewardActivity,
                UserId = rewardMessage.UserId,
                RewardDate = DateTime.Now,
            };

            await using var _db = new AppDbContext(_dbOptions);

            await _db.Rewards.AddAsync(reward);
            await _db.SaveChangesAsync();

            return true;
        }
        catch (Exception _)
        {
            return false;
        }
    }
}
