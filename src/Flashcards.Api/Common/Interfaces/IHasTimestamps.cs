namespace Flashcards.Api.Common.Interfaces;

public interface IHasTimestamps
{
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}