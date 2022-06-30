using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
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
        private config.ModConfig config = new config.ModConfig();
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            config = Helper.ReadConfig<config.ModConfig>();
            if (config.Edited == false)
            {
                Monitor.Log("You need to edit the configuration!");
                Environment.Exit(1);
            }

            var Mon= this.Monitor;
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
            
            if(e.Command.CommandText.ToLower() == "ping")
            {
                client.SendMessage(e.Command.ChatMessage.Channel, "Pong!");
            }

            if (e.Command.CommandText.ToLower() == "help")
            {
                string Help =   "!money add/remove <bits>. " 
                              + "You can also use channel points to add or remove coins."
                              //+ "!time set <time> "
                              + "!time add/remove <bits where 100 bits = 1 hour). ";
                client.SendMessage(e.Command.ChatMessage.Channel, Help);
            }
            if (!Context.IsWorldReady) return;
            
            if (e.Command.ChatMessage.Bits != 0)
            {
                BitsActions action = new BitsActions
                {
                    Monitor = Monitor,
                    Config = config
                };
                Monitor.Log("Got to DoBitsCommand");
                action.DoBitsAction(e.Command);
            }
        }

        private void Client_OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            this.Monitor.Log("Disconnected from Twitch");
        }

        private void Client_OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (!Context.IsWorldReady) return;
            PointsActions action = new PointsActions
            {
                Monitor = Monitor,
                Config = config
            };
            action.DoPointsAction(e.ChatMessage);
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