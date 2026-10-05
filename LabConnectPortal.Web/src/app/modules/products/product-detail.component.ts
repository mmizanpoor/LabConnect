import { DecimalPipe, NgClass } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { firstValueFrom } from 'rxjs';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { BreadcrumbService } from '@core/services/breadcrumb/breadcrumb.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import { PortalLabAccessService } from '@core/services/site/portal-lab-access.service';
import {
  isDateAttributeFieldType,
  isExpiryDateAttributeFieldType,
  ProductAttributeValueDto,
} from '../admin/products/product-catalog.types';
import { PublicProductsService } from './products.service';
import { PublicProductListingDetailDto, SellerContactDto } from './products.types';
import { ContactLoginDialogComponent } from './contact-login-dialog.component';
import {
  ProductImagePreviewDialogComponent,
} from './product-image-preview-dialog.component';
import { LocationMapPickerComponent } from '@modules/profile/location-map-picker/location-map-picker.component';
import { isResumeSectionComplete } from '../profile/resume/resume-completeness';
import { ResumeService } from '../profile/resume/resume.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

const MS_PER_DAY = 24 * 60 * 60 * 1000;

function stripHtmlToText(html: string): string {
  return html.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
}

function toSafeRichHtml(raw: string, sanitizer: DomSanitizer): SafeHtml {
  const value = raw ?? '';
  const looksLikeHtml = /<[a-z][\s\S]*>/i.test(value);
  const html = looksLikeHtml
    ? value
    : value
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/\n/g, '<br>');
  return sanitizer.bypassSecurityTrustHtml(html);
}

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [NgClass, MatButtonModule, MatIconModule, DecimalPipe, TranslocoPipe, LocationMapPickerComponent],
  templateUrl: './product-detail.component.html',
  styleUrl: './product-detail.component.scss',
})
export class ProductDetailComponent implements OnInit {
  readonly entity = SystemEntity.Product;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Product);
  private _dialog = inject(MatDialog);
  private _portalLabAccess = inject(PortalLabAccessService);
  private _sanitizer = inject(DomSanitizer);

  loading = true;
  error = '';
  product: PublicProductListingDetailDto | null = null;
  imageUrl = PublicProductsService.productImageUrl;
  centerLogoUrl = PublicProductsService.centerLogoUrl;
  activeImageIndex = 0;
  contactLoading = false;
  contactError = '';
  contactRevealed = false;
  sellerContact: SellerContactDto | null = null;
  resumeComplete = false;
  resumeAlreadySent = false;
  resumeSubmitLoading = false;
  resumeSubmitError = '';
  resumeSubmitSuccess = '';

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _publicProductsService: PublicProductsService,
    private _resumeService: ResumeService,
    private _breadcrumb: BreadcrumbService,
    readonly authUtils: AuthUtils,
    private _localization: LocalizationService,
  ) {}

  get showResumeApplyBox(): boolean {
    if (!this.product?.acceptsResume) return false;
    if (!this.authUtils.isAuthenticated) return true;
    return !this.authUtils.isStore() && !this.authUtils.isLabPortalUser();
  }

  get publishedDateLabel(): string {
    const publishedAt = this.product?.publishedAt;
    if (!publishedAt) return '';
    const parsed = jMoment(publishedAt).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : '';
  }

  get relativePublishedLabel(): string {
    const publishedAt = this.product?.publishedAt;
    if (!publishedAt) return '';

    const published = new Date(publishedAt);
    if (Number.isNaN(published.getTime())) return '';

    const days = Math.max(0, Math.floor((Date.now() - published.getTime()) / MS_PER_DAY));

    if (days < 30) {
      return this._localization.translate('modules.products.relativeTime.daysAgo', {
        count: this.formatFaNumber(Math.max(days, 1)),
      });
    }

    if (days < 365) {
      const months = Math.max(1, Math.floor(days / 30));
      return this._localization.translate('modules.products.relativeTime.monthsAgo', {
        count: this.formatFaNumber(months),
      });
    }

    const years = Math.max(1, Math.floor(days / 365));
    if (years === 1) {
      return this._localization.translate('modules.products.relativeTime.oneYearAgo');
    }
    return this._localization.translate('modules.products.relativeTime.yearsAgo', {
      count: this.formatFaNumber(years),
    });
  }

  private formatFaNumber(value: number): string {
    return new Intl.NumberFormat('fa-IR', {
      useGrouping: true,
      maximumFractionDigits: 0,
    }).format(value);
  }

  ngOnInit(): void {
    void this._portalLabAccess.ensure();
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    const id = this._route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = this._localization.translate('modules.products.errors.notFound');
      this.loading = false;
      return;
    }

    void this._publicProductsService.recordView(id).catch(() => undefined);

    try {
      const result = await this._publicProductsService.getProductDetail(id);
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.products.errors.notFound'));
      }
      this.product = result.data;
      this.activeImageIndex = 0;
      this._breadcrumb.setDynamicLabel(result.data.title);
      try {
        this.maybeAutoRevealContact();
      } catch {
        // ignore contact auto-reveal failures
      }
      void this.loadResumeContext();
    } catch (e: unknown) {
      const message =
        e instanceof Error
          ? e.message
          : typeof e === 'object' && e && 'message' in e && typeof (e as { message?: unknown }).message === 'string'
            ? (e as { message: string }).message
            : this._localization.translate('modules.products.errors.loadFailed');
      this.error = message || this._localization.translate('modules.products.errors.loadFailed');
      this.product = null;
    } finally {
      this.loading = false;
    }
  }

  get isNegotiablePrice(): boolean {
    return this.product?.isNegotiablePrice ?? false;
  }

  private maybeAutoRevealContact(): void {
    if (this.authUtils.isAuthenticated) {
      void this.revealContact();
    }
  }

  async onViewContactClick(): Promise<void> {
    if (!this.product || this.authUtils.isAuthenticated) return;

    const dialogRef = this._dialog.open(ContactLoginDialogComponent, {
      width: '480px',
      maxWidth: '92vw',
      autoFocus: false,
      disableClose: false,
      panelClass: 'contact-login-dialog-panel',
    });

    const loggedIn = (await firstValueFrom(dialogRef.afterClosed())) === true;
    if (loggedIn) {
      void this.revealContact();
      void this.loadResumeContext();
    }
  }

  async revealContact(): Promise<void> {
    if (!this.product) return;

    this.contactLoading = true;
    this.contactError = '';
    try {
      const result = await this._publicProductsService.getSellerContact(this.product.productId);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.products.errors.contactLoadFailed'),
        );
      }
      this.sellerContact = result.data;
      this.contactRevealed = true;
    } catch (e: unknown) {
      this.contactError =
        e instanceof Error ? e.message : this._localization.translate('modules.products.errors.contactLoadFailed');
    } finally {
      this.contactLoading = false;
    }
  }

  get activeImage(): PublicProductListingDetailDto['images'][number] | null {
    if (!this.product?.images.length) return null;
    return this.product.images[this.activeImageIndex] ?? this.product.images[0] ?? null;
  }

  get visibleAttributeValues(): ProductAttributeValueDto[] {
    return (this.product?.attributeValues ?? []).filter((attr) => this.hasAttributeValue(attr));
  }

  get hasSpecs(): boolean {
    return this.visibleAttributeValues.length > 0;
  }

  get hasExpertReviews(): boolean {
    return (this.product?.expertReviews?.length ?? 0) > 0;
  }

  get hasLocation(): boolean {
    return this.product?.latitude != null && this.product?.longitude != null;
  }

  get hasDescription(): boolean {
    return stripHtmlToText(this.product?.description ?? '').length > 0;
  }

  descriptionHtml(): SafeHtml {
    return toSafeRichHtml(this.product?.description ?? '', this._sanitizer);
  }

  hasAttributeValue(attr: ProductAttributeValueDto): boolean {
    return !!attr.value?.trim();
  }

  attributeValueClass(attr: ProductAttributeValueDto): string {
    if (isExpiryDateAttributeFieldType(attr.fieldType)) {
      return 'text-amber-800 font-semibold bg-amber-50 px-2.5 py-1 rounded-md';
    }
    if (isDateAttributeFieldType(attr.fieldType)) {
      return 'text-sky-800 font-semibold bg-sky-50 px-2.5 py-1 rounded-md';
    }
    return 'text-gray-900';
  }

  formatAttributeValue(attr: ProductAttributeValueDto): string {
    if (!attr.value) return '—';
    if (!isDateAttributeFieldType(attr.fieldType)) return attr.value;
    const parsed = jMoment(attr.value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : attr.value;
  }

  selectImage(index: number): void {
    if (!this.product || index < 0 || index >= this.product.images.length) return;
    this.activeImageIndex = index;
  }

  openImagePreview(): void {
    if (!this.product || !this.activeImage) return;

    this._dialog.open(ProductImagePreviewDialogComponent, {
      data: {
        imagePath: this.activeImage.imagePath,
        title: this.product.title,
      },
      width: '720px',
      maxWidth: '94vw',
      autoFocus: false,
      panelClass: 'ad-preview-dialog-panel',
    });
  }

  goToLogin(): void {
    if (!this.product) return;
    void this._router.navigate(['/auth/login'], {
      queryParams: { returnUrl: `/products/${this.product.productId}` },
    });
  }

  goToResume(): void {
    void this._router.navigate(['/profile/resume']);
  }

  async submitResume(): Promise<void> {
    if (!this.product?.acceptsResume || this.resumeAlreadySent) return;
    if (!this.authUtils.isAuthenticated) {
      this.goToLogin();
      return;
    }
    if (!this.authUtils.isUser() || !this.resumeComplete) return;

    this.resumeSubmitLoading = true;
    this.resumeSubmitError = '';
    this.resumeSubmitSuccess = '';
    try {
      const result = await this._publicProductsService.submitResume(this.product.productId);
      if (!result.success) {
        throw new Error(
          result.message ?? this._localization.translate('modules.products.applyResume.submitFailed'),
        );
      }
      this.resumeAlreadySent = true;
      this.resumeSubmitSuccess = this._localization.translate('modules.products.applyResume.success');
    } catch (e: unknown) {
      this.resumeSubmitError =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.products.applyResume.submitFailed');
    } finally {
      this.resumeSubmitLoading = false;
    }
  }

  private async loadResumeContext(): Promise<void> {
    this.resumeComplete = false;
    this.resumeAlreadySent = false;
    this.resumeSubmitError = '';
    this.resumeSubmitSuccess = '';
    if (!this.product?.acceptsResume || !this.authUtils.isAuthenticated || !this.authUtils.isUser()) {
      return;
    }

    try {
      const resume = await this._resumeService.getMyResume();
      this.resumeComplete =
        resume.isCompleteForApplication === true || isResumeSectionComplete('basicInfo', resume);
    } catch {
      this.resumeComplete = false;
    }

    try {
      const apps = await this._publicProductsService.getMyResumeApplications();
      if (apps.success && apps.data) {
        this.resumeAlreadySent = apps.data.some((item) => item.productId === this.product?.productId);
      }
    } catch {
      this.resumeAlreadySent = false;
    }
  }
}
