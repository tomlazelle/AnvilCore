using System.ComponentModel.DataAnnotations;

namespace AnvilCore.SampleApi;

public sealed record CreateReservationRequest(
    [Required, MinLength(1)] string GuestName,
    [Range(typeof(decimal), "0.01", "999999.99")] decimal Amount);
