using System;

namespace Progaudi.Tarantool.Bot.Models
{
    public class Commit
    {
        public string Id { get; set; }
        public string TreeId { get; set; }
        public bool Distinct { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
        public User Author { get; set; }
        public User Committer { get; set; }
        public object[] Added { get; set; }
        public object[] Removed { get; set; }
        public string[] Modified { get; set; }
    }
}