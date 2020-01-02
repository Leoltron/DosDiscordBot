namespace DosGame.Match
{
    public class MatchResult
    {
        public MatchType Type { get; }
        public string Message { get; }

        public MatchResult(MatchType type, string message = null)
        {
            Type = type;
            Message = message;
        }

        public MatchResult AddText(string text)
        {
            var newMessage = string.IsNullOrEmpty(Message) ? text : $"{Message} {text}";
            return new MatchResult(Type, newMessage);
        }

        public static MatchResult NoMatch(string message = null) => new MatchResult(MatchType.NoMatch, message);
    }
}
