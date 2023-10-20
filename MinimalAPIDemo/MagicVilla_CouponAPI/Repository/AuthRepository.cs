using AutoMapper;
using MagicVilla_CouponAPI.Data;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Models.DTOs;
using MagicVilla_CouponAPI.Repository.IRepository;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MagicVilla_CouponAPI.Repository;

public class AuthRepository : IAuthRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private string secretKey;

    public AuthRepository(ApplicationDbContext db, IMapper mapper, IConfiguration configuration)
    {
        _db = db;
        _mapper = mapper;
        _configuration = configuration;
        secretKey = _configuration.GetValue<string>("ApiSettings:Secret");
    }
    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
    {
        var user = _db.LocalUsers.SingleOrDefault(x => x.UserName == loginRequestDTO.UserName
                                                        && x.Password == loginRequestDTO.Password);

        if (user == null) return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.Role,user.Role),
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        LoginResponseDTO response = new()
        {
            User = _mapper.Map<UserDTO>(user),
            Token = new JwtSecurityTokenHandler().WriteToken(token)
        };

        return response;
    }

    public bool IsUniqueUser(string username)
    {
        var user = _db.LocalUsers.FirstOrDefault(x => x.UserName == username);

        if (user == null) return true;

        return false;
    }

    public async Task<UserDTO> Register(RegistrationRequestDTO requestDTO)
    {
        LocalUser userObj = new()
        {
            UserName = requestDTO.UserName,
            Password = requestDTO.Password,
            Name = requestDTO.Name,
            Role = "admin"
        };

        _db.LocalUsers.Add(userObj);
        await _db.SaveChangesAsync();
        userObj.Password = "";

        return _mapper.Map<UserDTO>(userObj);
    }
}
