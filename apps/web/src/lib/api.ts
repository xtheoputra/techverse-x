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

export type RoadmapStep = {
  order: number;
  isPrerequisite: boolean;
  title: string;
  description: string;
};

export type TechnologyTool = {
  slug: string;
  name: string;
  summary: string;
  homepage: string | null;
  note: string | null;
};

export type Project = { id: string; title: string; brief: string };

export type ResourceType = 'OfficialDocs' | 'Video' | 'Paper' | 'Repository';

export type Resource = { id: string; type: ResourceType; title: string; url: string };

/**
 * Satu topik lengkap dengan kelima bagian template ADR-012.
 *
 * `missingSections` datang dari server, bukan dihitung di sini — lihat catatan
 * di `TechnologyResponse`. Aturan "roadmap baru terisi kalau ada langkah SESUDAH
 * langkah 0" tinggal di agregat, dan menyalinnya ke klien adalah cara paling
 * mudah membuat dua jawaban yang berbeda untuk satu pertanyaan.
 */
export type TechnologyDetail = TechnologySummary & {
  status: string;
  reviewedAt: string | null;
  createdAt: string;
  updatedAt: string;
  roadmap: RoadmapStep[];
  tools: TechnologyTool[];
  projects: Project[];
  resources: Resource[];
  missingSections: string[];
};

/**
 * Jawaban `GET /api/v1/search` — bidang DAN topik dalam satu permintaan.
 *
 * 🔑 Keduanya ada karena keduanya punya halaman: ADR-009 memberi URL kanonik ke
 * tiap bidang dan tiap topik, dan `/teknologi/<slug>` melayani keduanya. Hari ini
 * bidangnya bahkan mayoritas — 14 bidang berbanding nol sampai lima topik — jadi
 * pencarian yang cuma menjawab topik akan membalas "tidak ada hasil" untuk hampir
 * setiap kata yang tercetak di halaman muka.
 *
 * `query` adalah kata kunci yang BENAR-BENAR dipakai server (sudah dipangkas dan
 * dipotong ke batas panjangnya), bukan yang diketik. Yang ditampilkan ke pembaca
 * harus yang ini.
 */
export type SearchResult = {
  query: string;
  fields: Field[];
  technologies: PagedResponse<TechnologySummary>;
  isEmpty: boolean;
};

export type PagedResponse<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
};

/**
 * 🔴 `notFound` dipisahkan dari kegagalan lain DENGAN SENGAJA.
 *
 * "Topik ini tidak ada" dan "API-nya sedang tidak bisa dihubungi" tampak sama di
 * lapisan fetch dan **harus berakhir berbeda di halaman**: yang pertama boleh
 * jadi 404, yang kedua tidak pernah boleh. API di Koyeb tidur setelah satu jam
 * (ADR-019), jadi menyamakan keduanya berarti setiap kali instance-nya bangun,
 * halaman yang sehat menjawab "hilang" — ke pembaca maupun ke mesin pengindeks.
 */
export type ApiResult<T> =
  | { ok: true; data: T }
  | { ok: false; reason: string; notFound: boolean };

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
      return gagal(path, `API membalas ${response.status}.`, response.status === 404);
    }

    return { ok: true, data: (await response.json()) as T };
  } catch {
    // API mati bukan alasan halamannya ikut putih. Fase 1 masih sering
    // dijalankan tanpa backend menyala.
    //
    // ⚠️ notFound: false — tidak bisa dihubungi BUKAN tidak ada.
    return gagal(path, `Tidak bisa menghubungi API di ${baseUrl}.`, false);
  }
}

/**
 * Satu tempat keluar untuk kegagalan — dan satu-satunya tempat sebabnya DICATAT.
 *
 * 🔑 **Sebab kegagalan pindah dari HALAMAN ke LOG, bukan hilang.** Sampai
 * 2026-09-10 kalimat ini dicetak apa adanya ke pembaca, lengkap dengan alamat
 * API internal, di empat tempat — dan di produksi itu keadaan yang biasa, bukan
 * langka: instance gratis Koyeb tidur tiap jam (ADR-019). Menyembunyikannya dari
 * halaman tanpa menaruhnya di tempat lain akan MENGHILANGKAN diagnosis yang
 * dipakai #40 untuk membedakan `API_BASE_URL` yang salah dari #39 yang belum
 * selesai. Karena itu ia dicatat di sini.
 *
 * Ini berjalan di server — `server-only` di kepala berkas menjaminnya — jadi
 * keluarannya masuk ke log fungsi Vercel atau stdout peti kemas, bukan ke
 * peramban pembaca.
 *
 * ⚠️ `path`-nya ikut dicatat: tanpa itu, dua komponen yang gagal berbarengan
 * menghasilkan dua baris yang identik dan tak terbedakan.
 */
function gagal<T>(path: string, reason: string, notFound: boolean): ApiResult<T> {
  // 404 bukan insiden — itu jawaban yang sah untuk slug yang memang tidak ada,
  // dan mencatatnya berarti setiap perayap yang menebak URL menghasilkan baris
  // log palsu.
  if (!notFound) {
    console.error(`[TechVerseX] GET ${path} gagal: ${reason}`);
  }

  return { ok: false, reason, notFound };
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

/** Satu topik berikut kelima bagian isinya. */
export function getTechnology(slug: string): Promise<ApiResult<TechnologyDetail>> {
  return getJson<TechnologyDetail>(`/api/v1/technologies/${encodeURIComponent(slug)}`);
}

/**
 * Satu kata kunci, dua jenis hasil.
 *
 * ⚠️ Penyaringan dan peringkatnya dikerjakan PostgreSQL, bukan di sini. Aturan
 * "apa yang dianggap cocok" tinggal di satu tempat — lihat `SearchTechnologyHandler`
 * dan ADR-022. Menyalinnya ke klien adalah cara paling mudah membuat dua jawaban
 * berbeda untuk satu pertanyaan, persis seperti `missingSections` di atas.
 */
export function cari(q: string, pageSize = 20): Promise<ApiResult<SearchResult>> {
  const query = new URLSearchParams({ q, pageSize: String(pageSize) });

  return getJson<SearchResult>(`/api/v1/search?${query}`);
}
