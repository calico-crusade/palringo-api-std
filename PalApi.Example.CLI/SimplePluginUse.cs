using PalApi.Plugins;
using PalApi.Types;

namespace PalApi.Example.CLI
{
    /// <summary>
    /// Shows basic implementation of the plugins.
    /// </summary>
    [Command("!ex")] //The prefix to the command (Optional)
    public class SimplePluginUse : IPlugin //Requires IPlugin in order to be picked up (for Runtime performance)
    {
        /// <summary>
        /// Shows basic implementation of the Plugins system
        /// </summary>
        /// <param name="bot">The bot that is making the request</param>
        /// <param name="msg">The message that triggered the request</param>
        /// <param name="cmd">The message without the command (!ex test)</param>
        [Command("test")]
        public async void TestPlugins(IPalBot bot, Message msg, string cmd)
        {
            //Make the bot reply to the calling user with "Hello world!"
            await bot.Reply(msg, "Hello world!");

            //Make the bot send a PM to the calling user
            await bot.Private(msg.UserId, "Hey there! Thanks for testing the bot!");

            //If the message was sent in a group, send a message back to the group
            if (msg.MesgType == MessageType.Group)
            {
                await bot.Group(msg.GroupId.Value, "Is this group any fun to chat in?");
            }
        }

        /// <summary>
        /// Shows how to request things from the user
        /// </summary>
        /// <param name="bot">The bot that is making the request</param>
        /// <param name="msg">The message that triggered the request</param>
        /// <param name="cmd">The message without the command (!ex questions)</param>
        [Command("questions")]
        public async void TestQuestionPlugins(IPalBot bot, Message msg, string cmd)
        {
            //Ask the user what their favourite colour is
            await bot.Reply(msg, "Hello! Whats your favourite colour?");

            //Get the reponse from the user
            var colourResponse = await bot.NextMessage(msg);

            //Get the users age
            await bot.Reply(msg, $"So your favourite colour is {colourResponse.Content}. How old are you?");

            //Get the response from the user
            var ageResponse = await bot.NextMessage(msg);

            //Make sure the age is valid.
            if (!int.TryParse(ageResponse.Content, out int age) || age <= 0 || age > 125)
            {
                //Let the user know their response was not valid.
                await bot.Reply(msg, $"I'm sorry but \"{ageResponse.Content}\" is not a valid age! Please try again later...");
                return;
            }

            if (age >= 21)
            {
                await bot.Reply(msg, $"You can drink in the states!");
                return;
            }

            if (age >= 18)
            {
                await bot.Reply(msg, $"You're legal! Awesome!");
                return;
            }

            if (age < 17)
            {
                await bot.Reply(msg, $"You shouldn't be on here! Palringo has an age restriction of 17+");
                return;
            }

            await bot.Reply(msg, $"So you're 17... huh... boring age... Haha");
        }
    }
}
