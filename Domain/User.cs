namespace Domain
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public ICollection<Group> Groups { get; set; }

        public User() 
        {
            Groups = new List<Group>();
        }
    }

    public class CreateUserDTO
    {
        public string UserName { get; set; }
        public List<int> GroupIds { get; set; } 
    }

    public class UpdateUserDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; }

        public List<GroupDto> Groups { get; set; }
    }
}
