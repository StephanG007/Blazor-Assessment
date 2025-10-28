using System.Linq.Expressions;
using System.Threading.Tasks;
using API.Data.Entities;
using API.Interfaces;
using Contracts.Account;
using Contracts.Bookings;
using Contracts.Clinics;
using Contracts.Users;

namespace API.Extensions;

public static class DtoConversions
{
    public static async Task<LoginResponse> ToLoginResponse(this User user, ITokenService tokenService) => new()
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        Email = user.Email!,
        ImageUrl = user.ImageUrl,
        Token = await tokenService.CreateToken(user)
    };

    public static UserProfileResponse ToProfileResponse(this User user) => new()
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

    public static BookingDetailsResponse ToDetailsResponse(this Booking booking) => new(
        booking.AppointmentSlot.Clinic!.Name,
        booking.AppointmentSlot.StartTime,
        booking.AppointmentSlot.EndTime,
        booking.PatientName,
        booking.PatientEmail,
        booking.Notes);

    public static ClinicDto ToDto(this Clinic clinic) => new(
        clinic.Id,
        clinic.Name,
        clinic.Address?.City,
        clinic.Address?.Province,
        clinic.PhoneNumber,
        clinic.LogoBase64);

    public static readonly Expression<Func<AppointmentSlot, AvailableSlotResponse>> ToSlotResponse = slot =>
        new AvailableSlotResponse(
            slot.Id,
            slot.StartTime,
            slot.EndTime,
            slot.Booking != null,
            slot.Booking != null ? slot.Booking.Id : (int?)null);
}

