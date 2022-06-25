using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Linq;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;

namespace TwitchControl
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        
        /// <summary>
        /// TODO:
        /// Utility.ModifyTime();
        /// Channel Points: Modify Time (+1 hour)
        /// Channel Points: Modify Time (-1 hour)
        /// Bits: Set Time (Exact Time in Minutes)
        /// Bits: End Day (Midnight, not 2AM, so the player has a chance to get home
        /// Bits: More: 2AM
        ///
        /// Channel Points: Swap Hotbar Item
        /// Bits: Remove Item (Except Tools)
        /// Bits: Add Item (Except Tools)
        /// Bits: Upgrade Current Tool to Next Tier
        /// Bits: Downgrade Current Tool to Previous Tier
        ///
        /// Channel Points: Add Health
        /// Channel Points: Add Stamina
        /// Channel Points: Remove Health
        /// Channel Points: Remove Stamina
        ///
        /// Bits & Channel Points: Spawn Enemies
        /// (Bits are 1 Enemy, Channel Points can do more)
        ///
        /// Bits/Channel Points: Status Effects
        /// Bits: Change Days Luck
        /// Bits: Change Weather
        ///
        /// Bits: Teleport Player to Location
        /// Bits: Teleport Player to Random Location
        ///
        /// Channel Points: Change Relationship Levels (+ or -)
        /// 
        /// </summary>
        
        
        
        private TwitchClient client = new TwitchClient();
        private ModConfig config = new ModConfig();
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            config = Helper.ReadConfig<ModConfig>();
            if (config.Edited == false)
            {
                Monitor.Log("You need to edit the configuration!");
                Environment.Exit(1);
            }
            Bot();
            helper.Events.GameLoop.SaveLoaded += GameLoopOnSaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += GameLoopOnReturnedToTitle;
        }

        private void GameLoopOnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            client.SendMessage(config.Channel, "Main Menu, not accepting commands. (yet!)");
        }

        private void GameLoopOnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            client.SendMessage(config.Channel, "Ready to accept commands!");
        }

        class ModConfig
        {
            public string Username = "PFCKrutonium";
            public string oAuth = "oauth:xyxyxyxyxyxyxyxyxyxyxyxyxy";
            public string Channel = "PFCKrutonium";
            public bool Edited = false;
            public int CoinsPerBit = 100;
            public string RewardIDRemove100 = "7d344760-1876-4b5b-a4ee-4e991ac76ecc";
            public string RewardIDRemove1000 = "baac6fc0-c5b4-4c91-9357-5438f6db8a25";
            public string RewardIDAdd100 = "96bce38f-927b-4ca0-8a05-901e76072c0f";
            public string RewardIDAdd1000 = "8691d6cc-4e12-4a83-83dc-1f9963d40be2";
            
        }
        
        public void Bot()
        {
            ConnectionCredentials credentials =
                new ConnectionCredentials(config.Username, config.oAuth);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 30,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, config.Channel);
            client.OnConnected += Client_OnConnected;
            client.OnJoinedChannel += Client_OnJoinedChannel;
            client.OnMessageReceived += Client_OnMessageReceived;
            client.OnDisconnected += Client_OnDisconnected;
            client.AddChatCommandIdentifier('!');
            client.OnChatCommandReceived += Client_OnChatCommandReceived;
            client.Connect();
            
        }

        private void Client_OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            if(e.Command.CommandText.ToLower() == "test")
            {
                client.SendMessage(e.Command.ChatMessage.Channel, "Test");
            }
            
            if (e.Command.CommandText.ToLower().StartsWith("money"))
            {
                if (!Context.IsWorldReady) return;
                Monitor.Log("Money command received");
                if (e.Command.ChatMessage.Bits > 0)
                {
                    //!money add bitsAmount
                    //Ratio is 1 bit -> 100 coins
                    int coins = e.Command.ChatMessage.Bits * 100;
                    if (e.Command.ArgumentsAsString.ToLower().Contains("add"))
                    {
                        Monitor.Log("Adding " + coins + " coins");
                        Game1.player.Money += coins;
                    }
                    else if (e.Command.ArgumentsAsString.ToLower().Contains("remove"))
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

        private void Client_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            this.Monitor.Log("Disconnected from Twitch");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            this.Monitor.Log($"Message received: {e.ChatMessage.Message}");
            Monitor.Log(e.ChatMessage.CustomRewardId);
            //Remove 100 coins from Stardew
            if (e.ChatMessage.CustomRewardId == config.RewardIDRemove100)
            {
                Game1.player.Money -= 100;        
            }

            //Remove 1000 coins from Stardew
            if (e.ChatMessage.CustomRewardId == config.RewardIDAdd1000)
            {
                Game1.player.Money -= 1000;
            }
            
            //Add 100 Coins to Stardew
            if (e.ChatMessage.CustomRewardId == config.RewardIDAdd100)
            {
                Game1.player.Money += 100;
            }
            
            //Add 1000 Coins to Stardew
            if (e.ChatMessage.CustomRewardId == config.RewardIDAdd1000)
            {
                Game1.player.Money += 1000;
            }
        }

        private void Client_OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            this.Monitor.Log($"Connected to Channel: {e.Channel}");
            client.SendMessage(e.Channel, "Hello, I am the Twitch Control bot!");
        }

        private void Client_OnConnected(object sender, OnConnectedArgs e)
        {
            Monitor.Log("Connected to Twitch");
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
        }
    }
}