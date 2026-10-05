const TEXT_KEYS = new Set(['text', 'tlp']);

/** Structural SFDT keys that may contain nested text content. */
const STRUCTURE_KEYS = new Set([
  'sections',
  'sec',
  'blocks',
  'b',
  'inlines',
  'i',
  'rows',
  'cells',
  'c',
  'headersFooters',
  'hf',
  'header',
  'footer',
  'evenHeader',
  'evenFooter',
  'firstPageHeader',
  'firstPageFooter',
]);

export function sfdtToPlainText(content?: string | null): string {
  if (!content?.trim()) return '';

  const trimmed = content.trim();
  if (!trimmed.startsWith('{')) return stripMarkup(trimmed);

  try {
    const doc = JSON.parse(trimmed) as Record<string, unknown>;
    const sections = doc['sections'] ?? doc['sec'];
    const text = Array.isArray(sections)
      ? joinParts(sections.map((section) => extractStructure(section)), '\n\n')
      : extractStructure(doc);

    return text.replace(/\n{3,}/g, '\n\n').trim();
  } catch {
    return stripMarkup(trimmed);
  }
}

function extractStructure(node: unknown, key?: string): string {
  if (node == null) return '';

  if (typeof node === 'string') return '';

  if (Array.isArray(node)) {
    if (key === 'sections' || key === 'sec') {
      return joinParts(node.map((item) => extractStructure(item)), '\n\n');
    }
    if (key === 'blocks' || key === 'b') {
      return joinParts(node.map((item) => extractStructure(item)), '\n');
    }
    if (key === 'inlines' || key === 'i') {
      return node.map((item) => extractStructure(item)).join('');
    }
    if (key === 'rows') {
      return joinParts(node.map((item) => extractStructure(item)), '\n');
    }
    if (key === 'cells' || key === 'c') {
      return joinParts(node.map((item) => extractStructure(item)), '\t');
    }
    return joinParts(node.map((item) => extractStructure(item)), '\n');
  }

  if (typeof node !== 'object') return '';

  const obj = node as Record<string, unknown>;
  const parts: string[] = [];

  for (const [entryKey, value] of Object.entries(obj)) {
    if (TEXT_KEYS.has(entryKey) && typeof value === 'string') {
      const plain = stripMarkup(value);
      if (plain) parts.push(plain);
      continue;
    }

    if (!STRUCTURE_KEYS.has(entryKey)) continue;

    const extracted = extractStructure(value, entryKey);
    if (extracted) parts.push(extracted);
  }

  if (key === 'inlines' || key === 'i') return parts.join('');
  if (key === 'blocks' || key === 'b') return parts.join('\n');
  if (key === 'sections' || key === 'sec') return parts.join('\n\n');
  if (key === 'cells' || key === 'c') return parts.join('\t');
  if (key === 'rows') return parts.join('\n');

  return parts.join('\n');
}

function joinParts(parts: string[], separator: string): string {
  return parts.filter(Boolean).join(separator);
}

function stripMarkup(value: string): string {
  if (!value) return '';

  if (typeof document !== 'undefined') {
    const element = document.createElement('div');
    element.innerHTML = value;
    return (element.textContent ?? element.innerText ?? '').replace(/\u00a0/g, ' ').trim();
  }

  return value
    .replace(/<[^>]+>/g, '')
    .replace(/&nbsp;/gi, ' ')
    .replace(/&amp;/gi, '&')
    .replace(/&lt;/gi, '<')
    .replace(/&gt;/gi, '>')
    .replace(/&quot;/gi, '"')
    .replace(/&#39;/gi, "'")
    .trim();
}
