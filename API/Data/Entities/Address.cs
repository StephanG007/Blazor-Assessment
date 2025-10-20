using System.ComponentModel.DataAnnotations;

namespace API.Data.Entities;

public class Address
{
    public int Id { get; set; }

    [MaxLength(120)]
    public string? StreetAddress { get; set; }
    
    [MaxLength(60)]
    public string? City { get; set; }
    
    [MaxLength(30)]
    public string? Province { get; set; }
    
    [MaxLength(10)]
    public string? PostalCode { get; set; }

    [MaxLength(60)]
    public string? Country { get; set; } = "South Africa";
}