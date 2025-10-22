using API.Data.Entities;
using API.Interfaces;
using Contracts.Account;
using Contracts.Bookings;
using Contracts.Users;

namespace API.Extensions;

public static class AppUserExtensions
{
    public static UserListResponse ToListResponse(this User user)
    {
        return new UserListResponse
        {
            Id = user.Id,
            DisplayName = $"{user.Name} {user.Surname}",
            Region = user.Region,
            Country = user.Country,
            Gender = user.Gender,
            ImageUrl = user.ImageUrl
        };
    }

    public static async Task<LoginResponse> ToLoginResponse(this User user, ITokenService tokenService)
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

    public static UserProfileResponse ToProfileResponse(this User user)
    {
        return new UserProfileResponse
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

    public static BookingDetailsResponse ToDetailsResponse(this Booking booking)
    {
        return new BookingDetailsResponse(
            booking.AppointmentSlot.Clinic.Name,
            booking.AppointmentSlot.StartTime,
            booking.AppointmentSlot.EndTime,
            booking.PatientName,
            booking.PatientEmail,
            booking.Notes
        );
    }
}
