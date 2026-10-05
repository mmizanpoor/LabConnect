import { EnvironmentProviders, inject, isDevMode, Provider, provideAppInitializer } from '@angular/core';
import { provideTransloco } from '@jsverse/transloco';
import { lastValueFrom } from 'rxjs';
import { LocalizationService } from './localization.service';
import { SupportedLanguages } from './langs';
import { TranslocoHttpLoader } from './transloco.http-loader';

export const provideTranslocoConfig = (): Array<Provider | EnvironmentProviders> => {
  return [
    provideTransloco({
      config: {
        availableLangs: SupportedLanguages.map((l) => ({ id: l.id, label: l.label })),
        defaultLang: 'fa',
        fallbackLang: 'fa',
        reRenderOnLangChange: true,
        prodMode: !isDevMode(),
      },
      loader: TranslocoHttpLoader,
    }),
    provideAppInitializer(() => {
      const translationService = inject(LocalizationService);
      const lang = translationService.getLangFromLocalStorage();
      translationService.transloco.setActiveLang(lang);
      return lastValueFrom(translationService.transloco.load(lang));
    }),
  ];
};
