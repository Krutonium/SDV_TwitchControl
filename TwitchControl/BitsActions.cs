using System;
using System.Linq;
using System.Threading;
using StardewModdingAPI;
using StardewValley;
using TwitchLib.Client.Models;


namespace TwitchControl
{
    public class BitsActions
    {
        public IMonitor Monitor;
        public config.ModConfig Config;
        public void DoBitsAction(ChatCommand command)
        {
            if (command.CommandText.ToLower() == "money")
            {
                ManipulateMoney(command);
            }

            if (command.CommandText.ToLower() == "time")
            {
                ManipulateTime(command);
            }
        }

        private void ManipulateTime(ChatCommand command)
        {
            if (command.ArgumentsAsString.ToLower().Contains("set") && false)
            {
                try
                {
                    Game1.timeOfDay = Int32.Parse(command.ArgumentsAsList.Last());
                } catch (Exception e)
                {
                    Monitor.Log("Error: " + e.Message, LogLevel.Error);
                }

                return;
            }

            int time = (int)Math.Round(command.ChatMessage.Bits * (60.0 / Config.BitsPerHour), 0);
            Monitor.Log(time.ToString());
            
            if (command.ArgumentsAsString.ToLower().Contains("add"))
            {
                try
                {
                    Game1.timeOfDay = Utility.ModifyTime(Game1.timeOfDay, time);
                }
                catch (Exception e)
                {
                    Monitor.Log("Error: " + e.Message, LogLevel.Error);
                }
            }

            if (command.ArgumentsAsString.ToLower().Contains("remove"))
            {
                try
                {
                    Game1.timeOfDay = Utility.ModifyTime(Game1.timeOfDay, -time);
                }
                catch (Exception e)
                {
                    Monitor.Log("Error: " + e.Message, LogLevel.Error);
                }
            }
        }

        private void ManipulateMoney(ChatCommand command)
        {
            int coins = command.ChatMessage.Bits * 100;
            Monitor.Log("Adding " + coins + " coins");
            
            if (command.ArgumentsAsString.ToLower().Contains("add"))
            {
                Monitor.Log("Adding " + coins + " coins");
                Game1.player.Money += coins;
            }
            else if (command.ArgumentsAsString.ToLower().Contains("remove"))
            {
                Monitor.Log("Removing " + coins + " coins");
                Game1.player.Money -= coins;
            }
            else
            {
                Monitor.Log("No add or remove specified, assuming add");
                Game1.player.Money += coins;
            }
        }
    }
}