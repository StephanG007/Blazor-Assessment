using System.Collections.Generic;
using API.Data;
using API.Data.Entities;
using API.Services;
using Contracts.Bookings;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AutoFixture;
using AutoFixture.Xunit2;

namespace Tests.Services;

public class BookingServiceTests
{
    // ---------- Attribute to provide: a configured Fixture + fresh InMemory AppDbContext ----------
    private sealed class AutoDbDataAttribute : AutoDataAttribute
    {
        public AutoDbDataAttribute() : base(() =>
        {
            var f = new Fixture();

            // Prevent crazy deep graphs
            f.Behaviors.Clear();
            f.Behaviors.Add(new OmitOnRecursionBehavior());

            // Keep IDs and navs clean; we'll set only what we need in tests
            f.Customize<Clinic>(c =>
                c.Without(x => x.Id));

            f.Customize<AppointmentSlot>(c =>
                c.Without(x => x.Id)
                 .Without(x => x.Clinic)
                 .Without(x => x.Booking)
                 .With(x => x.IsActive, true)); // default to active

            f.Customize<Booking>(c =>
                c.Without(x => x.Id)
                 .Without(x => x.Clinic)
                 .Without(x => x.AppointmentSlot)
                 .With(x => x.CreatedAt, DateTime.UtcNow));

            // Fresh DbContext per test
            f.Register(() =>
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;
                return new AppDbContext(options);
            });

            return f;
        }) { }
    }

    [Theory, AutoDbData]
    public async Task GetAvailableSlotsAsync_returns_active_slots_with_booking_status(
        AppDbContext context, IFixture fixture, Clinic clinic)
    {
        // Arrange
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        var day = DateTime.UtcNow.Date;

        var availableEarly = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .With(s => s.StartTime, day.AddHours(9))
            .With(s => s.EndTime,   day.AddHours(10))
            .Without(s => s.Booking)
            .Without(s => s.Clinic)
            .Create();

        var availableLate = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .With(s => s.StartTime, day.AddHours(15))
            .With(s => s.EndTime,   day.AddHours(16))
            .Without(s => s.Booking)
            .Without(s => s.Clinic)
            .Create();

        var inactiveSlot = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .With(s => s.StartTime, day.AddHours(11))
            .With(s => s.EndTime,   day.AddHours(12))
            .With(s => s.IsActive,  false)
            .Without(s => s.Booking)
            .Without(s => s.Clinic)
            .Create();

        var bookedSlot = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .With(s => s.StartTime, day.AddHours(13))
            .With(s => s.EndTime,   day.AddHours(14))
            .Without(s => s.Clinic)
            .Create();

        context.AppointmentSlots.AddRange(availableEarly, availableLate, inactiveSlot, bookedSlot);
        await context.SaveChangesAsync();
        
        var booking = fixture.Build<Booking>()
            .With(b => b.AppointmentSlotId, bookedSlot.Id)
            .Create();

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act
        var slots = await service.GetAvailableSlotsAsync(clinic.Id, null, null);

        // Assert
        slots.Should().BeEquivalentTo(new[]
        {
            new AvailableSlotResponse(availableEarly.Id, availableEarly.StartTime, availableEarly.EndTime, false, null),
            new AvailableSlotResponse(bookedSlot.Id,     bookedSlot.StartTime,     bookedSlot.EndTime,     true,  booking.Id),
            new AvailableSlotResponse(availableLate.Id,  availableLate.StartTime,  availableLate.EndTime,  false, null),
        }, opts => opts.WithStrictOrdering());
    }

    [Theory, AutoDbData]
    public async Task CreateBookingAsync_persists_and_returns_details_for_new_booking(
        AppDbContext context, IFixture fixture, Clinic clinic)
    {
        // Arrange
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        var slots = context.AppointmentSlots.ToList();

        var day = DateTime.UtcNow.Date;

        var slot = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .With(s => s.StartTime, day.AddHours(10))
            .With(s => s.EndTime,   day.AddHours(11))
            .Without(s => s.Booking)
            .Without(s => s.Clinic)
            .Create();

        context.AppointmentSlots.Add(slot);
        await context.SaveChangesAsync();

        var request = fixture.Build<BookingRequest>()
            .With(r => r.ClinicId, clinic.Id)
            .With(r => r.AppointmentSlotId, slot.Id)
            .Create();

        var service = new BookingService(context);

        // act
        var result = await service.CreateBookingAsync(request);

        // assert (response)
        result.ClinicName.Should().Be(clinic.Name);
        result.StartTime.Should().Be(slot.StartTime);
        result.EndTime.Should().Be(slot.EndTime);
        result.PatientName.Should().Be(request.PatientName);
        result.PatientEmail.Should().Be(request.PatientEmail);
        result.Notes.Should().Be(request.Notes);

        // assert (persistence)
        var persisted = await context.Bookings.Include(b => b.AppointmentSlot).SingleAsync();
        persisted.AppointmentSlotId.Should().Be(slot.Id);
        persisted.PatientEmail.Should().Be(request.PatientEmail);
    }

    [Theory, AutoDbData]
    public async Task CreateBookingAsync_throws_when_slot_already_booked(
        AppDbContext context, IFixture fixture, Clinic clinic)
    {
        // arrange
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        var day = DateTime.UtcNow.Date;

        var slot = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .With(s => s.StartTime, day.AddHours(12))
            .With(s => s.EndTime,   day.AddHours(13))
            .Create();

        context.AppointmentSlots.Add(slot);
        await context.SaveChangesAsync();

        // existing booking for that slot
        var existing = fixture.Build<Booking>()
            .With(b => b.AppointmentSlotId, slot.Id)
            .Create();

        context.Bookings.Add(existing);
        await context.SaveChangesAsync();

        var request = fixture.Build<BookingRequest>()
            .With(r => r.ClinicId, clinic.Id)
            .With(r => r.AppointmentSlotId, slot.Id)
            .Create();

        var service = new BookingService(context);

        // act
        var act = async () => await service.CreateBookingAsync(request);

        // assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage($"Appointment slot with id {slot.Id} was not available.");
    }

    [Theory, AutoDbData]
    public async Task DeleteBookingAsync_removes_existing_booking(
        AppDbContext context, IFixture fixture, Clinic clinic)
    {
        // arrange
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        var slot = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .Create();

        context.AppointmentSlots.Add(slot);
        await context.SaveChangesAsync();

        var booking = fixture.Build<Booking>()
            .With(b => b.AppointmentSlotId, slot.Id)
            .Create();

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // act
        await service.DeleteBookingAsync(booking.Id);

        // assert
        (await context.Bookings.CountAsync()).Should().Be(0);
    }

    [Theory, AutoDbData]
    public async Task DeleteBookingAsync_throws_when_booking_not_found(AppDbContext context)
    {
        // arrange
        var service = new BookingService(context);

        // act
        var act = async () => await service.DeleteBookingAsync(12345);

        // assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Booking with id 12345 was not found.");
    }
}
