using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CoworkSpaces.Application.DTOs.Auth;
using CoworkSpaces.Domain.Enums;
using CoworkSpaces.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoworkSpaces.Tests.Integration;

[Collection(IntegrationTestCollection.Name)]
public class ReservationConcurrencyTests
{
    private static readonly Guid SpaceId = new("11111111-1111-1111-1111-111111111111");
    private readonly SqlServerWebApplicationFactory _factory;

    public ReservationConcurrencyTests(SqlServerWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ShouldAllowOnlyOneReservationWhenTwoConcurrentRequestsTargetSameSpaceAndTime()
    {
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        var auth = await RegisterAndAuthenticateAsync(client1);

        client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var startAt = DateTime.UtcNow.AddDays(2);
        startAt = new DateTime(startAt.Year, startAt.Month, startAt.Day, 10, 0, 0, DateTimeKind.Utc);
        var endAt = startAt.AddHours(2);

        var payload = new
        {
            spaceId = SpaceId,
            startAt,
            endAt
        };

        var task1 = client1.PostAsJsonAsync("/api/reservations", payload);
        var task2 = client2.PostAsJsonAsync("/api/reservations", payload);

        var responses = await Task.WhenAll(task1, task2);
        var orderedStatusCodes = responses.Select(response => response.StatusCode).OrderBy(statusCode => (int)statusCode).ToArray();

        Assert.Equal(new[] { HttpStatusCode.Created, HttpStatusCode.Conflict }, orderedStatusCodes);

        var conflictResponse = responses.Single(response => response.StatusCode == HttpStatusCode.Conflict);
        var conflictJson = await conflictResponse.Content.ReadFromJsonAsync<JsonElement>();

        var message = conflictJson.GetProperty("message").GetString();
        Assert.NotNull(message);
        Assert.Contains("reservado en ese horario", message, StringComparison.OrdinalIgnoreCase);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var reservationCount = await dbContext.Reservations.CountAsync(reservation =>
            reservation.SpaceId == SpaceId
            && reservation.StartAt == startAt
            && reservation.EndAt == endAt
            && reservation.Status == ReservationStatus.Confirmed);

        Assert.Equal(1, reservationCount);
    }

    private static async Task<AuthResponse> RegisterAndAuthenticateAsync(HttpClient client)
    {
        var email = $"integration_{Guid.NewGuid():N}@coworkspaces.local";

        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Integration User",
            email,
            password = "Test1234"
        });

        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!;
    }
}
