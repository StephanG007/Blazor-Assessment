using System.Text.Json;
using API.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class Seed
{
  public static async Task SeedUsers(UserManager<User> userManager)
  {
    if (await userManager.Users.AnyAsync()) return; // Do nothing if DB contains users

    var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");
    var users = JsonSerializer.Deserialize<List<User>>(userData) ?? new List<User>();

    if (!users.Any())
    {
      Console.WriteLine("No Users in seed data");
      return;
    }

    for(var i=0; i<users.Count; i++)
    {
      users[i].Id = i.ToString();
      users[i].DisplayName = users[i].Email!;
      users[i].UserName = users[i].Email;
      // This exists only because I could not figure out how to create a random int when generating the data
      users[i].ImageUrl = users[i].ImageUrl?.Replace("xyz", i.ToString());
      var result = await userManager.CreateAsync(users[i], "P@ssw0rd");
      if (!result.Succeeded)
      {
        var user = JsonSerializer.Serialize(users[i]);
        Console.WriteLine(JsonSerializer.Serialize(result.Errors));
      }
      await userManager.AddToRoleAsync(users[i], "User");
    }

    var adminUser = new User
    {
      Name = "Bruce",
      Surname = "Wayne",
      Phone = "0721112222",
      Email = "bruce@wayne.co.za",
      Address = "Unknown",
      PostalZip = "1007",
      Region = "Gotham Central",
      Country = "United States",
      Gender = 1,
      DisplayName = "bruce@wayne.co.za",
      UserName = "bruce@wayne.co.za",
      ImageUrl = "https://randomuser.me/api/portraits/lego/5.jpg"
    };

    var adminResult = await userManager.CreateAsync(adminUser, "P@ssw0rd");
    if (adminResult.Succeeded)
      Console.WriteLine("Admin Created");

    await userManager.AddToRolesAsync(adminUser, ["Admin", "User"]);
  }

  public static async Task SeedClinics(AppDbContext context)
  {
    if (await context.Clinics.AnyAsync()) return;

    var clinics = new List<Clinic>
    {
      new()
      {
        Name = "Milnerton Health Clinic",
        Address = new Address {
          StreetAddress = "123 Main Street",
          City = "Cape Town",
          Province = "Western Cape",
          PostalCode = "7441"
        },
        PhoneNumber = "072 101 2002"
      },
      new()
      {
        Name = "Lakeside Family Practice",
        Address = new Address
        {
           StreetAddress = "40 Van Wouw",
           City = "Pretoria",
           Province = "Gauteng",
           PostalCode = "1007"
        },
        PhoneNumber = "082 303 6006"
      }
    };

    var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

    foreach (var clinic in clinics)
    {
      for (var dayOffset = 0; dayOffset < 7; dayOffset++)
      {
        var date = today.AddDays(dayOffset);
        for (var hour = 9; hour < 17; hour++)
        {
          var start = DateTime.SpecifyKind(date.ToDateTime(new TimeOnly(hour, 0)), DateTimeKind.Utc);
          var end = start.AddMinutes(60);
          clinic.AppointmentSlots.Add(new AppointmentSlot
          {
            ClinicId = clinic.Id,
            StartTime = start,
            EndTime = end
          });
        }
      }
    }

    await context.Clinics.AddRangeAsync(clinics);
    await context.SaveChangesAsync();
  }
}
