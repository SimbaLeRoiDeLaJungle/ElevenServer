namespace GameServer
{
    public class UserData
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public int DbId { get; set; }

        public int Cash { get; set; }

        public UserData(string name, string email, int dbId, int cash)
        {
            Username = name;
            Email = email;
            DbId = dbId;
            Cash = cash;
        }
    }
}