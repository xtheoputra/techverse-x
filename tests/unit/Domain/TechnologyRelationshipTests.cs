using TechVerseX.TechnologyService.Domain;

namespace TechVerseX.TechnologyService.Tests.Domain;

public sealed class TechnologyRelationshipTests
{
    [Fact]
    public void Create_MenyimpanArahDanJenisHubungan()
    {
        var from = Guid.CreateVersion7();
        var to = Guid.CreateVersion7();

        var relationship = TechnologyRelationship.Create(from, to, RelationshipKind.Requires);

        Assert.Equal(from, relationship.FromTechnologyId);
        Assert.Equal(to, relationship.ToTechnologyId);
        Assert.Equal(RelationshipKind.Requires, relationship.Kind);
    }

    [Fact]
    public void Create_MenolakHubunganKeDirinyaSendiri()
    {
        var id = Guid.CreateVersion7();

        Assert.Throws<ArgumentException>(() => TechnologyRelationship.Create(id, id, RelationshipKind.Uses));
    }
}
