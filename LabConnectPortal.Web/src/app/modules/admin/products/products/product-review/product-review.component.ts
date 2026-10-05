import { Component, OnInit, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { firstValueFrom } from 'rxjs';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { LocationMapPickerComponent } from '@modules/profile/location-map-picker/location-map-picker.component';
import { ProductCatalogService } from '../../product-catalog.service';
import {
  isDateAttributeFieldType,
  isExpiryDateAttributeFieldType,
  ProductAttributeValueDto,
  ProductDto,
  ProductImageDto,
  ProductStatus,
} from '../../product-catalog.types';
import { ProductRejectDialogComponent } from '../product-reject-dialog.component';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

function stripHtmlToText(html: string): string {
  return html.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
}

@Component({
  selector: 'app-product-review',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    LocationMapPickerComponent,
  ],
  templateUrl: './product-review.component.html',
  styleUrl: './product-review.component.scss',
})
export class ProductReviewComponent implements OnInit {
  readonly entity = SystemEntity.Product;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Product);
  private readonly _route = inject(ActivatedRoute);
  private readonly _router = inject(Router);
  private readonly _dialog = inject(MatDialog);
  private readonly _catalog = inject(ProductCatalogService);
  private readonly _localization = inject(LocalizationService);
  private readonly _sitePermission = inject(SitePermissionService);
  private readonly _sanitizer = inject(DomSanitizer);

  loading = true;
  submitting = false;
  error = '';
  success = '';
  product: ProductDto | null = null;
  productId = '';
  readonly adminNotes = new FormControl('', { nonNullable: true });

  ngOnInit(): void {
    this.productId = this._route.snapshot.paramMap.get('id') ?? '';
    void this.load();
  }

  get heroImagePath(): string | null {
    const featured = this.product?.featuredImagePath?.trim();
    if (featured) return featured;
    const firstGallery = this.product?.images?.[0]?.imagePath?.trim();
    return firstGallery || null;
  }

  get galleryImages(): ProductImageDto[] {
    const images = this.product?.images ?? [];
    const featured = this.heroImagePath;
    if (!featured || images.length <= 1) return images;
    const rest = images.filter((img) => img.imagePath?.trim() !== featured);
    return rest.length ? rest : images;
  }

  get shortDescription(): string {
    const text = stripHtmlToText(this.product?.description ?? '');
    if (!text) return '';
    if (text.length <= 140) return text;
    return `${text.slice(0, 140).trim()}…`;
  }

  get hasDescription(): boolean {
    return stripHtmlToText(this.product?.description ?? '').length > 0;
  }

  descriptionHtml(): SafeHtml {
    const raw = this.product?.description ?? '';
    const looksLikeHtml = /<[a-z][\s\S]*>/i.test(raw);
    const html = looksLikeHtml
      ? raw
      : raw
          .replace(/&/g, '&amp;')
          .replace(/</g, '&lt;')
          .replace(/>/g, '&gt;')
          .replace(/\n/g, '<br>');
    return this._sanitizer.bypassSecurityTrustHtml(html);
  }

  get categoriesText(): string {
    const cats = this.product?.categoryTitles ?? [];
    if (!cats.length) return '—';
    return cats.join('، ');
  }

  get locationText(): string {
    const province = this.product?.provinceName?.trim();
    if (province) return province;
    if (this.hasCoordinates) {
      return this._localization.translate('modules.admin.products.locationCoordinatesOnly');
    }
    return this._localization.translate('modules.admin.products.reviewEmpty.location');
  }

  get hasCoordinates(): boolean {
    return this.product?.latitude != null && this.product?.longitude != null;
  }

  get mapExternalUrl(): string {
    const lat = this.product?.latitude;
    const lng = this.product?.longitude;
    if (lat == null || lng == null) return '#';
    return `https://www.openstreetmap.org/?mlat=${lat}&mlon=${lng}#map=16/${lat}/${lng}`;
  }

  get isPending(): boolean {
    const s = this.product?.status;
    return s === 'PendingApproval' || s === 1;
  }

  get isApproved(): boolean {
    const s = this.product?.status;
    return s === 'Approved' || s === 2;
  }

  get isUnpublished(): boolean {
    const s = this.product?.status;
    return s === 'Unpublished' || s === 4;
  }

  get canUpdate(): boolean {
    return this._sitePermission.can(SystemEntity.Product, 'update');
  }

  get statusLabel(): string {
    return this._localization.translate(this.statusKey(this.product?.status));
  }

  attributeRowClass(attr: ProductAttributeValueDto): string {
    if (isExpiryDateAttributeFieldType(attr.fieldType)) return 'attr-row attr-row--expiry';
    if (isDateAttributeFieldType(attr.fieldType)) return 'attr-row attr-row--date';
    return 'attr-row';
  }

  formatAttributeValue(attr: ProductAttributeValueDto): string {
    if (!attr.value) return '—';
    if (!isDateAttributeFieldType(attr.fieldType)) return attr.value;
    const parsed = jMoment(attr.value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : attr.value;
  }

  get statusTone(): string {
    switch (this.product?.status) {
      case 'PendingApproval':
      case 1:
        return 'pending';
      case 'Approved':
      case 2:
        return 'approved';
      case 'Rejected':
      case 3:
        return 'rejected';
      case 'Unpublished':
      case 4:
        return 'draft';
      default:
        return 'draft';
    }
  }

  imageUrl(path?: string | null): string {
    return path ? this._catalog.getProductImageUrl(path) : '';
  }

  formatJalaliDate(value?: string | null): string {
    if (!value?.trim()) return '—';
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD HH:mm') : value.trim();
  }

  formatJalaliDateShort(value?: string | null): string {
    if (!value?.trim()) return '—';
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : value.trim();
  }

  formatPrice(value?: number | null): string {
    if (value == null || Number.isNaN(Number(value))) return '—';
    return Number(value).toLocaleString('fa-IR');
  }

  async load(): Promise<void> {
    if (!this.productId) {
      this.error = this._localization.translate('modules.admin.products.errors.loadFailed');
      this.loading = false;
      return;
    }

    this.loading = true;
    this.error = '';
    const result = await this._catalog.getProductById(this.productId);
    if (!result.success || !result.data) {
      this.error =
        result.message ?? this._localization.translate('modules.admin.products.errors.loadFailed');
      this.product = null;
    } else {
      this.product = result.data;
      this.adminNotes.setValue('');
    }
    this.loading = false;
  }

  async approve(): Promise<void> {
    if (!this.product || !this.isPending || this.submitting) return;
    this.submitting = true;
    this.error = '';
    this.success = '';
    const result = await this._catalog.approveProduct(this.product.productId);
    if (!result.success || !result.data) {
      this.error =
        result.message ?? this._localization.translate('modules.admin.products.errors.approveFailed');
      this.submitting = false;
      return;
    }
    this.product = result.data;
    this.success = this._localization.translate('modules.admin.products.approveSuccess');
    this.submitting = false;
  }

  async requestRevision(): Promise<void> {
    if (!this.product || !this.isPending || this.submitting) return;
    const notes = this.adminNotes.value.trim();
    if (notes.length < 3) {
      this.error = this._localization.translate('modules.admin.products.adminNotesRequired');
      return;
    }
    await this.submitRejection(notes, 'revision');
  }

  async reject(): Promise<void> {
    if (!this.product || !this.isPending || this.submitting) return;

    let reason = this.adminNotes.value.trim();
    if (reason.length < 3) {
      const ref = this._dialog.open(ProductRejectDialogComponent, {
        width: '440px',
        panelClass: BASE_DIALOG_PANEL_CLASS,
      });
      const fromDialog = await firstValueFrom(ref.afterClosed());
      if (!fromDialog) return;
      reason = String(fromDialog).trim();
    }

    await this.submitRejection(reason, 'reject');
  }

  private async submitRejection(
    reason: string,
    mode: 'reject' | 'revision',
  ): Promise<void> {
    if (!this.product) return;
    this.submitting = true;
    this.error = '';
    this.success = '';
    const result = await this._catalog.rejectProduct({
      productId: this.product.productId,
      rejectionReason: reason,
    });
    if (!result.success || !result.data) {
      this.error =
        result.message ?? this._localization.translate('modules.admin.products.errors.rejectFailed');
      this.submitting = false;
      return;
    }
    this.product = result.data;
    this.adminNotes.setValue('');
    this.success = this._localization.translate(
      mode === 'revision'
        ? 'modules.admin.products.revisionSuccess'
        : 'modules.admin.products.rejectSuccess',
    );
    this.submitting = false;
  }

  async unpublish(): Promise<void> {
    if (!this.product || !this.isApproved || this.submitting) return;
    this.submitting = true;
    this.error = '';
    this.success = '';
    const result = await this._catalog.unpublishProduct(this.product.productId);
    if (!result.success || !result.data) {
      this.error =
        result.message ?? this._localization.translate('modules.admin.products.errors.unpublishFailed');
      this.submitting = false;
      return;
    }
    this.product = result.data;
    this.success = this._localization.translate('modules.admin.products.unpublishSuccess');
    this.submitting = false;
  }

  async republish(): Promise<void> {
    if (!this.product || !this.isUnpublished || this.submitting) return;
    this.submitting = true;
    this.error = '';
    this.success = '';
    const result = await this._catalog.approveProduct(this.product.productId);
    if (!result.success || !result.data) {
      this.error =
        result.message ?? this._localization.translate('modules.admin.products.errors.republishFailed');
      this.submitting = false;
      return;
    }
    this.product = result.data;
    this.success = this._localization.translate('modules.admin.products.republishSuccess');
    this.submitting = false;
  }

  openEdit(): void {
    if (!this.canUpdate) return;
    void this._router.navigate(['/admin/products', this.productId, 'edit']);
  }

  private statusKey(status?: ProductStatus | null): string {
    switch (status) {
      case 'PendingApproval':
      case 1:
        return 'modules.admin.products.status.pendingAdminApproval';
      case 'Approved':
      case 2:
        return 'modules.admin.products.status.approved';
      case 'Rejected':
      case 3:
        return 'modules.admin.products.status.rejected';
      case 'Unpublished':
      case 4:
        return 'modules.admin.products.status.unpublished';
      default:
        return 'modules.admin.products.status.draft';
    }
  }
}
