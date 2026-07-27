namespace SoundMate.Application.Abstractions.Persistence;

/// <summary>Average star rating and number of reviews for a teacher in an academy.</summary>
public sealed record RatingSummary(double Average, int Count);
