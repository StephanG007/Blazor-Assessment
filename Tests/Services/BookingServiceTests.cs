using System.Collections.Generic;
using System.Text.Json;
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
            f.Customize<Clinic>(c => c
                .Without(x => x.Id)
                .Without(x => x.AppointmentSlots));

            f.Customize<AppointmentSlot>(c =>
                c.Without(x => x.Id)
                 .Without(x => x.Clinic)
                 .Without(x => x.Booking)
                 .With(x => x.IsActive, true)); // default to active
            
            f.Register(() =>
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;
                return new AppDbContext(options);
            });

            f.Freeze<AppDbContext>();

            return f;
        }) { }
    }

    [Theory, AutoDbData]
    public async Task GetAvailableSlotsAsync_returns_all_and_marks_booked_correctly(
        AppDbContext context, IFixture fixture, Clinic clinic)
    {
        // Arrange
        context.Clinics.Add(clinic);
        await context.SaveChangesAsync();

        // Use a fixed day and explicit window [day, day+1)
        var day = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = day;
        var to   = day.AddDays(1);

        // Prevent accidental graph creation
        fixture.Customize<AppointmentSlot>(c => c
            .Without(s => s.Clinic)
            .Without(s => s.Booking)   // never auto-create
            .With(s => s.IsActive, true));

        fixture.Customize<Booking>(c => c
            .Without(b => b.AppointmentSlot)); // never auto-create

        var t0 = day.ToDateTime(TimeOnly.MinValue);
        var s1_available = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .With(s => s.Clinic, clinic)
            .With(s => s.IsActive, true)
            .Without(s => s.Booking)
            .With(s => s.StartTime, t0.AddHours(9)).With(s => s.EndTime, t0.AddHours(10))
            .Create();

        var s2_inactive = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .With(s => s.Clinic, clinic)
            .Without(s => s.Booking)
            .With(s => s.StartTime, t0.AddHours(11)).With(s => s.EndTime, t0.AddHours(12))
            .With(s => s.IsActive, false)
            .Create();

        var s3_booked = fixture.Build<AppointmentSlot>()
            .Without(s => s.Booking)
            .With(s => s.IsActive, true)
            .With(s => s.Clinic, clinic)
            .With(s => s.ClinicId, clinic.Id).With(s => s.StartTime, t0.AddHours(13)).With(s => s.EndTime, t0.AddHours(14))
            .Create();

        var s4_available = fixture.Build<AppointmentSlot>()
            .With(s => s.ClinicId, clinic.Id)
            .With(s => s.Clinic, clinic)
            .With(s => s.IsActive, true)
            .Without(s => s.Booking)
            .With(s => s.StartTime, t0.AddHours(15)).With(s => s.EndTime, t0.AddHours(16))
            .Create();

        var defaultSlots = context.AppointmentSlots.ToList();
        
        context.AppointmentSlots.AddRange(s1_available, s2_inactive, s3_booked, s4_available);
        await context.SaveChangesAsync();

        var clinics = context.Clinics.ToList();
        var slotsJSON = await context.AppointmentSlots.ToListAsync();
        
        var booking = fixture.Build<Booking>()
            .With(b => b.AppointmentSlotId, s3_booked.Id)
            .With(b => b.AppointmentSlot, s3_booked)
            .Create();

        context.Bookings.Add(booking);
        await context.SaveChangesAsync();

        var service = new BookingService(context);

        // Act
        var slots = await service.GetAvailableSlotsAsync(clinic.Id, from, to);

        // Assert (ordered)
        slots.Should().BeEquivalentTo([
            new AvailableSlotResponse(s1_available.Id, s1_available.StartTime, s1_available.EndTime, false, null),
            new AvailableSlotResponse(s3_booked.Id, s3_booked.StartTime, s3_booked.EndTime, true,  booking.Id),
            new AvailableSlotResponse(s4_available.Id, s4_available.StartTime, s4_available.EndTime, false, null)
        ], opts => opts.WithStrictOrdering());
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
            .Without(s => s.Booking)
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
