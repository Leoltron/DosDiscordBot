using System;

namespace Dos.ReplayService.Models
{
    public class ReplayListItemViewModel
    {
        public string ServerName { get; }
        public string ChannelName { get; }
        public Guid ReplayId { get; }
        public DateTime Date { get; }
        public int PlayersCount { get; }

        public ReplayListItemViewModel(string serverName, string channelName, Guid replayId, DateTime date,
                                       int playersCount)
        {
            ServerName = serverName;
            ChannelName = channelName;
            ReplayId = replayId;
            Date = date;
            PlayersCount = playersCount;
        }
    }
}
