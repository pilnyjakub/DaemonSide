namespace DaemonSide
{
    class PcBackup
    {
        public int Id { get; set; }
        public int IdPc { get; set; }
        public int IdConfig { get; set; }
    }
    class BackupFiles
    {
        public int Id { get; set; }
        public int IdConfig { get; set; }
        public string Path { get; set; }
    }
    class Time
    {
        public int Id { get; set; }
        public int IdConfig { get; set; }
        public string Frequency { get; set; }
        public string Days { get; set; }
        public string Date { get; set; }
    }
    public class Storage
    {
        public int Id { get; set; }
        public int IdConfig { get; set; }
        public string Place { get; set; }
        public string ServerIP { get; set; }
        public string ServerLogin { get; set; }
        public string ServerPass { get; set; }
        public string Path { get; set; }
    }
    public class BackupConfig
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Email { get; set; }
        public bool EmailBool { get; set; }
    }
    public class Backup
    {
        public int Id { get; set; }
        public string BackupTime { get; set; }
        public int FileCount { get; set; }
        public int FileCountSuccess { get; set; }
        public int FileCountFailed { get; set; }
        public string Errors { get; set; }
        public int IdPcBackUp { get; set; }
        public string Operations { get; set; }
    }
    public class Pc
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string OS { get; set; }
        public string State { get; set; }
        private Pc() { }
        public static Pc Instance = new Pc();
    }
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        private User() { }
        public static User Instance = new User();
    }
}
