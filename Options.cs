using CommandLine;

namespace PlanFix2PgSQL;

public static partial class Program
{
    
    public class Options
    {
        [Option('h', "host", Default="localhost", Group="PostgreSQL")]
        public string Host { get; set; }
        [Option('p', "port", Default=5432, Group="PostgreSQL")]
        public ushort Port { get; set; }
        [Option('d', "dbname", Default = "postgres", Group="PostgreSQL")]
        public string DataBase { get; set; }
        [Option('u', "user", Default = "postgres", Group="PostgreSQL")]
        public string UserName { get; set; }
        [Option('P', "passwd", Required=true, Group="PostgreSQL")]
        public string Password { get; set; }
        
        [Option('U', "planfix", Required = true, Group="PlanFix")]
        public string pfUserName { get; set; }
        [Option('t', "token", Required = true, Group="PlanFix")]
        public string pfToken { get; set; }
        [Value(0, Required = true)]
        public int idBegin { get; set; }
        [Value(1, Required = true)]
        public int idEnd { get; set; }
    }
}