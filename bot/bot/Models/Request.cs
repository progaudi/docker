namespace Progaudi.Tarantool.Bot.Models
{
    public class Request
    {
        public string Branch { get; } = "develop";

        public Config Config { get; } = new Config();
    }
}