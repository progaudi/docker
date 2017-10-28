namespace Progaudi.Tarantool.Bot.Models
{
    public class PushPayload
    {
        public string Ref { get; set; }
        public string Before { get; set; }
        public string After { get; set; }
        public bool Created { get; set; }
        public bool Deleted { get; set; }
        public bool Forced { get; set; }
        public object BaseRef { get; set; }
        public string Compare { get; set; }
        public Commit[] Commits { get; set; }
        public Commit HeadCommit { get; set; }
        public Repository Repository { get; set; }
        public Pusher Pusher { get; set; }
        public Sender Sender { get; set; }
    }
}