// Tipe untuk parse.mjs — ADR-028 Tahap 3a. Berkas ini hanya untuk TypeScript (penampil);
// pemeriksa berkas memakai parse.mjs langsung.

export type Inline =
  | { type: 'text'; text: string }
  | { type: 'code'; text: string }
  | { type: 'strong'; children: Inline[] }
  | { type: 'em'; children: Inline[] }
  /** `href` hanya http(s) absolut; `null` berarti ditolak dan penampil tak membuat tautan. */
  | { type: 'link'; href: string | null; children: Inline[] };

export type Perataan = 'left' | 'center' | 'right' | null;

export type JenisPeringatan = 'catatan' | 'tips' | 'peringatan';

export type Block =
  | { type: 'paragraph'; children: Inline[] }
  /** Tingkat relatif: 2 = sub-judul, 3 = anak sub-judul. Penampil memetakannya ke tingkat HTML sesuai konteks halaman. */
  | { type: 'heading'; level: 2 | 3; children: Inline[] }
  | { type: 'code'; lang: string | null; text: string }
  | { type: 'quote'; kind: JenisPeringatan | null; children: Block[] }
  | { type: 'list'; ordered: boolean; start: number; items: Block[][] }
  | { type: 'table'; align: Perataan[]; head: Inline[][]; rows: Inline[][][] }
  | { type: 'media'; key: string };

export interface Problem {
  /** Nomor baris (mulai dari 1) di teks yang diurai. */
  line: number;
  message: string;
}

/** Total: menerima apa pun dan tak pernah melempar. `problems` untuk pemeriksa; penampil mengabaikannya. */
export function parse(sumber: unknown): { blocks: Block[]; problems: Problem[] };

/** Kunci semua blok media di pohon, berurut kemunculan. */
export function mediaKeys(blocks: Block[]): string[];

/** Teks polos pohon (tanpa tanda), untuk menghitung kata. */
export function plainText(blocks: Block[]): string;
