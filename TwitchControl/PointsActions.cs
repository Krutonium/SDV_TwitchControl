using System.Threading;
using StardewModdingAPI;
using StardewValley;
using TwitchLib.Client.Models;

namespace TwitchControl
{
    public class PointsActions
    {
        public IMonitor Monitor;
        public config.ModConfig Config;
        
        public void DoPointsAction(ChatMessage message)
        {
            Monitor.Log($"Message received: {message.Message}");
            Monitor.Log(message.CustomRewardId);
            //Remove 100 coins from Stardew
            if (message.CustomRewardId == Config.RewardIDRemove100)
            {
                Game1.player.Money -= 100;        
            }

            //Remove 1000 coins from Stardew
            if (message.CustomRewardId == Config.RewardIDRemove1000)
            {
                Game1.player.Money -= 1000;
            }
            
            //Add 100 Coins to Stardew
            if (message.CustomRewardId == Config.RewardIDAdd100)
            {
                Game1.player.Money += 100;
            }
            
            //Add 1000 Coins to Stardew
            if (message.CustomRewardId == Config.RewardIDAdd1000)
            {
                Game1.player.Money += 1000;
            }
        }
    }
}