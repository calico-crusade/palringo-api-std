namespace PalApi.Plugins.Linguistics
{
    public class LangMessage : Message
    {
        public override string Command => "LANG_MESSAGE";

        public string LanguageKey { get; set; }

        public LangMessage() { }

        public LangMessage(Message msg, string lang)
        {
            LanguageKey = lang;
            UserId = msg.UserId;
            GroupId = msg.GroupId;
            Content = msg.Content;
            MimeType = msg.MimeType;
            UnsortedTimestamp = msg.UnsortedTimestamp;
        }
    }
}
