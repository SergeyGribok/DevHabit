using System.Linq.Expressions;

namespace DevHabit.Api.DTOs.Tags;

internal static class TagQueries
{
    public static Expression<Func<Entities.Tag, TagDto>> ProjectToDto()
    {
        return tag => new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description,
            CreatedAtUtc = tag.CreatedAtUtc,
            UpdatedAtUtc = tag.UpdatedAtUtc
        };
    }
}
