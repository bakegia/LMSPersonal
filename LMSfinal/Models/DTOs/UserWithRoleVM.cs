namespace LMSfinal.Models.DTOs
{
    public class UserWithRoleVM
    {
        public ApplicationUser User { get; set; }
        public IList<string> Roles { get; set; }
    }
}
