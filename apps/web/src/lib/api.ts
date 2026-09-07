import 'server-only';

/**
 * Klien tipis ke TechVerse X API.
 *
 * Rujukan: KERANGKA.md 4.9 — web adalah salah satu klien API, bukan pemilik
 * datanya. Karena itu ia bicara lewat HTTP, tidak menyentuh basis data.
 */

const baseUrl = process.env.API_BASE_URL ?? 'http://localhost:5080';

export type TechnologySummary = {
  id: string;
  slug: string;
  name: string;
  summary: string;
  category: string;
};

export type PagedResponse<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
};

export type ApiResult<T> =
  | { ok: true; data: T }
  | { ok: false; reason: string };

/**
 * Di Next.js 16 `fetch` TIDAK di-cache secara bawaan dan menahan render sampai
 * selesai. Itu yang kita mau di sini — daftar teknologi harus segar. Kalau nanti
 * perlu di-cache, pakai direktif `use cache`, bukan opsi fetch yang lama.
 */
export async function searchTechnologies(
  params: { q?: string; category?: string; pageSize?: number } = {},
): Promise<ApiResult<PagedResponse<TechnologySummary>>> {
  const query = new URLSearchParams();
  if (params.q) query.set('q', params.q);
  if (params.category) query.set('category', params.category);
  query.set('pageSize', String(params.pageSize ?? 20));

  try {
    const response = await fetch(`${baseUrl}/api/v1/technologies?${query}`, {
      headers: { Accept: 'application/json' },
      signal: AbortSignal.timeout(5000),
    });

    if (!response.ok) {
      return { ok: false, reason: `API membalas ${response.status}.` };
    }

    return { ok: true, data: (await response.json()) as PagedResponse<TechnologySummary> };
  } catch {
    // API mati bukan alasan halamannya ikut putih. Fase 1 masih sering
    // dijalankan tanpa backend menyala.
    return { ok: false, reason: `Tidak bisa menghubungi API di ${baseUrl}.` };
  }
}
