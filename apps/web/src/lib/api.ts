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
 * Berapa lama hasil API boleh dipakai ulang sebelum diambil lagi, dalam detik.
 *
 * 🔑 Angka ini BUKAN setelan performa — ia yang membuat penyebaran gratis
 * mungkin (ADR-019). API berjalan di instance yang **tidur setelah satu jam
 * menganggur**; tanpa cache, setiap pembaca pertama sesudah jeda menunggu
 * instance itu bangun. Dengan cache, pembaca menerima HTML yang sudah jadi, dan
 * yang menunggu bangunnya API adalah penyegaran di latar — bukan orangnya.
 *
 * Lima menit dipilih karena isi TechVerse X berubah dalam hitungan hari, bukan
 * detik. Menaikkannya menghemat bangun, menurunkannya membuat perubahan tampak
 * lebih cepat; keduanya sah. Yang TIDAK sah adalah mengembalikannya ke nol,
 * sebab itu memindahkan penantian kembali ke pembaca.
 */
const REVALIDATE_SECONDS = 300;

/**
 * Di Next.js 16 `fetch` TIDAK di-cache secara bawaan. Di sini ia SENGAJA
 * di-cache lewat `next.revalidate` — lihat {@link REVALIDATE_SECONDS}.
 *
 * ⚠️ `cacheComponents` belum dinyalakan di `next.config.ts`, jadi yang berlaku
 * model caching lama (`next: { revalidate }`), bukan direktif `use cache`.
 * Kalau suatu saat bendera itu dinyalakan, berkas inilah yang pertama harus
 * ditulis ulang.
 */
async function getJson<T>(path: string): Promise<ApiResult<T>> {
  try {
    const response = await fetch(`${baseUrl}${path}`, {
      headers: { Accept: 'application/json' },
      next: { revalidate: REVALIDATE_SECONDS },

      // 🔑 30 detik, bukan 5 - dan itu KONSEKUENSI dari baris di atasnya, bukan
      // kelonggaran. Selama fetch menahan render, batas pendek benar: pembaca
      // tidak boleh menunggu lama. Sesudah di-cache, fetch ini berjalan di
      // penyegaran latar, dan batas pendek justru berbahaya - ia menggugurkan
      // permintaan tepat saat instance yang tidur sedang bangun, lalu GALATNYA
      // yang ter-cache untuk lima menit berikutnya.
      signal: AbortSignal.timeout(30_000),
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
