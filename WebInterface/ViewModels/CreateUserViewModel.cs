using Domain;
using System.ComponentModel.DataAnnotations;

namespace WebInterface.ViewModels
{
    public class CreateUserViewModel
    {
        public int? UserId {  get; set; }

        [Required(ErrorMessage = "User name is required.")]
        [StringLength(100, ErrorMessage = "User name cannot exceed 100 characters.")]
        public string UserName { get; set; }

        public List<Group> AvailableGroups { get; set; } = new List<Group>();
        public List<int> SelectedGroupIds { get; set; } = new List<int>();

    }
}
