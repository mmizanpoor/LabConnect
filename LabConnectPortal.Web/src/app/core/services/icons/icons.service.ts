import { Injectable } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';

@Injectable({ providedIn: 'root' })
export class IconsService {
  constructor(
    domSanitizer: DomSanitizer,
    matIconRegistry: MatIconRegistry,
  ) {
    matIconRegistry.addSvgIconSet(domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/material-twotone.svg'));
    matIconRegistry.addSvgIconSetInNamespace('custom', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/custom.svg'));
    matIconRegistry.addSvgIconSetInNamespace('mat_outline', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/material-outline.svg'));
    matIconRegistry.addSvgIconSetInNamespace('mat_solid', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/material-solid.svg'));
    matIconRegistry.addSvgIconSetInNamespace('feather', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/feather.svg'));
    matIconRegistry.addSvgIconSetInNamespace('heroicons_outline', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/heroicons-outline.svg'));
    matIconRegistry.addSvgIconSetInNamespace('heroicons_solid', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/heroicons-solid.svg'));
    matIconRegistry.addSvgIconSetInNamespace('heroicons_mini', domSanitizer.bypassSecurityTrustResourceUrl('assets/icons/heroicons-mini.svg'));
  }
}
