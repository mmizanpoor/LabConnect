export interface SupportedLanguage {
  id: string;
  label: string;
  direction: LanguageDirection;
}

export enum LanguageDirection {
  ltr,
  rtl,
}

export const SupportedLanguages: SupportedLanguage[] = [
  { id: 'fa', label: 'فارسی', direction: LanguageDirection.rtl },
];
