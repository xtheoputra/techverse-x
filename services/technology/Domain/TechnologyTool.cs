namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Tautan banyak-ke-banyak antara <see cref="Technology"/> dan <see cref="Tool"/>.
/// </summary>
/// <remarks>
/// Ia punya kelasnya sendiri, bukan tabel bayangan yang dibuatkan EF, karena ia
/// membawa keterangan yang hanya berlaku pada <b>pasangan</b> itu:
/// <see cref="Note"/> menjelaskan kenapa alat ini relevan untuk topik ini.
/// "Docker" pada topik <em>Cloud</em> dan pada topik <em>Edge AI</em> adalah alat
/// yang sama dengan alasan yang berbeda, dan alasan itu tidak punya tempat lain.
/// </remarks>
public sealed class TechnologyTool
{
    private TechnologyTool()
    {
        // Dipakai EF Core.
    }

    private TechnologyTool(Guid technologyId, Guid toolId, string? note)
    {
        TechnologyId = technologyId;
        ToolId = toolId;
        Note = note;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid TechnologyId { get; private set; }

    public Guid ToolId { get; private set; }

    /// <summary>Kenapa alat ini relevan untuk topik ini. Null kalau tidak perlu dijelaskan.</summary>
    public string? Note { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Dipanggil <see cref="Technology"/> saja.</summary>
    internal static TechnologyTool Create(Guid technologyId, Guid toolId, string? note)
    {
        if (toolId == Guid.Empty)
        {
            throw new ArgumentException("Tautan alat harus menunjuk alat yang ada.", nameof(toolId));
        }

        return new TechnologyTool(
            technologyId,
            toolId,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim());
    }
}
