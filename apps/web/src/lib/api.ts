import 'server-only';

/**
 * Klien tipis ke TechVerse X API.
 *
 * Rujukan: KERANGKA.md 4.9 — web adalah salah satu klien API, bukan pemilik
 * datanya. Karena itu ia bicara lewat HTTP, tidak menyentuh basis data.
 */

const baseUrl = process.env.API_BASE_URL ?? 'http://localhost:5080';

/**
 * Tiga tingkat kematangan isi dari ADR-012. Tipenya sempit dengan sengaja:
 * menambah tingkat baru harus memaksa setiap tempat yang menampilkannya ikut
 * diperbarui, bukan lolos sebagai string bebas.
 */
export type ContentMaturity = 'Curated' | 'MachineDrafted' | 'HumanReviewed';

export type TechnologySummary = {
  id: string;
  slug: string;
  name: string;
  summary: string;
  fieldSlug: string;
  fieldName: string;
  maturity: ContentMaturity;
};

export type Field = {
  id: string;
  slug: string;
  name: string;
  summary: string;
  priority: 'Core' | 'Supporting' | 'Peripheral';
  displayOrder: number;
  topicCount: number;
  reviewedTopicCount: number;
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
 * selesai. Itu yang kita mau di sini — daftarnya harus segar. Kalau nanti perlu
 * di-cache, pakai direktif `use cache`, bukan opsi fetch yang lama.
 */
async function getJson<T>(path: string): Promise<ApiResult<T>> {
  try {
    const response = await fetch(`${baseUrl}${path}`, {
      headers: { Accept: 'application/json' },
      signal: AbortSignal.timeout(5000),
    });

    if (!response.ok) {
      return { ok: false, reason: `API membalas ${response.status}.` };
    }

    return { ok: true, data: (await response.json()) as T };
  } catch {
    // API mati bukan alasan halamannya ikut putih. Fase 1 masih sering
    // dijalankan tanpa backend menyala.
    return { ok: false, reason: `Tidak bisa menghubungi API di ${baseUrl}.` };
  }
}

export function searchTechnologies(
  params: { q?: string; field?: string; pageSize?: number } = {},
): Promise<ApiResult<PagedResponse<TechnologySummary>>> {
  const query = new URLSearchParams();
  if (params.q) query.set('q', params.q);
  if (params.field) query.set('field', params.field);
  query.set('pageSize', String(params.pageSize ?? 20));

  return getJson<PagedResponse<TechnologySummary>>(`/api/v1/technologies?${query}`);
}

/** Keempat belas bidang ADR-010. Daftarnya tertutup, jadi tidak ada penomoran halaman. */
export function listFields(): Promise<ApiResult<Field[]>> {
  return getJson<Field[]>('/api/v1/fields');
}
