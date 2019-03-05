﻿using System;
using PalApi.Example.ProjectPlugin;
using PalApi.Types;

namespace PalApi.Cli
{
    /// <summary>
    /// Example of how to use the Palringo API
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Start the bot.
        /// </summary>
        public async void Start()
        {
            //Create a new PalBot
            var bot = PalBot.Create()
                        //Tell the PalBot to log packet activity to the Console
                        .UseConsoleLogging()
                        //Call our Register Extension we created for the example
                        .RegisterProjectPlugin()
                        //Set Groupings for this bot
                        .SetGroupings("TestGroup")
                        //Add the owner's pal ID to the Authorized users list.
                        .AddAuth(1234)
                        //Print out Login Failed reason
                        .LoginFailed(reason => Console.WriteLine("Login Failed: " + reason))
                        //Print out that the bot got disconnected
                        .Disconnected(() => Console.WriteLine("Bot was disconnected"))
                        //Print out that there was a handled error within the bot
                        .Error((error, note) => Console.WriteLine("An error occurred: " + note + "\r\nStack Trace: " + error.ToString()))
                        //Print out that the bot coulnd't connect
                        .CouldNotConnect(() => Console.WriteLine("Bot could not connect to Palringo"))
                        //Load localizations - Don't include this if you aren't going to use localizations (multiple languages)
                        .LanguagesFromFlatFile("localizations.lang")
                        //Automatically relogin when the bot gets throttled
                        .ReloginOnThrottle();

            bot.On.GroupUpdate += (b, u) => Console.WriteLine($"User {u.UserId} {u.Type}ed {u.GroupId}");
            bot.On.AdminAction += (b, u) => Console.WriteLine($"User {u.SourceId} {u.Action}ed {u.TargetId}");

            //Start the login sequence. Only Email and Password are required. Rest will default
            await bot.Login(
                            //Email Address
                            "example@test.com", 
                            //Password
                            "asdf",
                            //Authorization Status (Away, Invisible, Online, ect)
                            AuthStatus.Away,
                            //What device to mask the bot as (Suggest PC / Web / Generic)
                            DeviceType.Generic,
                            //Span filter (Suggest false, cause its a bot...)
                            false);

            
        }

        /// <summary>
        /// Primary entry point of the application
        /// </summary>
        /// <param name="args">Command line arguments (not used)</param>
        static void Main(string[] args)
        {
            //Call the start method.
            new Program().Start();

            //Block the current thread.
            while (true)
                Console.ReadKey();
        }
    }
}
