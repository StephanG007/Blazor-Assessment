using API.Controllers.Account.DTOs;
using API.Controllers.Booking.DTOs;
using API.Controllers.Users.DTOs;
using API.Data.Entities;
using API.Interfaces;

namespace API.Extensions;

public static class AppUserExtensions
{
    public static UserListDto ToListDto(this User user)
    {
        return new UserListDto
        {
            Id = user.Id,
            DisplayName = $"{user.Name} {user.Surname}",
            Region = user.Region,
            Country = user.Country,
            Gender = user.Gender,
            ImageUrl = user.ImageUrl
        };
    }
    
    public static async Task<LoginResponse> ToLoginDto(this User user, ITokenService tokenService)
    {
        return new LoginResponse
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email!,
            ImageUrl = user.ImageUrl,
            Token = await tokenService.CreateToken(user)
        };
    }

    public static UserProfileDto ToProfileDto(this User user)
    {
        return new UserProfileDto
        {
            Name = user.Name,
            Surname = user.Surname,
            DisplayName = user.DisplayName,
            Phone = user.Phone,
            Address = user.Address,
            PostalZip = user.PostalZip,
            Region = user.Region,
            Country = user.Country,
            Gender = user.Gender,
            ImageUrl = user.ImageUrl,
        };
    }

    public static BookingDetailsDto ToDto(this Booking booking)
    {
        return new BookingDetailsDto(
            booking.AppointmentSlot.Clinic.Name,
            booking.AppointmentSlot.StartTime,
            booking.AppointmentSlot.EndTime,
            booking.PatientName,
            booking.PatientEmail,
            booking.Notes
        );
    }
}
