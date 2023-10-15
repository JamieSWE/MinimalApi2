namespace MagicVilla_CouponAPI.Models.DTOs;

public class CouponCreateDTO
{
    public string Name { get; set; }
    public int Percent { get; set; }
    public bool IsActive { get; set; }
}
