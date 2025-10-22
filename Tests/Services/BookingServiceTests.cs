using API.Data;
using API.Data.Entities;
using API.Services;
using Bogus;
using Contracts.Bookings;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Services;

public class BookingServiceTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public async Task GetAvailableSlotsAsync_returns_active_unbooked_slots_in_order()
    {
        await using var context = CreateContext();
        var clinic = new Clinic { Name = Faker.Company.CompanyName() };
        await SeedClinicAsync(context, clinic);

        var day = DateTime.UtcNow.Date;

        var availableEarly = new AppointmentSlot
        {
            ClinicId = clinic.Id,
            Clinic = clinic,
            StartTime = day.AddHours(9),
            EndTime = day.AddHours(10),
            IsActive = true
        };

        var availableLate = new AppointmentSlot
        {
            ClinicId = clinic.Id,
            Clinic = clinic,
            StartTime = day.AddHours(15),
            EndTime = day.AddHours(16),
            IsActive = true
        };

        var inactiveSlot = new AppointmentSlot
        {
            ClinicId = clinic.Id,
            Clinic = clinic,
            StartTime = day.AddHours(11),
            EndTime = day.AddHours(12),
            IsActive = false
        };

        var bookedSlot = new AppointmentSlot
        {
            ClinicId = clinic.Id,
            Clinic = clinic,
            StartTime = day.AddHours(13),
            EndTime = day.AddHours(14),
            IsActive = true
        };

        context.AppointmentSlots.AddRange(availableEarly, availableLate, inactiveSlot, bookedSlot);
        await context.SaveChangesAsync();

        var booking = new Booking
        {
            AppointmentSlotId = bookedSlot.Id,
            AppointmentSlot = bookedSlot,
            PatientName = Faker.Name.FullName(),
            PatientEmail = Faker.Internet.Email(),
            Notes = Faker.Lorem.Sentence(),
            CreatedAt = DateTime.UtcNow
        };

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        var slots = await service.GetAvailableSlotsAsync(clinic.Id, DateOnly.FromDateTime(day));

        slots.Should().BeEquivalentTo(new[]
        {
            new AvailableSlotResponse(availableEarly.Id, availableEarly.StartTime, availableEarly.EndTime),
            new AvailableSlotResponse(availableLate.Id, availableLate.StartTime, availableLate.EndTime)
        }, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task CreateBookingAsync_persists_and_returns_details_for_new_booking()
    {
        await using var context = CreateContext();
        var clinic = new Clinic { Name = Faker.Company.CompanyName() };
        await SeedClinicAsync(context, clinic);

        var day = DateTime.UtcNow.Date;

        var slot = new AppointmentSlot
        {
            ClinicId = clinic.Id,
            Clinic = clinic,
            StartTime = day.AddHours(10),
            EndTime = day.AddHours(11),
            IsActive = true
        };

        context.AppointmentSlots.Add(slot);
        await context.SaveChangesAsync();

        var service = new BookingService(context);
        var request = new BookingRequest
        {
            ClinicId = clinic.Id,
            AppointmentSlotId = slot.Id,
            PatientName = Faker.Name.FullName(),
            PatientEmail = Faker.Internet.Email(),
            Notes = Faker.Lorem.Sentence()
        };

        var result = await service.CreateBookingAsync(request);

        result.Should().NotBeNull();
        result.ClinicName.Should().Be(clinic.Name);
        result.StartTime.Should().Be(slot.StartTime);
        result.EndTime.Should().Be(slot.EndTime);
        result.PatientName.Should().Be(request.PatientName);
        result.PatientEmail.Should().Be(request.PatientEmail);
        result.Notes.Should().Be(request.Notes);

        var persistedBooking = await context.Bookings.Include(b => b.AppointmentSlot).SingleAsync();
        persistedBooking.AppointmentSlotId.Should().Be(slot.Id);
        persistedBooking.PatientEmail.Should().Be(request.PatientEmail);
    }

    [Fact]
    public async Task CreateBookingAsync_throws_when_slot_already_booked()
    {
        await using var context = CreateContext();
        var clinic = new Clinic { Name = Faker.Company.CompanyName() };
        await SeedClinicAsync(context, clinic);

        var day = DateTime.UtcNow.Date;

        var slot = new AppointmentSlot
        {
            ClinicId = clinic.Id,
            Clinic = clinic,
            StartTime = day.AddHours(12),
            EndTime = day.AddHours(13),
            IsActive = true
        };

        context.AppointmentSlots.Add(slot);
        await context.SaveChangesAsync();

        context.Bookings.Add(new Booking
        {
            AppointmentSlotId = slot.Id,
            AppointmentSlot = slot,
            PatientName = Faker.Name.FullName(),
            PatientEmail = Faker.Internet.Email(),
            Notes = Faker.Lorem.Sentence()
        });
        await context.SaveChangesAsync();

        var service = new BookingService(context);
        var request = new BookingRequest
        {
            ClinicId = clinic.Id,
            AppointmentSlotId = slot.Id,
            PatientName = Faker.Name.FullName(),
            PatientEmail = Faker.Internet.Email(),
            Notes = Faker.Lorem.Sentence()
        };

        var act = async () => await service.CreateBookingAsync(request);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Appointment slot with id {slot.Id} was not available.");
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task SeedClinicAsync(AppDbContext context, Clinic clinic)
    {
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();
    }
}
