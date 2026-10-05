import { Component, OnInit, inject } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import {
  ProductCatalogOptionDto,
  ProductDto,
} from '@modules/admin/products/product-catalog.types';
import { ProductCatalogService } from '@modules/admin/products/product-catalog.service';
import { ProductImageDto } from '@modules/admin/products/product-catalog.types';
import { PublicProductsService } from '@modules/products/products.service';
import { CenterProfileService } from '../center-profile/center-profile.service';
import { CenterProfileDto } from '../center-profile/center-profile.types';
import { ProductListingsService } from './product-listings.service';
import { CenterProductListingDto, ProductListingStatus } from './product-listings.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { MatDialog } from '@angular/material/dialog';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { ActivityLogHistoryDialogComponent } from '@shared/activity-log/activity-log-history-dialog.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';

@Component({
  selector: 'app-product-listing-form',
  standalone: true,
  imports: [
    NgTemplateOutlet,
    ReactiveFormsModule,
    RouterLink,
    BaseCheckboxComponent,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
  ],
  templateUrl: './product-listing-form.component.html',
})
export class ProductListingFormComponent implements OnInit {
  readonly entity = SystemEntity.ProductListing;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ProductListing);
  private readonly _dialog = inject(MatDialog);

  loading = false;
  saving = false;
  publishing = false;
  error = '';
  success = '';
  isNew = true;
  listingId = '';
  listing: CenterProductListingDto | null = null;
  centerProfile: CenterProfileDto | null = null;
  readOnly = false;

  get canEditFields(): boolean {
    return this.canManageListings && (this.isNew || this.listing?.status !== 'Published');
  }
  selectedProduct: ProductDto | null = null;
  catalogOptions: ProductCatalogOptionDto[] = [];
  imageUrl = PublicProductsService.productImageUrl;
  listingImages: ProductImageDto[] = [];
  uploadingListingImage = false;

  form = new FormGroup({
    productId: new FormControl<string | null>(null, Validators.required),
    price: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    stockQuantity: new FormControl<number | null>(null, [Validators.required, Validators.min(0)]),
    discountPercent: new FormControl<number | null>(null, [Validators.min(0), Validators.max(100)]),
    isNegotiablePrice: new FormControl(false),
    isUsed: new FormControl(false),
    sellerDescription: new FormControl(''),
  });

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _productListingsService: ProductListingsService,
    private _productCatalogService: ProductCatalogService,
    private _centerProfileService: CenterProfileService,
    private _localization: LocalizationService,
    private _authUtils: AuthUtils,
    private _labPermission: LabPermissionService,
  ) {}

  get canManageListings(): boolean {
    return this.centerProfile?.isApproved ?? false;
  }

  get canSave(): boolean {
    return this._labPermission.can(
      this.entity,
      this.isNew ? 'create' : 'update'
    );
  }

  get canPublish(): boolean {
    return this._labPermission.can(this.entity, 'update');
  }

  get showProfileStatusMessage(): boolean {
    return this._authUtils.canManageCenterProfile();
  }

  get catalogProductOptions(): BaseFormSelectOption[] {
    return this.catalogOptions.map((opt) => ({
      value: opt.productId,
      label: `${opt.title} – ${opt.brandTitle}`,
    }));
  }

  get profileBannerKey(): string {
    if (!this.centerProfile) {
      return 'modules.profile.productListings.profileNotApprovedBanner';
    }
    if (this.centerProfile.rejectionReason && !this.centerProfile.isComplete) {
      return 'modules.profile.productListings.profileRejectedBanner';
    }
    if (this.centerProfile.isComplete) {
      return 'modules.profile.productListings.profileAwaitingApproval';
    }
    return 'modules.profile.productListings.profileNotApprovedBanner';
  }

  get canUploadListingImages(): boolean {
    return this.canManageListings && !this.isNew && !!this.listingId && this.listing?.isUsed === true && !this.readOnly;
  }

  get needsSaveBeforeUploadImages(): boolean {
    return this.isNew || (!!this.form.controls.isUsed.value && this.listing?.isUsed !== true);
  }

  openHistoryDialog(): void {
    if (!this.listingId) return;

    this._dialog.open(ActivityLogHistoryDialogComponent, {
      width: '40rem',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      autoFocus: false,
      data: {
        entityName: 'CenterProductListing',
        recordKey: this.listingId,
      },
    });
  }

  ngOnInit(): void {
    this.resolveRouteMode();
    this.form.controls.isNegotiablePrice.valueChanges.subscribe((value) => {
      if (!this.readOnly) {
        this.updateNegotiablePriceFields(!!value);
      }
    });
    this.form.controls.productId.valueChanges.subscribe((productId) => {
      if (productId) {
        void this.onProductSelected(productId);
      } else {
        this.selectedProduct = null;
      }
    });
    void this.load();
  }

  private resolveRouteMode(): void {
    const idParam = this._route.snapshot.paramMap.get('id');
    const isNewRoute = this._route.snapshot.routeConfig?.path === 'product-listings/new';
    this.isNew = isNewRoute || !idParam || idParam === 'new';
    this.listingId = this.isNew ? '' : idParam!;
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const profileResult = await this._centerProfileService.getMyProfile();
      if (profileResult.success && profileResult.data) {
        this.centerProfile = profileResult.data;
      }

      if (this.isNew && !this.canManageListings) {
        void this._router.navigate(['/profile/product-listings']);
        return;
      }

      if (!this.isNew) {
        const result = await this._productListingsService.getById(this.listingId);
        if (!result.success || !result.data) {
          throw new Error(result.message ?? this._localization.translate('modules.profile.productListings.errors.loadFailed'));
        }
        this.applyListing(result.data);
      } else if (this.canManageListings) {
        await this.loadCatalogOptions();
      }

      if (!this.canManageListings) {
        this.readOnly = true;
        this.form.disable();
      }
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.productListings.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  private applyListing(data: CenterProductListingDto): void {
    this.listing = data;
    this.selectedProduct = data.product;
    this.listingImages = data.images ?? [];
    this.readOnly = !this.canEditFields;

    this.form.patchValue(
      {
        productId: data.productId,
        price: data.price,
        stockQuantity: data.stockQuantity,
        discountPercent: data.discountPercent ?? null,
        isNegotiablePrice: data.isNegotiablePrice ?? false,
        isUsed: data.isUsed ?? false,
        sellerDescription: data.sellerDescription ?? '',
      },
      { emitEvent: false },
    );

    if (this.readOnly) {
      this.form.disable({ emitEvent: false });
      return;
    }

    this.form.enable({ emitEvent: false });
    if (!this.isNew) {
      this.form.controls.productId.disable({ emitEvent: false });
    }
    this.updateNegotiablePriceFields(data.isNegotiablePrice ?? false);
  }

  async loadCatalogOptions(title?: string | null): Promise<void> {
    const term = title?.trim();
    const result = await this._productCatalogService.searchCatalog({
      title: term || undefined,
      page: 1,
      pageSize: 50,
    });
    if (result.success && result.data) {
      this.catalogOptions = result.data.items;
      return;
    }

    this.catalogOptions = [];
    this.error =
      result.message ?? this._localization.translate('modules.profile.productListings.errors.catalogLoadFailed');
  }

  onCatalogSelectSearch(term: string): void {
    this.error = '';
    void this.loadCatalogOptions(term);
  }

  async onProductSelected(productId: string): Promise<void> {
    if (!productId) {
      this.selectedProduct = null;
      return;
    }

    const result = await this._productCatalogService.getProductById(productId);
    if (result.success && result.data) {
      this.selectedProduct = result.data;
    }
  }

  statusLabel(status: ProductListingStatus): string {
    return this._localization.translate(`enum.productListingStatus.${status}`);
  }

  private updateNegotiablePriceFields(isNegotiable: boolean): void {
    const price = this.form.controls.price;
    const discount = this.form.controls.discountPercent;

    if (isNegotiable) {
      price.clearValidators();
      price.setValue(0, { emitEvent: false });
      price.disable({ emitEvent: false });
      discount.clearValidators();
      discount.setValue(null, { emitEvent: false });
      discount.disable({ emitEvent: false });
    } else {
      price.enable({ emitEvent: false });
      price.setValidators([Validators.required, Validators.min(0)]);
      discount.enable({ emitEvent: false });
      discount.setValidators([Validators.min(0), Validators.max(100)]);
    }

    price.updateValueAndValidity({ emitEvent: false });
    discount.updateValueAndValidity({ emitEvent: false });
  }

  buildCommand() {
    const value = this.form.getRawValue();
    const isNegotiable = value.isNegotiablePrice ?? false;
    return {
      productId: value.productId!,
      price: isNegotiable ? 0 : (value.price ?? 0),
      stockQuantity: value.stockQuantity ?? 0,
      discountPercent: isNegotiable ? null : (value.discountPercent ?? null),
      isNegotiablePrice: isNegotiable,
      isUsed: value.isUsed ?? false,
      sellerDescription: value.sellerDescription?.trim() || null,
    };
  }

  async onListingImageSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.listingId) return;

    if (this.form.controls.isUsed.value && !this.listing?.isUsed) {
      const saved = await this.save();
      if (!saved || !this.listing?.isUsed) {
        input.value = '';
        return;
      }
    }

    if (!this.listing?.isUsed) {
      this.error = this._localization.translate('modules.profile.productListings.saveBeforeUploadImages');
      input.value = '';
      return;
    }

    this.uploadingListingImage = true;
    this.error = '';
    try {
      const result = await this._productListingsService.uploadImage(this.listingId, file);
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.profile.productListings.errors.uploadImageFailed'));
      }
      this.listingImages = [...this.listingImages, result.data];
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.productListings.errors.uploadImageFailed');
    } finally {
      this.uploadingListingImage = false;
      input.value = '';
    }
  }

  async deleteListingImage(image: ProductImageDto): Promise<void> {
    if (!this.listingId || this.readOnly) return;

    this.error = '';
    const result = await this._productListingsService.deleteImage(this.listingId, image.id);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.profile.productListings.errors.deleteImageFailed');
      return;
    }
    this.listingImages = this.listingImages.filter((item) => item.id !== image.id);
  }

  async save(): Promise<boolean> {
    if (!this.canSave) return false;
    if (this.readOnly) return false;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return false;
    }
    this.saving = true;
    this.error = '';
    this.success = '';
    try {
      const command = this.buildCommand();
      const result = this.isNew
        ? await this._productListingsService.create(command)
        : await this._productListingsService.update({ ...command, id: this.listingId });

      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.profile.productListings.errors.saveFailed'));
      }
      this.success = this._localization.translate('modules.profile.productListings.saveSuccess');
      this.isNew = false;
      this.listingId = result.data.id;
      this.applyListing(result.data);
      if (this._router.url !== `/profile/product-listings/${result.data.id}`) {
        await this._router.navigate(['/profile/product-listings', result.data.id]);
      }
      return true;
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.productListings.errors.saveFailed');
      return false;
    } finally {
      this.saving = false;
    }
  }

  async saveAndPublish(): Promise<void> {
    if (!this.canSave || !this.canPublish) return;
    if (this.readOnly || !this.selectedProduct) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const saved = await this.save();
    if (!saved || !this.listingId) return;

    await this.publish();
  }

  async publish(): Promise<void> {
    if (!this.canPublish) return;
    if (!this.listingId || this.isNew) return;
    if (this.listing?.status === 'Published') return;

    this.publishing = true;
    this.error = '';
    try {
      const result = await this._productListingsService.publish(this.listingId);
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.profile.productListings.errors.publishFailed'));
      }
      this.success = this._localization.translate('modules.profile.productListings.publishSuccess');
      this.applyListing(result.data);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.profile.productListings.errors.publishFailed');
    } finally {
      this.publishing = false;
    }
  }

  async unpublish(): Promise<void> {
    if (!this.canPublish) return;
    if (!this.listingId || this.isNew) return;
    const result = await this._productListingsService.unpublish(this.listingId);
    if (!result.success || !result.data) {
      this.error = result.message ?? this._localization.translate('modules.profile.productListings.errors.unpublishFailed');
      return;
    }
    this.applyListing(result.data);
  }
}
