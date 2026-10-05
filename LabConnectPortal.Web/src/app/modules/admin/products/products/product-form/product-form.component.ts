import { Location } from '@angular/common';
import { Component, ElementRef, OnDestroy, OnInit, inject } from '@angular/core';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LocalizationService } from '@core/services/localization/localization.service';
import { PortalLabAccessService } from '@core/services/site/portal-lab-access.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BASE_DIALOG_PANEL_CLASS,
} from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseConfirmDialogComponent } from '@modules/base/components/base-dialog/base-confirm-dialog.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { RichTextEditorComponent } from '@modules/base/components/rich-text-editor/rich-text-editor.component';
import { ActivityLogHistoryDialogComponent } from '@shared/activity-log/activity-log-history-dialog.component';
import { LocationMapPickerComponent } from '@modules/profile/location-map-picker/location-map-picker.component';
import { ProfileService } from '@modules/profile/profile.service';
import { firstValueFrom } from 'rxjs';
import { ProductCatalogService } from '../../product-catalog.service';
import {
  BrandDto,
  ProductAttributeDto,
  ProductCategoryDto,
  ProductCategoryGroupDto,
  ProductExpertReviewDto,
  ProductImageDto,
  ProductStatus,
  isDateAttributeFieldType,
} from '../../product-catalog.types';
import jMoment, { Moment } from 'moment-jalaali';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { ProductRejectDialogComponent } from '../product-reject-dialog.component';
import {
  ExpertReviewFormDialogComponent,
  ExpertReviewFormDialogResult,
} from './expert-review-form-dialog.component';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

export type ProductWizardStep = 'basic' | 'offer' | 'location' | 'specs' | 'media';

interface ProductGalleryEntry {
  id: string;
  previewUrl: string;
  file?: File;
  serverImage?: ProductImageDto;
}

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    BaseCheckboxComponent,
    MatDialogModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseImageUploadComponent,
    RichTextEditorComponent,
    LocationMapPickerComponent,
  ],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss',
})
export class ProductFormComponent implements OnInit, OnDestroy {
  readonly entity =
    (inject(ActivatedRoute).snapshot.data['systemEntity'] as string | undefined) ?? SystemEntity.Product;
  private readonly _systemEntityBinding = bindSystemEntity(this.entity);
  private readonly _authUtils = inject(AuthUtils);
  private readonly _route = inject(ActivatedRoute);
  private readonly _host = inject(ElementRef<HTMLElement>);
  private readonly _dialog = inject(MatDialog);
  private readonly _location = inject(Location);
  private readonly _profileService = inject(ProfileService);
  private readonly _portalLabAccess = inject(PortalLabAccessService);

  loading = false;
  saving = false;
  uploading = false;
  uploadingFeatured = false;
  submitting = false;
  galleryDragging = false;
  error = '';
  isEdit = false;
  productId: string | null = null;
  status: ProductStatus | null = null;
  rejectionReason = '';
  createdBySiteAdmin = true;
  brands: BrandDto[] = [];
  allCategories: ProductCategoryDto[] = [];
  categoryGroups: ProductCategoryGroupDto[] = [];
  attributes: ProductAttributeDto[] = [];
  /** For each displayed attribute id, all matching attribute ids across selected categories. */
  private attributeAliasIds: Record<number, number[]> = {};
  expertReviews: ProductExpertReviewDto[] = [];
  images: ProductImageDto[] = [];
  galleryEntries: ProductGalleryEntry[] = [];
  featuredImagePath = '';
  pendingFeaturedFile: File | null = null;
  pendingFeaturedPreviewUrl = '';
  provinceName = '';
  listLink = '/admin/products';

  attributeControls: Record<number, FormControl> = {};
  expertReviewForms = new FormArray<FormGroup>([]);

  private readonly allWizardSteps: { id: ProductWizardStep; labelPath: string }[] = [
    { id: 'basic', labelPath: 'basicInfoSection' },
    { id: 'offer', labelPath: 'offerSection' },
    { id: 'location', labelPath: 'locationSection' },
    { id: 'specs', labelPath: 'specsSection' },
    { id: 'media', labelPath: 'mediaSection' },
  ];

  /** Profile listing form uses آگهی copy for these paths. */
  private static readonly listingFormOverrides = new Set([
    'addTitle',
    'editTitle',
    'basicInfoSection',
    'basicInfoHint',
    'attributesSection',
    'specsSection',
    'specsHint',
    'specsNeedCategory',
    'mediaSection',
    'mediaHint',
    'submitProduct',
    'submitAndPublish',
    'saveDraft',
    'confirmPublishTitle',
    'confirmPublishMessage',
    'confirmPublishConfirm',
    'publishRequiresSave',
    'previewTitle',
    'previewUntitled',
    'stepperLabel',
    'tipAccurate',
    'unpublishSuccess',
    'republishSuccess',
    'fields.description',
    'fields.negotiablePrice',
    'fields.discountPercent',
    'fields.stockUnit',
    'errors.loadFailed',
    'errors.saveFailed',
    'errors.deleteFailed',
    'errors.uploadFailed',
    'errors.deleteImageFailed',
    'errors.unpublishFailed',
    'errors.republishFailed',
  ]);

  /** Resolve admin vs profile listing i18n key for product form copy. */
  productsKey(path: string): string {
    if (this.isProfileProductForm && ProductFormComponent.listingFormOverrides.has(path)) {
      return `modules.profile.productListings.form.${path}`;
    }
    return `modules.admin.products.${path}`;
  }
  private currentStepId: ProductWizardStep = 'basic';
  private maxUnlockedStepId: ProductWizardStep = 'basic';
  /** Steps the user has left via «ادامه» (used for completion ticks). */
  private readonly visitedStepIds = new Set<ProductWizardStep>();

  form = new FormGroup({
    productCategoryGroupId: new FormControl<number | null>(null, Validators.required),
    categoryIds: new FormControl<number[]>([], Validators.required),
    title: new FormControl('', Validators.required),
    brandId: new FormControl<number | null>(null),
    warranty: new FormControl(''),
    description: new FormControl(''),
    isNegotiablePrice: new FormControl(true, { nonNullable: true }),
    isUsed: new FormControl(false, { nonNullable: true }),
    price: new FormControl<number | null>(0, [Validators.min(0)]),
    stockQuantity: new FormControl<number | null>(1, [Validators.required, Validators.min(0)]),
    discountPercent: new FormControl<number | null>(null, [Validators.min(0), Validators.max(100)]),
    latitude: new FormControl<number | null>(null),
    longitude: new FormControl<number | null>(null),
  });

  constructor(
    private _catalogService: ProductCatalogService,
    private _router: Router,
    private _localization: LocalizationService,
  ) {}

  get isSiteAdmin(): boolean {
    return this._authUtils.canAccessAdminPortal();
  }

  get pageTitleKey(): string {
    return this.isEdit ? this.productsKey('editTitle') : this.productsKey('addTitle');
  }

  get canUploadMedia(): boolean {
    return this.canEditContent;
  }

  get featuredPreviewUrl(): string {
    if (this.pendingFeaturedPreviewUrl) return this.pendingFeaturedPreviewUrl;
    return this.featuredImagePath ? this.getImageUrl(this.featuredImagePath) : '';
  }

  get showDetails(): boolean {
    return (this.form.controls.categoryIds.value?.length ?? 0) > 0;
  }

  get canEditContent(): boolean {
    if (this.isSiteAdmin) {
      return true;
    }
    if (this.status == null) return true;
    const s = String(this.status);
    return (
      s === 'Draft' ||
      s === 'Rejected' ||
      s === 'Unpublished' ||
      s === '0' ||
      s === '3' ||
      s === '4'
    );
  }

  get isApproved(): boolean {
    const s = String(this.status);
    return s === 'Approved' || s === '2';
  }

  get isUnpublished(): boolean {
    const s = String(this.status);
    return s === 'Unpublished' || s === '4';
  }

  get isPending(): boolean {
    const s = String(this.status);
    return s === 'PendingApproval' || s === '1';
  }

  /** Locked listing (published or awaiting approval): all wizard steps show complete. */
  get isLockedListing(): boolean {
    return this.isEdit && !this.canEditContent;
  }

  get isBrandRequired(): boolean {
    const ids = this.form.controls.categoryIds.value ?? [];
    if (!ids.length) return true;
    const selected = this.allCategories.filter((c) => ids.includes(c.productCategoryId));
    if (selected.length !== ids.length) return true;
    return selected.some((c) => !c.acceptsResume);
  }

  get isProfileProductForm(): boolean {
    return this.listLink === '/profile/product-listings';
  }

  get isResumeListing(): boolean {
    const ids = this.form.controls.categoryIds.value ?? [];
    if (!ids.length) return false;
    const selected = this.allCategories.filter((c) => ids.includes(c.productCategoryId));
    if (selected.length !== ids.length) return false;
    return selected.every((c) => !!c.acceptsResume);
  }

  get showBrandField(): boolean {
    return !(this.isProfileProductForm && this.isResumeListing);
  }

  get showOfferSection(): boolean {
    if (!(this.showDetails || this.isEdit)) return false;
    return !(this.isProfileProductForm && this.isResumeListing);
  }

  get wizardSteps(): { id: ProductWizardStep; labelKey: string }[] {
    const defs = this.showOfferSection
      ? this.allWizardSteps
      : this.allWizardSteps.filter((step) => step.id !== 'offer');
    return defs.map((step) => ({
      id: step.id,
      labelKey: this.productsKey(step.labelPath),
    }));
  }

  get currentStepIndex(): number {
    const index = this.wizardSteps.findIndex((step) => step.id === this.currentStepId);
    return index >= 0 ? index : 0;
  }

  get maxUnlockedIndex(): number {
    const index = this.wizardSteps.findIndex((step) => step.id === this.maxUnlockedStepId);
    if (index >= 0) return index;
    if (this.maxUnlockedStepId === 'offer' && !this.showOfferSection) {
      return Math.max(
        0,
        this.wizardSteps.findIndex((step) => step.id === 'location'),
      );
    }
    return Math.max(0, this.wizardSteps.length - 1);
  }

  get showWarrantyField(): boolean {
    return !(this.isProfileProductForm && this.isResumeListing);
  }

  get showExpertReviewsSection(): boolean {
    return !(this.isProfileProductForm && this.isResumeListing);
  }

  get brandOptions(): BaseFormSelectOption[] {
    return this.brands.map((brand) => ({
      value: brand.brandId,
      label: brand.title,
    }));
  }

  get categoryGroupOptions(): BaseFormSelectOption[] {
    return this.categoryGroups.map((group) => ({
      value: group.productCategoryGroupId,
      label: group.name,
    }));
  }

  get categoryOptions(): BaseFormSelectOption[] {
    const groupId = this.form.controls.productCategoryGroupId.value;
    if (!groupId) return [];
    return this.allCategories
      .filter((c) => c.productCategoryGroupId === groupId)
      .map((category) => ({
        value: category.productCategoryId,
        label: category.title,
      }));
  }

  get statusLabel(): string {
    return this._localization.translate(this.statusKey(this.status));
  }

  get previewTitle(): string {
    const title = this.form.controls.title.value?.trim();
    return title || this._localization.translate(this.productsKey('previewUntitled'));
  }

  get previewCategory(): string {
    const ids = this.form.controls.categoryIds.value ?? [];
    if (!ids.length) return this._localization.translate(this.productsKey('previewNoCategory'));
    return this.allCategories
      .filter((c) => ids.includes(c.productCategoryId))
      .map((c) => c.title)
      .join('، ');
  }

  get previewPrice(): string {
    if (this.form.controls.isNegotiablePrice.value) {
      return this._localization.translate(this.productsKey('fields.negotiablePrice'));
    }
    const price = this.form.controls.price.value ?? 0;
    const discount = this.form.controls.discountPercent.value ?? 0;
    const final = discount > 0 ? Math.round(price * (1 - discount / 100)) : price;
    return new Intl.NumberFormat('fa-IR').format(final);
  }

  get previewStock(): string {
    return new Intl.NumberFormat('fa-IR').format(this.form.controls.stockQuantity.value ?? 0);
  }

  get previewImageUrl(): string {
    if (this.featuredPreviewUrl) return this.featuredPreviewUrl;
    const first = this.galleryEntries[0];
    return first?.previewUrl ?? '';
  }

  get basicComplete(): boolean {
    const groupOk = this.form.controls.productCategoryGroupId.valid;
    const catsOk = this.form.controls.categoryIds.valid;
    const titleOk = this.form.controls.title.valid;
    const brandOk = !this.showBrandField || !this.isBrandRequired || this.form.controls.brandId.valid;
    return groupOk && catsOk && titleOk && brandOk;
  }

  get offerFieldsValid(): boolean {
    if (!this.showOfferSection) return true;
    const price = this.form.controls.price;
    // Disabled controls report valid=false; negotiable/inquiry mode must still pass.
    const priceOk = price.disabled || price.valid;
    return priceOk && this.form.controls.stockQuantity.valid;
  }

  get offerComplete(): boolean {
    if (!this.showOfferSection) return true;
    // Defaults must not show a green tick until the user has passed this step once.
    if (!this.visitedStepIds.has('offer')) return false;
    return this.offerFieldsValid;
  }

  get locationComplete(): boolean {
    if (!this.hasReachedStep('location')) return false;
    return this.form.controls.latitude.value != null && this.form.controls.longitude.value != null;
  }

  get specsComplete(): boolean {
    if (!this.hasReachedStep('specs')) return false;
    if (!this.showDetails) return false;
    const hasAttributeValue = this.attributes.some((attr) => {
      const raw = this.attributeControl(attr.productAttributeId).value;
      if (raw == null) return false;
      if (typeof raw === 'string') return raw.trim().length > 0;
      return true;
    });
    const hasReview = this.expertReviewForms.controls.some((group) => {
      const title = String(group.controls['title']?.value ?? '').trim();
      const description = String(group.controls['description']?.value ?? '').trim();
      return title.length > 0 || description.length > 0;
    });
    return hasAttributeValue || hasReview;
  }

  get mediaComplete(): boolean {
    if (!this.hasReachedStep('media')) return false;
    return !!this.featuredPreviewUrl || this.galleryEntries.length > 0;
  }

  get completionPercent(): number {
    if (this.isLockedListing) return 100;
    const parts = [
      this.basicComplete,
      ...(this.showOfferSection ? [this.offerComplete] : []),
      this.locationComplete,
      this.specsComplete,
      this.mediaComplete,
    ];
    return Math.round((parts.filter(Boolean).length / parts.length) * 100);
  }

  get currentStep(): ProductWizardStep {
    return this.wizardSteps[this.currentStepIndex]?.id ?? 'basic';
  }

  get isLastStep(): boolean {
    return this.currentStepIndex >= this.wizardSteps.length - 1;
  }

  /** Publish requires a prior successful save (persisted product id). */
  get canPublish(): boolean {
    return !!this.productId;
  }

  get nextStepLabelKey(): string {
    return this.wizardSteps[this.currentStepIndex + 1]?.labelKey ?? '';
  }

  get isDraftStatus(): boolean {
    if (this.status == null) return true;
    return String(this.status) === 'Draft';
  }

  get stepperProgressPercent(): number {
    if (this.isLockedListing) return 100;
    const last = this.wizardSteps.length - 1;
    if (last <= 0) return 0;
    // Keep fill at farthest unlocked progress (survives going back / opening a draft).
    const farthest = Math.max(this.currentStepIndex, this.maxUnlockedIndex);
    return Math.round((farthest / last) * 100);
  }

  isStepUnlocked(index: number): boolean {
    return index <= this.maxUnlockedIndex;
  }

  isStepContentComplete(index: number): boolean {
    if (this.isLockedListing) return true;
    switch (this.wizardSteps[index]?.id) {
      case 'basic':
        return this.basicComplete;
      case 'offer':
        return this.offerComplete;
      case 'location':
        return this.locationComplete;
      case 'specs':
        return this.specsComplete;
      case 'media':
        return this.mediaComplete;
      default:
        return false;
    }
  }

  isStepDone(index: number): boolean {
    // Current step stays green when its content is already complete (e.g. draft on step 1).
    if (index === this.currentStepIndex) {
      return this.isStepContentComplete(index);
    }
    // Steps already passed via «ادامه» — still require real content completion
    if (index < this.maxUnlockedIndex) {
      return this.isStepContentComplete(index);
    }
    // Draft / edit: unlocked steps with filled content keep completed (green) style
    if ((this.isDraftStatus || this.isEdit) && index <= this.maxUnlockedIndex) {
      return this.isStepContentComplete(index);
    }
    return false;
  }

  isStepCurrent(index: number): boolean {
    return index === this.currentStepIndex && !this.isStepDone(index);
  }

  isConnectorDone(index: number): boolean {
    return this.maxUnlockedIndex >= index;
  }

  /** Prefilled or saved data must not mark a step complete until the user opens or leaves it. */
  private hasReachedStep(stepId: ProductWizardStep): boolean {
    if (this.isLockedListing) return true;
    return this.currentStepId === stepId || this.visitedStepIds.has(stepId);
  }

  goToStep(index: number): void {
    if (index < 0 || index > this.maxUnlockedIndex) return;
    const step = this.wizardSteps[index];
    if (!step) return;
    this.currentStepId = step.id;
    this.scrollWizardToTop();
  }

  goToPreviousStep(): void {
    if (this.currentStepIndex === 0) return;
    const prev = this.wizardSteps[this.currentStepIndex - 1];
    if (!prev) return;
    this.currentStepId = prev.id;
    this.scrollWizardToTop();
  }

  continueToNextStep(): void {
    if (!this.validateCurrentStep()) {
      this.scrollToFirstError();
      return;
    }
    this.visitedStepIds.add(this.currentStep);
    if (this.isLastStep) {
      void this.submitForApproval();
      return;
    }
    const nextIndex = this.currentStepIndex + 1;
    const next = this.wizardSteps[nextIndex];
    if (!next) return;
    if (nextIndex > this.maxUnlockedIndex) {
      this.maxUnlockedStepId = next.id;
    }
    this.currentStepId = next.id;
    this.scrollWizardToTop();
  }

  private validateCurrentStep(): boolean {
    switch (this.currentStep) {
      case 'basic':
        this.form.controls.productCategoryGroupId.markAsTouched();
        this.form.controls.categoryIds.markAsTouched();
        this.form.controls.title.markAsTouched();
        this.form.controls.brandId.markAsTouched();
        return this.basicComplete;
      case 'offer':
        this.form.controls.price.markAsTouched();
        this.form.controls.stockQuantity.markAsTouched();
        return this.offerFieldsValid;
      case 'location':
      case 'specs':
      case 'media':
        return true;
      default:
        return false;
    }
  }

  private scrollWizardToTop(): void {
    this._host.nativeElement.querySelector('.product-stepper')?.scrollIntoView({
      behavior: 'smooth',
      block: 'start',
    });
  }

  openHistoryDialog(): void {
    if (!this.productId) return;

    this._dialog.open(ActivityLogHistoryDialogComponent, {
      width: '40rem',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      autoFocus: false,
      data: {
        entityName: 'Product',
        recordKey: this.productId,
      },
    });
  }

  ngOnInit(): void {
    this.listLink = this._route.snapshot.data['listLink'] ?? '/admin/products';
    if (this.listLink === '/profile/product-listings') {
      void this._portalLabAccess.ensure();
    }
    const id = this._route.snapshot.paramMap.get('id');
    if (this.isSiteAdmin && (!id || id === 'new')) {
      void this._router.navigateByUrl(this.listLink);
      return;
    }
    if (id && id !== 'new') {
      this.isEdit = true;
      this.productId = id;
    }
    this.form.controls.productCategoryGroupId.valueChanges.subscribe((groupId) => {
      const allowed = new Set(
        this.allCategories
          .filter((c) => c.productCategoryGroupId === groupId)
          .map((c) => c.productCategoryId),
      );
      const current = this.form.controls.categoryIds.value ?? [];
      this.form.controls.categoryIds.setValue(current.filter((id) => allowed.has(id)));
    });
    this.form.controls.categoryIds.valueChanges.subscribe((categoryIds) => {
      this.syncCategoryFormRules();
      void this.loadAttributes(categoryIds ?? []);
    });
    this.form.controls.isNegotiablePrice.valueChanges.subscribe(() => {
      this.syncOfferValidation();
    });
    void this.init();
  }

  attributeControl(attributeId: number): FormControl {
    if (!this.attributeControls[attributeId]) {
      this.attributeControls[attributeId] = new FormControl(null);
    }
    return this.attributeControls[attributeId];
  }

  isDateAttribute(attr: ProductAttributeDto): boolean {
    return isDateAttributeFieldType(attr.fieldType);
  }

  reviewControl(index: number, field: 'title' | 'description'): FormControl {
    return this.expertReviewForms.at(index).controls[field] as FormControl;
  }

  onLocationChange(location: { latitude: number; longitude: number }): void {
    this.form.patchValue({
      latitude: location.latitude,
      longitude: location.longitude,
    });
    void this.resolveProvince(location.latitude, location.longitude);
  }

  private async resolveProvince(latitude: number, longitude: number): Promise<void> {
    try {
      const result = await this._catalogService.resolveProvinceFromLocation(latitude, longitude);
      this.provinceName = result.success && result.data ? result.data.provinceName : '';
    } catch {
      this.provinceName = '';
    }
  }

  private async prefillLocationFromProfile(): Promise<void> {
    try {
      const profile = await this._profileService.getProfile();
      if (profile.latitude == null || profile.longitude == null) return;
      this.form.patchValue({
        latitude: profile.latitude,
        longitude: profile.longitude,
      });
      await this.resolveProvince(profile.latitude, profile.longitude);
    } catch {
      // Optional default from account location.
    }
  }

  async init(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const loaders: Promise<unknown>[] = [
        this._catalogService.getBrandOptions(),
        this._catalogService.getCategoryOptions(),
        this._catalogService.getCategoryGroupOptions(),
      ];

      const results = await Promise.all(loaders);
      const brandsResult = results[0] as Awaited<ReturnType<ProductCatalogService['getBrandOptions']>>;
      const categoriesResult = results[1] as Awaited<ReturnType<ProductCatalogService['getCategoryOptions']>>;
      const groupsResult = results[2] as Awaited<ReturnType<ProductCatalogService['getCategoryGroupOptions']>>;

      if (brandsResult.success && brandsResult.data) this.brands = brandsResult.data;
      if (categoriesResult.success && categoriesResult.data) this.allCategories = categoriesResult.data;
      if (groupsResult.success && groupsResult.data) this.categoryGroups = groupsResult.data;
      this.syncCategoryFormRules();

      if (this.isEdit && this.productId) {
        const productResult = await this._catalogService.getProductById(this.productId);
        if (!productResult.success || !productResult.data) {
          throw new Error(
            productResult.message ?? this._localization.translate(this.productsKey('errors.loadFailed')),
          );
        }
        const product = productResult.data;
        this.status = product.status;
        this.rejectionReason = product.rejectionReason ?? '';
        this.createdBySiteAdmin = product.createdBySiteAdmin !== false;
        this.featuredImagePath = product.featuredImagePath ?? '';
        this.form.patchValue({
          productCategoryGroupId: product.productCategoryGroupId ?? null,
          title: product.title,
          brandId: product.brandId,
          categoryIds: product.categoryIds,
          warranty: product.warranty,
          description: product.description,
          isNegotiablePrice: product.isNegotiablePrice ?? true,
          isUsed: product.isUsed ?? false,
          price: product.price ?? 0,
          stockQuantity: product.stockQuantity ?? 1,
          discountPercent: product.discountPercent ?? null,
          latitude: product.latitude ?? null,
          longitude: product.longitude ?? null,
        });
        this.provinceName = product.provinceName ?? '';
        this.syncExpertReviews(product.expertReviews);
        this.images = product.images;
        this.setGalleryFromServer(product.images);
        await this.loadAttributes(product.categoryIds);
        for (const av of product.attributeValues) {
          const displayedId = this.displayedAttributeIdFor(av.productAttributeId);
          if (displayedId == null) continue;
          const attr = this.attributes.find((a) => a.productAttributeId === displayedId);
          if (!attr) continue;
          const control = this.attributeControl(displayedId);
          const current = control.value;
          const hasCurrent =
            current != null &&
            !(typeof current === 'string' && current.trim() === '') &&
            !(jMoment.isMoment(current) && !current.isValid());
          if (hasCurrent) continue;
          if (this.isDateAttribute(attr)) {
            control.setValue(this.parseAttributeDate(av.value));
          } else {
            control.setValue(av.value ?? '');
          }
        }
        if (!this.canEditContent) {
          this.form.disable({ emitEvent: false });
        } else {
          this.syncOfferValidation();
        }
        this.maxUnlockedStepId = this.wizardSteps[this.wizardSteps.length - 1]?.id ?? 'media';
        if (this.isProfileProductForm) {
          this.currentStepId = this.maxUnlockedStepId;
        }
        for (const step of this.wizardSteps) {
          this.visitedStepIds.add(step.id);
        }
        this.ensureValidWizardStep();
      } else {
        await this.prefillLocationFromProfile();
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate(this.productsKey('errors.loadFailed'));
    } finally {
      this.loading = false;
    }
  }

  private syncCategoryFormRules(): void {
    this.syncBrandValidation();
    this.syncOfferValidation();

    if (this.isProfileProductForm && this.isResumeListing) {
      this.form.controls.brandId.setValue(null, { emitEvent: false });
      this.form.controls.warranty.setValue('', { emitEvent: false });
      this.clearExpertReviews();
    }

    this.ensureValidWizardStep();
  }

  private ensureValidWizardStep(): void {
    const visible = this.wizardSteps;
    if (!visible.some((step) => step.id === this.currentStepId)) {
      this.currentStepId =
        this.currentStepId === 'offer' ? 'location' : (visible[0]?.id ?? 'basic');
    }
    if (!visible.some((step) => step.id === this.maxUnlockedStepId)) {
      this.maxUnlockedStepId =
        this.maxUnlockedStepId === 'offer'
          ? 'location'
          : (visible[visible.length - 1]?.id ?? 'basic');
    }
    // Keep unlock at least on the current visible step
    const currentIdx = visible.findIndex((step) => step.id === this.currentStepId);
    const unlockedIdx = visible.findIndex((step) => step.id === this.maxUnlockedStepId);
    if (currentIdx > unlockedIdx) {
      this.maxUnlockedStepId = this.currentStepId;
    }
  }

  private clearExpertReviews(): void {
    this.expertReviews = [];
    this.expertReviewForms.clear();
  }

  private syncBrandValidation(): void {
    const brand = this.form.controls.brandId;
    if (this.isBrandRequired) {
      brand.setValidators(Validators.required);
    } else {
      brand.clearValidators();
    }
    brand.updateValueAndValidity({ emitEvent: false });
  }

  private syncOfferValidation(): void {
    const stock = this.form.controls.stockQuantity;
    const price = this.form.controls.price;
    const hideOffer = this.isProfileProductForm && this.isResumeListing;

    if (hideOffer) {
      stock.setValidators([Validators.min(0)]);
      price.setValidators([Validators.min(0)]);
    } else {
      stock.setValidators([Validators.required, Validators.min(0)]);
      const negotiable = this.form.controls.isNegotiablePrice.value;
      if (negotiable) {
        price.setValidators([Validators.min(0)]);
      } else {
        price.setValidators([Validators.required, Validators.min(0.01)]);
      }
    }

    stock.updateValueAndValidity({ emitEvent: false });
    price.updateValueAndValidity({ emitEvent: false });
    this.syncPriceEnabledState();
  }

  private syncPriceEnabledState(): void {
    const price = this.form.controls.price;
    if (!this.canEditContent) {
      price.disable({ emitEvent: false });
      return;
    }

    const hideOffer = this.isProfileProductForm && this.isResumeListing;
    const negotiable = !!this.form.controls.isNegotiablePrice.value;
    if (!hideOffer && negotiable) {
      price.disable({ emitEvent: false });
    } else {
      price.enable({ emitEvent: false });
    }
  }

  async loadAttributes(categoryIds: number[]): Promise<void> {
    if (!categoryIds.length) {
      this.attributes = [];
      this.attributeAliasIds = {};
      this.attributeControls = {};
      return;
    }

    const result = await this._catalogService.getAttributesByCategoryIds({ categoryIds });
    if (result.success && result.data) {
      const { attributes, aliasIds } = this.dedupeAttributes(result.data);
      const nextControls: Record<number, FormControl> = {};
      for (const attr of attributes) {
        const existing = this.attributeControls[attr.productAttributeId];
        nextControls[attr.productAttributeId] =
          existing ?? new FormControl(this.isDateAttribute(attr) ? null : '');
      }
      this.attributeControls = nextControls;
      this.attributeAliasIds = aliasIds;
      this.attributes = attributes;
    }
  }

  /** Keep one field per title (+ field type) when multiple categories share the same attribute. */
  private dedupeAttributes(items: ProductAttributeDto[]): {
    attributes: ProductAttributeDto[];
    aliasIds: Record<number, number[]>;
  } {
    const byKey = new Map<string, ProductAttributeDto>();
    const aliasIds: Record<number, number[]> = {};

    for (const attr of items) {
      const key = this.attributeDedupeKey(attr);
      const existing = byKey.get(key);
      if (!existing) {
        byKey.set(key, attr);
        aliasIds[attr.productAttributeId] = [attr.productAttributeId];
        continue;
      }
      aliasIds[existing.productAttributeId] = [
        ...(aliasIds[existing.productAttributeId] ?? [existing.productAttributeId]),
        attr.productAttributeId,
      ];
    }

    return {
      attributes: [...byKey.values()].sort((a, b) =>
        (a.title ?? '').localeCompare(b.title ?? '', 'fa', { sensitivity: 'base', numeric: true }),
      ),
      aliasIds,
    };
  }

  private attributeDedupeKey(attr: ProductAttributeDto): string {
    const title = (attr.title ?? '').trim().replace(/\s+/g, ' ').toLocaleLowerCase('fa');
    const fieldType = attr.fieldType == null ? '' : String(attr.fieldType);
    return `${title}::${fieldType}`;
  }

  private displayedAttributeIdFor(attributeId: number): number | null {
    if (this.attributeControls[attributeId]) return attributeId;
    for (const [canonicalId, aliases] of Object.entries(this.attributeAliasIds)) {
      if (aliases.includes(attributeId)) return Number(canonicalId);
    }
    return null;
  }

  private parseAttributeDate(value: string | null | undefined): Moment | null {
    if (!value?.trim()) return null;
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.clone() : null;
  }

  private buildAttributeDateIso(date: Moment | null | undefined): string {
    if (!date || !jMoment.isMoment(date) || !date.isValid()) return '';
    return date.clone().startOf('day').toISOString();
  }

  private attributeValueForSave(attr: ProductAttributeDto): string {
    const raw = this.attributeControl(attr.productAttributeId).value;
    if (this.isDateAttribute(attr)) {
      return this.buildAttributeDateIso(raw as Moment | null);
    }
    return typeof raw === 'string' ? raw.trim() : String(raw ?? '').trim();
  }

  private syncExpertReviews(reviews: ProductExpertReviewDto[]): void {
    this.expertReviews = reviews.map((review) => ({ ...review }));
    this.expertReviewForms.clear();
    for (const review of this.expertReviews) {
      this.expertReviewForms.push(this.createReviewGroup(review));
    }
  }

  private createReviewGroup(review?: Partial<ProductExpertReviewDto>): FormGroup {
    return new FormGroup({
      title: new FormControl(review?.title ?? ''),
      description: new FormControl(review?.description ?? ''),
      sortOrder: new FormControl(review?.sortOrder ?? this.expertReviewForms.length),
    });
  }

  async openAddExpertReviewDialog(): Promise<void> {
    if (!this.canEditContent) return;

    const ref = this._dialog.open<
      ExpertReviewFormDialogComponent,
      null,
      ExpertReviewFormDialogResult | null
    >(ExpertReviewFormDialogComponent, {
      width: '28rem',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      autoFocus: false,
      data: null,
    });

    const result = await firstValueFrom(ref.afterClosed());
    if (!result) return;

    const review: ProductExpertReviewDto = {
      title: result.title,
      description: result.description,
      sortOrder: this.expertReviews.length,
    };
    this.expertReviews.push(review);
    this.expertReviewForms.push(this.createReviewGroup(review));
  }

  removeExpertReview(index: number): void {
    if (!this.canEditContent) return;
    this.expertReviews.splice(index, 1);
    this.expertReviewForms.removeAt(index);
    this.expertReviewForms.controls.forEach((group, i) => {
      group.controls['sortOrder'].setValue(i);
    });
  }

  getImageUrl(imagePath: string): string {
    return this._catalogService.getProductImageUrl(imagePath);
  }

  ngOnDestroy(): void {
    this.revokePendingMediaUrls();
  }

  async onImageSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []).filter((file) => file.type.startsWith('image/'));
    input.value = '';
    this.stageGalleryFiles(files);
  }

  private stageGalleryFiles(files: File[]): void {
    if (!this.canUploadMedia || !files.length) return;
    for (const file of files) {
      this.galleryEntries.push({
        id: `local-${Date.now()}-${Math.random().toString(36).slice(2, 9)}`,
        file,
        previewUrl: URL.createObjectURL(file),
      });
    }
  }

  private setGalleryFromServer(images: ProductImageDto[]): void {
    const pending = this.galleryEntries.filter((entry) => !!entry.file);
    this.galleryEntries = [
      ...images.map((image) => ({
        id: image.id,
        previewUrl: this.getImageUrl(image.imagePath),
        serverImage: image,
      })),
      ...pending,
    ];
  }

  async deleteImage(entry: ProductGalleryEntry): Promise<void> {
    if (!this.canEditContent) return;

    if (entry.file) {
      this.revokeObjectUrl(entry.previewUrl);
      this.galleryEntries = this.galleryEntries.filter((item) => item.id !== entry.id);
      return;
    }

    if (!this.productId || !entry.serverImage) return;
    const confirmed = await this.confirmDeleteImage();
    if (!confirmed) return;

    const result = await this._catalogService.deleteImage({
      productId: this.productId,
      imageId: entry.serverImage.id,
    });
    if (!result.success) {
      this.error = result.message ?? this._localization.translate(this.productsKey('errors.deleteImageFailed'));
      return;
    }
    this.images = this.images.filter((i) => i.id !== entry.serverImage!.id);
    this.galleryEntries = this.galleryEntries.filter((item) => item.id !== entry.id);
  }

  async onFeaturedSelected(file: File): Promise<void> {
    if (!this.canUploadMedia) return;
    this.revokeObjectUrl(this.pendingFeaturedPreviewUrl);
    this.pendingFeaturedFile = file;
    this.pendingFeaturedPreviewUrl = URL.createObjectURL(file);
  }

  async removeFeaturedImage(): Promise<void> {
    if (!this.canEditContent) return;

    if (this.pendingFeaturedFile || this.pendingFeaturedPreviewUrl) {
      this.revokeObjectUrl(this.pendingFeaturedPreviewUrl);
      this.pendingFeaturedFile = null;
      this.pendingFeaturedPreviewUrl = '';
      return;
    }

    if (!this.productId || !this.featuredImagePath) return;
    const confirmed = await this.confirmDeleteImage();
    if (!confirmed) return;

    this.uploadingFeatured = true;
    const result = await this._catalogService.deleteFeaturedImage(this.productId);
    if (result.success) {
      this.featuredImagePath = '';
    } else {
      this.error = result.message ?? this._localization.translate(this.productsKey('errors.deleteImageFailed'));
    }
    this.uploadingFeatured = false;
  }

  private async confirmDeleteImage(): Promise<boolean> {
    const ref = this._dialog.open(BaseConfirmDialogComponent, {
      width: '400px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        title: this._localization.translate(this.productsKey('deleteImageTitle')),
        message: this._localization.translate(this.productsKey('confirmDeleteImage')),
        confirmLabel: this._localization.translate('shared.delete'),
        warnConfirm: true,
      },
    });
    return (await firstValueFrom(ref.afterClosed())) === true;
  }

  cancel(): void {
    if (this.isSiteAdmin && this.productId) {
      void this._router.navigate(['/admin/products', this.productId]);
      return;
    }
    void this._router.navigate([this.listLink], { queryParamsHandling: 'preserve' });
  }

  onGalleryDragOver(event: DragEvent): void {
    event.preventDefault();
    if (!this.canUploadMedia || this.uploading) return;
    this.galleryDragging = true;
  }

  onGalleryDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.galleryDragging = false;
  }

  async onGalleryDropped(event: DragEvent): Promise<void> {
    event.preventDefault();
    this.galleryDragging = false;
    if (!this.canUploadMedia || this.uploading) return;
    const files = Array.from(event.dataTransfer?.files ?? []).filter((file) =>
      file.type.startsWith('image/'),
    );
    this.stageGalleryFiles(files);
  }

  moveImage(index: number, direction: -1 | 1): void {
    const target = index + direction;
    if (target < 0 || target >= this.galleryEntries.length) return;
    const next = [...this.galleryEntries];
    const [item] = next.splice(index, 1);
    next.splice(target, 0, item);
    this.galleryEntries = next;
  }

  private async flushPendingMedia(): Promise<boolean> {
    if (!this.productId) return true;

    this.error = '';
    try {
      if (this.pendingFeaturedFile) {
        this.uploadingFeatured = true;
        const result = await this._catalogService.uploadFeaturedImage(
          this.productId,
          this.pendingFeaturedFile,
        );
        this.uploadingFeatured = false;
        if (!result.success || !result.data) {
          this.error =
            result.message ?? this._localization.translate(this.productsKey('errors.uploadFailed'));
          return false;
        }
        this.featuredImagePath = result.data.featuredImagePath ?? '';
        this.revokeObjectUrl(this.pendingFeaturedPreviewUrl);
        this.pendingFeaturedFile = null;
        this.pendingFeaturedPreviewUrl = '';
      }

      const pending = this.galleryEntries.filter((entry) => !!entry.file);
      if (pending.length) {
        this.uploading = true;
        for (const entry of pending) {
          const result = await this._catalogService.uploadImage(this.productId, entry.file!);
          if (!result.success || !result.data) {
            this.error =
              result.message ?? this._localization.translate(this.productsKey('errors.uploadFailed'));
            this.uploading = false;
            return false;
          }
          this.images.push(result.data);
          this.revokeObjectUrl(entry.previewUrl);
          const idx = this.galleryEntries.findIndex((item) => item.id === entry.id);
          if (idx >= 0) {
            this.galleryEntries[idx] = {
              id: result.data.id,
              previewUrl: this.getImageUrl(result.data.imagePath),
              serverImage: result.data,
            };
          }
        }
        this.uploading = false;
        this.galleryEntries = [...this.galleryEntries];
      }

      return true;
    } catch {
      this.uploading = false;
      this.uploadingFeatured = false;
      this.error = this._localization.translate(this.productsKey('errors.uploadFailed'));
      return false;
    }
  }

  private revokePendingMediaUrls(): void {
    this.revokeObjectUrl(this.pendingFeaturedPreviewUrl);
    for (const entry of this.galleryEntries) {
      if (entry.file) this.revokeObjectUrl(entry.previewUrl);
    }
  }

  private revokeObjectUrl(url: string): void {
    if (url?.startsWith('blob:')) {
      URL.revokeObjectURL(url);
    }
  }

  async saveDraft(): Promise<void> {
    await this.persist(true);
  }

  async submitForApproval(): Promise<void> {
    if (!this.canPublish) {
      this.error = this._localization.translate(this.productsKey('publishRequiresSave'));
      return;
    }

    const confirmed = await this.confirmPublish();
    if (!confirmed) return;

    this.submitting = true;
    this.error = '';
    try {
      let result;
      if (this.isSiteAdmin) {
        result = await this._catalogService.approveProduct(this.productId!);
      } else if (this.isUnpublished) {
        result = await this._catalogService.republishProduct(this.productId!);
      } else {
        result = await this._catalogService.submitForApproval(this.productId!);
      }
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate(this.productsKey('errors.saveFailed')));
      }
      const listingSubmitted = !this.isSiteAdmin && !this.isUnpublished;
      this.status = result.data.status;
      await this._router.navigate([this.listLink], {
        queryParamsHandling: 'preserve',
        state: listingSubmitted ? { listingSubmitted: true } : undefined,
      });
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate(this.productsKey('errors.saveFailed'));
    } finally {
      this.submitting = false;
    }
  }

  private async confirmPublish(): Promise<boolean> {
    const ref = this._dialog.open(BaseConfirmDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        title: this._localization.translate(this.productsKey('confirmPublishTitle')),
        message: this._localization.translate(this.productsKey('confirmPublishMessage')),
        confirmLabel: this._localization.translate(this.productsKey('confirmPublishConfirm')),
        cancelLabel: this._localization.translate('shared.cancel'),
        warnConfirm: false,
      },
    });
    return (await firstValueFrom(ref.afterClosed())) === true;
  }

  async unpublish(): Promise<void> {
    if (!this.productId || !this.isApproved) return;
    this.submitting = true;
    this.error = '';
    try {
      const result = await this._catalogService.unpublishProduct(this.productId);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate(this.productsKey('errors.unpublishFailed')),
        );
      }
      this.status = result.data.status;
      if (this.canEditContent && this.form.disabled) {
        this.form.enable({ emitEvent: false });
        this.syncOfferValidation();
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(this.productsKey('errors.unpublishFailed'));
    } finally {
      this.submitting = false;
    }
  }

  async approve(): Promise<void> {
    if (!this.productId || !this.isSiteAdmin) return;
    this.submitting = true;
    const result = await this._catalogService.approveProduct(this.productId);
    if (result.success && result.data) {
      this.status = result.data.status;
    } else {
      this.error = result.message ?? this._localization.translate(this.productsKey('errors.approveFailed'));
    }
    this.submitting = false;
  }

  async reject(): Promise<void> {
    if (!this.productId || !this.isSiteAdmin) return;
    const ref = this._dialog.open(ProductRejectDialogComponent, {
      width: '440px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
    });
    const reason = await firstValueFrom(ref.afterClosed());
    if (!reason) return;

    this.submitting = true;
    const result = await this._catalogService.rejectProduct({
      productId: this.productId,
      rejectionReason: reason,
    });
    if (result.success && result.data) {
      this.status = result.data.status;
      this.rejectionReason = result.data.rejectionReason ?? '';
    } else {
      this.error = result.message ?? this._localization.translate(this.productsKey('errors.rejectFailed'));
    }
    this.submitting = false;
  }

  private scrollToFirstError(): void {
    queueMicrotask(() => {
      const invalid = this._host.nativeElement.querySelector('.ng-invalid');
      if (invalid instanceof HTMLElement) {
        invalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
    });
  }

  private async persist(stayOnPage: boolean): Promise<boolean> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.scrollToFirstError();
      return false;
    }
    if (!this.canEditContent) return false;

    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();
    const attributeValues = this.attributes.flatMap((attr) => {
      const value = this.attributeValueForSave(attr);
      const ids = this.attributeAliasIds[attr.productAttributeId] ?? [attr.productAttributeId];
      return ids.map((productAttributeId) => ({ productAttributeId, value }));
    });
    const expertReviews = this.expertReviewForms.controls.map((group, i) => ({
      ...this.expertReviews[i],
      title: group.controls['title'].value ?? '',
      description: group.controls['description'].value ?? '',
      sortOrder: i,
    }));

    const payload = {
      title: value.title ?? '',
      brandId: value.brandId ?? null,
      warranty: value.warranty ?? '',
      description: value.description ?? '',
      productCategoryGroupId: value.productCategoryGroupId!,
      price: value.price ?? 0,
      isNegotiablePrice: value.isNegotiablePrice ?? true,
      isUsed: value.isUsed ?? false,
      stockQuantity: value.stockQuantity ?? 0,
      discountPercent: value.discountPercent,
      latitude: value.latitude,
      longitude: value.longitude,
      categoryIds: value.categoryIds ?? [],
      attributeValues,
      expertReviews,
    };

    try {
      if (this.isEdit && this.productId) {
        const result = await this._catalogService.updateProduct({
          productId: this.productId,
          ...payload,
        });
        if (!result.success || !result.data) {
          throw new Error(result.message ?? this._localization.translate(this.productsKey('errors.saveFailed')));
        }
        this.status = result.data.status;
        this.provinceName = result.data.provinceName ?? '';
        if (!this.canEditContent) {
          this.form.disable({ emitEvent: false });
        }
        const mediaOk = await this.flushPendingMedia();
        return mediaOk;
      }

      const result = await this._catalogService.createProduct(payload);
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate(this.productsKey('errors.saveFailed')));
      }
      this.productId = result.data.productId;
      this.isEdit = true;
      this.status = result.data.status;
      this.provinceName = result.data.provinceName ?? '';
      const mediaOk = await this.flushPendingMedia();
      if (!mediaOk) return false;
      // Keep wizard on the current (last) step so Publish stays available.
      this.maxUnlockedStepId = this.wizardSteps[this.wizardSteps.length - 1]?.id ?? 'media';
      if (stayOnPage) {
        this._location.replaceState(`${this.listLink}/${result.data.productId}`);
      } else {
        void this._router.navigate([this.listLink, result.data.productId]);
      }
      return true;
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate(this.productsKey('errors.saveFailed'));
      return false;
    } finally {
      this.saving = false;
    }
  }

  private statusKey(status: ProductStatus | null): string {
    switch (status) {
      case 'PendingApproval':
      case 1:
        return this.productsKey('status.pendingApproval');
      case 'Approved':
      case 2:
        return this.productsKey('status.approved');
      case 'Rejected':
      case 3:
        return this.productsKey('status.rejected');
      case 'Unpublished':
      case 4:
        return this.productsKey('status.unpublished');
      default:
        return this.productsKey('status.draft');
    }
  }
}
