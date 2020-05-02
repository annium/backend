namespace Annium.linq2db.PostgreSql
{
    public interface IPostgreSqlConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}