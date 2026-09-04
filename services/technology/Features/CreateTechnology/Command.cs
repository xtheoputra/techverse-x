namespace TechVerseX.TechnologyService.Features.CreateTechnology;

public sealed record CreateTechnologyCommand(string Name, string Summary, string FieldSlug, string? Slug);
