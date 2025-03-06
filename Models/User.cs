using System.ComponentModel.DataAnnotations;

namespace Auth.Models
{
    public class User
    {
        public int Id { get; set; }
         
       [Required] public string FullName { get; set; }
        [Required] public string Email { get; set; }
        [Required] public string Phone { get; set; }
        [Required] public string PasswordHash { get; set; }
        public string Otp { get; set; }
        public DateTime OtpExpiry { get; set; }
        public bool IsVerified { get; set; }
    }

}
