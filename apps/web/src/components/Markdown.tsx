import type { ReactNode } from 'react';
import { parse, type Block, type Inline, type JenisPeringatan, type Perataan } from '@/lib/markdown/parse.mjs';

/**
 * Menampilkan teks isi yang ditulis dengan Markdown terbatas (ADR-028 Tahap 3a).
 *
 * 🔑 **Tak ada `dangerouslySetInnerHTML`.** Pengurai (`lib/markdown/parse.mjs`)
 * menghasilkan POHON, dan komponen ini memetakannya ke elemen React — teks isi tak
 * bisa menyuntik markup, apa pun yang tertulis di dalamnya. Satu-satunya jalur
 * berbahaya yang tersisa adalah skema URL tautan, dan itu sudah ditutup di pengurai:
 * `href` hanya http(s) absolut atau `null`, dan `null` berarti tak ada tautan sama
 * sekali (teksnya saja).
 *
 * Pemeriksa berkas isi (`pasang.mjs --cek`) memakai pengurai YANG SAMA dan menolak
 * konstruksi yang tak didukung, jadi di sini penampil boleh toleran: apa pun yang
 * lolos ke sini tampil sebagai sesuatu, dan tak pernah melempar.
 *
 * `headingBase` adalah tingkat HTML untuk sub-judul `##` di teks; `###` jadi satu
 * tingkat di bawahnya. Pemanggil yang menaruh teks di bawah `<h3>` memberi 4, sehingga
 * kerangka judul halaman tak pernah melompat tingkat.
 */
export default function Markdown({
  source,
  headingBase = 3,
  className = '',
}: {
  source: string;
  headingBase?: number;
  className?: string;
}) {
  const { blocks } = parse(source);

  if (blocks.length === 0) {
    return null;
  }

  return (
    <div className={`prose-tv prose-md ${className}`.trim()}>
      {blocks.map((block, i) => renderBlock(block, i, headingBase))}
    </div>
  );
}

const LABEL_PERINGATAN: Record<JenisPeringatan, string> = {
  catatan: 'Catatan',
  tips: 'Tips',
  peringatan: 'Peringatan',
};

function renderBlock(block: Block, key: number, headingBase: number): ReactNode {
  switch (block.type) {
    case 'paragraph':
      return <p key={key}>{renderInline(block.children)}</p>;

    case 'heading': {
      const tingkat = Math.min(6, headingBase + (block.level - 2));
      const Tag = `h${tingkat}` as 'h3' | 'h4' | 'h5' | 'h6';
      return <Tag key={key}>{renderInline(block.children)}</Tag>;
    }

    case 'code':
      return (
        <figure key={key} className="md-code">
          {block.lang && block.lang !== 'text' ? <figcaption className="md-code-lang">{block.lang}</figcaption> : null}
          {/* Dapat difokus supaya baris panjang bisa digulir dengan papan ketik. */}
          <pre tabIndex={0}>
            <code>{block.text}</code>
          </pre>
        </figure>
      );

    case 'quote': {
      const anak = block.children.map((b, i) => renderBlock(b, i, headingBase));
      if (block.kind === null) {
        return <blockquote key={key}>{anak}</blockquote>;
      }
      return (
        <div key={key} role="note" className={`md-aside md-aside-${block.kind}`}>
          <p className="md-aside-label">{LABEL_PERINGATAN[block.kind]}</p>
          {anak}
        </div>
      );
    }

    case 'list': {
      const butir = block.items.map((item, i) => (
        <li key={i}>
          {/* Butir berparagraf tunggal tak perlu <p> — jarak antar-butir dipegang <li>. */}
          {item.length === 1 && item[0].type === 'paragraph'
            ? renderInline(item[0].children)
            : item.map((b, j) => renderBlock(b, j, headingBase))}
        </li>
      ));
      return block.ordered ? (
        <ol key={key} start={block.start !== 1 ? block.start : undefined}>
          {butir}
        </ol>
      ) : (
        <ul key={key}>{butir}</ul>
      );
    }

    case 'table':
      return (
        // Digulir ke samping di dalam pembungkusnya, bukan membuat halaman meluber di ponsel.
        <div key={key} className="md-table" role="region" aria-label="Tabel" tabIndex={0}>
          <table>
            <thead>
              <tr>
                {block.head.map((sel, i) => (
                  <th key={i} scope="col" style={perataan(block.align[i])}>
                    {renderInline(sel)}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {block.rows.map((baris, i) => (
                <tr key={i}>
                  {baris.map((sel, j) => (
                    <td key={j} style={perataan(block.align[j])}>
                      {renderInline(sel)}
                    </td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      );

    case 'media':
      // Blok media dirender di Tahap 3b, bersama datanya. Sebelum itu ia tak punya apa
      // pun untuk ditampilkan, dan pemeriksa berkas isi menolaknya.
      return null;
  }
}

function perataan(a: Perataan | undefined) {
  return a ? { textAlign: a } : undefined;
}

function renderInline(nodes: Inline[]): ReactNode[] {
  return nodes.map((node, i): ReactNode => {
    switch (node.type) {
      case 'text':
        return node.text;
      case 'code':
        return <code key={i}>{node.text}</code>;
      case 'strong':
        return <strong key={i}>{renderInline(node.children)}</strong>;
      case 'em':
        return <em key={i}>{renderInline(node.children)}</em>;
      case 'link':
        // href null = skema ditolak pengurai: tampilkan teksnya, jangan buat tautan.
        return node.href === null ? (
          <span key={i}>{renderInline(node.children)}</span>
        ) : (
          <a key={i} href={node.href} rel="noreferrer noopener" target="_blank">
            {renderInline(node.children)}
          </a>
        );
    }
  });
}
