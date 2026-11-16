using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PillSync.Api.Controllers;
using PillSync.Api.Data;
using PillSync.Api.Models;
using PillSync.Api.Models.Dtos;
using Xunit;

namespace PillSync.Tests;

public class MedicationsControllerTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private MedicationsController GetController(AppDbContext context, string userId)
    {
        var controller = new MedicationsController(context);
        
        // Подделываем JWT-токен для тестов
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyCurrentUserMedications()
    {
        var context = GetInMemoryDbContext();
        var user1Id = "user-1";
        var user2Id = "user-2";

        context.Medications.Add(new Medication { Name = "User 1 Med", UserId = user1Id });
        context.Medications.Add(new Medication { Name = "User 2 Med", UserId = user2Id });
        await context.SaveChangesAsync();

        var controller = GetController(context, user1Id);

        var result = await controller.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var medications = Assert.IsAssignableFrom<IEnumerable<Medication>>(okResult.Value);
        
        Assert.Single(medications);
        Assert.Equal("User 1 Med", medications.First().Name);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenMedicationBelongsToAnotherUser()
    {
        var context = GetInMemoryDbContext();
        var user1Id = "user-1";
        var user2Id = "user-2";

        var med = new Medication { Name = "User 2 Med", UserId = user2Id };
        context.Medications.Add(med);
        await context.SaveChangesAsync();

        var controller = GetController(context, user1Id);
        var updateDto = new MedicationUpdateDto { Name = "Hacked!", StartDate = DateTime.UtcNow };

        var result = await controller.Update(med.Id, updateDto);

        Assert.IsType<NotFoundObjectResult>(result);
        
        var dbMed = await context.Medications.FindAsync(med.Id);
        Assert.Equal("User 2 Med", dbMed!.Name);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMedicationBelongsToAnotherUser()
    {
        var context = GetInMemoryDbContext();
        var user1Id = "user-1";
        var user2Id = "user-2";

        var med = new Medication { Name = "User 2 Med", UserId = user2Id };
        context.Medications.Add(med);
        await context.SaveChangesAsync();

        var controller = GetController(context, user1Id);

        var result = await controller.Delete(med.Id);

        Assert.IsType<NotFoundObjectResult>(result);
        
        var dbMed = await context.Medications.FindAsync(med.Id);
        Assert.NotNull(dbMed);
    }
}