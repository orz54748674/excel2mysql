namespace Excel2Mysql.entity
{
    class JsConfig
    {
        public DbConfig[] DbConfigs { get; set; }
        public User User { get; set; }
    }

    class DbConfig
    {
        public string host { get; set; }

        public string port { get; set; }

        public string user { get; set; }

        public string password { get; set; }

        public string desc { get; set; }

        public string charset {get;set;}

        public string hookUrl { get; set; }
    }

    class User
    {
        public string name { get; set; }
    }

    public delegate void SetMaxPro(int maxVal);
    public delegate void UpdataPro(string fileName);
}
