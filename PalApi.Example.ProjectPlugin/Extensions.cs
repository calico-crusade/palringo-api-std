namespace PalApi.Example.ProjectPlugin
{
    public static class Extensions
    {
        /// <summary>
        /// CSharp is a little too smart of its own good. It won't include a library that isn't used at all.
        /// So we use this extension to make sure the project and plugins are included
        /// This can also be used to supply settings and such to the plugin.
        /// </summary>
        /// <param name="bot"></param>
        /// <returns></returns>
        public static IPalBot RegisterProjectPlugin(this IPalBot bot)
        {
            return bot;
        }
    }
}
