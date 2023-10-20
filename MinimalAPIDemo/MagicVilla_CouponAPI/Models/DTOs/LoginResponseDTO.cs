namespace MagicVilla_CouponAPI.Models.DTOs;

public class LoginResponseDTO
{
    public UserDTO User { get; set; }
    public string Token { get; set; }
}
