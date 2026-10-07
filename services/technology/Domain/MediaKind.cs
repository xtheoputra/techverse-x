namespace TechVerseX.TechnologyService.Domain;

/// <summary>
/// Jenis media sebuah topik (ADR-028 Tahap 3b). Dua, dan bertipe — bukan blok generik:
/// aturan "gambar wajib punya berkas dan teks alternatif, video wajib punya ID" harus
/// bisa dijawab basis data (ADR-015), bukan hanya komentar.
/// </summary>
public enum MediaKind
{
    /// <summary>Gambar atau diagram yang disajikan dari berkas sendiri di <c>/media/…</c>.</summary>
    Image = 1,

    /// <summary>Video resmi yang disematkan lewat <c>youtube-nocookie.com</c>, dikenali dari ID-nya.</summary>
    Video = 2,
}
