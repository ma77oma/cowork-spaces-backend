using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CoworkSpaces.Application.DTOs.Auth;
using CoworkSpaces.Application.DTOs.Reservations;
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
            && reservation.Status == ReservationStatus.Pending);

        Assert.Equal(1, reservationCount);
    }

    [Fact]
    public async Task ShouldCreateReservationAsPendingAndAllowAdminToConfirmIt()
    {
        var customerClient = _factory.CreateClient();
        var adminClient = _factory.CreateClient();

        var customerAuth = await RegisterAndAuthenticateAsync(customerClient);
        var adminAuth = await LoginAsAdminAsync(adminClient);

        customerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerAuth.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth.Token);

        var startAt = DateTime.UtcNow.AddDays(3);
        startAt = new DateTime(startAt.Year, startAt.Month, startAt.Day, 11, 0, 0, DateTimeKind.Utc);
        var endAt = startAt.AddHours(1);

        var createResponse = await customerClient.PostAsJsonAsync("/api/reservations", new
        {
            spaceId = SpaceId,
            startAt,
            endAt
        });

        createResponse.EnsureSuccessStatusCode();

        var createdReservation = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var reservationId = createdReservation.GetProperty("id").GetGuid();
        var createdStatus = createdReservation.GetProperty("status").GetInt32();

        Assert.Equal((int)ReservationStatus.Pending, createdStatus);

        var confirmResponse = await adminClient.PostAsync($"/api/reservations/{reservationId}/confirm", content: null);

        confirmResponse.EnsureSuccessStatusCode();

        var confirmedReservation = await confirmResponse.Content.ReadFromJsonAsync<JsonElement>();
        var confirmedStatus = confirmedReservation.GetProperty("status").GetInt32();

        Assert.Equal((int)ReservationStatus.Confirmed, confirmedStatus);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var reservation = await dbContext.Reservations.SingleAsync(item => item.Id == reservationId);

        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
    }

    [Fact]
    public async Task ShouldReturnAllReservationsForAdminAndOnlyOwnReservationsForCustomer()
    {
        var customerClient1 = _factory.CreateClient();
        var customerClient2 = _factory.CreateClient();
        var adminClient = _factory.CreateClient();

        var customerAuth1 = await RegisterAndAuthenticateAsync(customerClient1);
        var customerAuth2 = await RegisterAndAuthenticateAsync(customerClient2);
        var adminAuth = await LoginAsAdminAsync(adminClient);

        customerClient1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerAuth1.Token);
        customerClient2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", customerAuth2.Token);
        adminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminAuth.Token);

        var firstStartAt = DateTime.UtcNow.AddDays(4);
        firstStartAt = new DateTime(firstStartAt.Year, firstStartAt.Month, firstStartAt.Day, 9, 0, 0, DateTimeKind.Utc);

        var secondStartAt = firstStartAt.AddHours(3);

        var createResponse1 = await customerClient1.PostAsJsonAsync("/api/reservations", new
        {
            spaceId = SpaceId,
            startAt = firstStartAt,
            endAt = firstStartAt.AddHours(1)
        });

        var createResponse2 = await customerClient2.PostAsJsonAsync("/api/reservations", new
        {
            spaceId = SpaceId,
            startAt = secondStartAt,
            endAt = secondStartAt.AddHours(1)
        });

        createResponse1.EnsureSuccessStatusCode();
        createResponse2.EnsureSuccessStatusCode();

        var createdReservation1 = await createResponse1.Content.ReadFromJsonAsync<ReservationResponse>();
        var createdReservation2 = await createResponse2.Content.ReadFromJsonAsync<ReservationResponse>();

        var customerReservationsResponse = await customerClient1.GetAsync("/api/reservations/my");
        customerReservationsResponse.EnsureSuccessStatusCode();

        var customerReservations = await customerReservationsResponse.Content.ReadFromJsonAsync<List<ReservationResponse>>();

        Assert.NotNull(customerReservations);
        Assert.Contains(customerReservations, item => item.Id == createdReservation1!.Id);
        Assert.DoesNotContain(customerReservations, item => item.Id == createdReservation2!.Id);

        var adminReservationsResponse = await adminClient.GetAsync("/api/reservations/my");
        adminReservationsResponse.EnsureSuccessStatusCode();

        var adminReservations = await adminReservationsResponse.Content.ReadFromJsonAsync<List<ReservationResponse>>();

        Assert.NotNull(adminReservations);
        Assert.Contains(adminReservations, item => item.Id == createdReservation1!.Id);
        Assert.Contains(adminReservations, item => item.Id == createdReservation2!.Id);
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

    private static async Task<AuthResponse> LoginAsAdminAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@coworkspaces.local",
            password = "Admin123"
        });

        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!;
    }
}
