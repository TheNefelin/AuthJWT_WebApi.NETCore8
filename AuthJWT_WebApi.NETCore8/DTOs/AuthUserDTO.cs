using System.ComponentModel.DataAnnotations;

namespace AuthJWT_WebApi.NETCore8.DTOs
{
    public class AuthUserDTO
    {
        [Required]
        [EmailAddress]
        [MaxLength(50, ErrorMessage = "Max Length 50")]
        public string Email { get; set; } = string.Empty;
        [Required]
        [MinLength(6, ErrorMessage = "Min Length 6")]
        [MaxLength(50, ErrorMessage = "Max Length 50")]
        public string Password { get; set; } = string.Empty;
    }
}
