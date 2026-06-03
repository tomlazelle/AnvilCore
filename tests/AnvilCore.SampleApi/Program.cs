using AnvilCore.SampleApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<IMessageBus, NoOpMessageBus>();
builder.Services.AddSingleton<IPaymentGateway, AlwaysSucceedPaymentGateway>();
builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddTransient<IReservationService, ReservationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Test"))
    app.MapOpenApi();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.MapPost("/reservations", async (CreateReservationRequest request, IReservationService service) =>
{
    if (string.IsNullOrWhiteSpace(request.GuestName))
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { ["GuestName"] = ["Guest name is required."] });

    if (request.Amount <= 0)
        return Results.ValidationProblem(
            new Dictionary<string, string[]> { ["Amount"] = ["Amount must be greater than zero."] });

    var reservation = await service.CreateAsync(request);
    return reservation is null
        ? Results.Problem(title: "Payment declined", statusCode: StatusCodes.Status402PaymentRequired)
        : Results.Created($"/reservations/{reservation.Id}", reservation);
});

app.Run();

public partial class Program { }
