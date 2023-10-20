using MagicVilla_CouponAPI.Models.DTOs;

namespace MagicVilla_CouponAPI.Repository.IRepository;

public interface IAuthRepository
{
    bool IsUniqueUser(string username);
    Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
    Task<UserDTO> Register(RegistrationRequestDTO requestDTO);
}
