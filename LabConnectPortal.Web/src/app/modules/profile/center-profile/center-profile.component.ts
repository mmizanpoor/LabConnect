import { Component, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { NgClass } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseConfirmDialogComponent } from '@modules/base/components/base-dialog/base-confirm-dialog.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { CenterProfileService } from './center-profile.service';
import {
  CenterProfileDto,
  CenterProfileFileKind,
  EMPLOYEE_COUNT_RANGES,
} from './center-profile.types';
import {
  CenterProfileIncompleteDialogComponent,
  CenterProfileIncompleteItem,
} from './center-profile-incomplete-dialog.component';
import { LocationMapPickerComponent } from '../location-map-picker/location-map-picker.component';
import {
  UsernameChangeChannel,
  UsernameChangeChannelDialogComponent,
} from '../account-security/username-change-channel-dialog.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

type CenterProfileTab = 'basic' | 'location' | 'documents';

interface CenterProfileTabItem {
  id: CenterProfileTab;
  labelKey: string;
  icon: string;
}

@Component({
  selector: 'app-center-profile',
  standalone: true,
  imports: [
    NgClass,
    // DecimalPipe,
    ReactiveFormsModule,
    MatDialogModule,
    TranslocoPipe,
    MatIconModule,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseImageUploadComponent,
    LocationMapPickerComponent,
  ],
  templateUrl: './center-profile.component.html',
  styleUrl: './center-profile.component.scss',
})
export class CenterProfileComponent implements OnInit, OnDestroy {
  readonly entity = SystemEntity.CenterProfile;
  private readonly _systemEntityBinding = bindSystemEntity(
    SystemEntity.CenterProfile
  );

  private _route = inject(ActivatedRoute);
  private _router = inject(Router);
  private _dialog = inject(MatDialog);
  viewMode = this._route.snapshot.data['viewMode'] === true;

  loading = false;
  saving = false;
  uploading: CenterProfileFileKind | null = null;
  error = '';
  success = '';
  profile: CenterProfileDto | null = null;
  employeeCountRanges = EMPLOYEE_COUNT_RANGES;
  activeTab: CenterProfileTab = 'basic';
  highlightMissing = false;

  contactOtpPending = false;
  contactOtpChannel: UsernameChangeChannel | null = null;
  sendingContactOtp = false;
  contactOtpCountdown = 0;
  private _contactOtpTimer: ReturnType<typeof setInterval> | null = null;
  private _originalMobile = '';
  private _originalEmail = '';

  contactOtpControl = new FormControl('', [
    Validators.minLength(6),
    Validators.maxLength(6),
  ]);

  readonly tabs: CenterProfileTabItem[] = [
    {
      id: 'basic',
      labelKey: 'modules.profile.center.tabs.basic',
      icon: 'storefront',
    },
    {
      id: 'documents',
      labelKey: 'modules.profile.center.tabs.documents',
      icon: 'folder_open',
    },
    {
      id: 'location',
      labelKey: 'modules.profile.center.tabs.location',
      icon: 'location_on',
    },
  ];

  get employeeCountOptions(): BaseFormSelectOption[] {
    return [
      { value: null, label: '—' },
      ...this.employeeCountRanges.map((range) => ({
        value: range,
        label: this._localization.translate(`enum.employeeCountRange.${range}`),
      })),
    ];
  }

  logoUrl = '';
  nationalCardUrl = '';
  licenseUrl = '';
  officialImageUrl = '';
  tradeCardUrl = '';
  logoFileName = '';
  nationalCardFileName = '';
  licenseFileName = '';
  officialImageFileName = '';
  tradeCardFileName = '';

  form = new FormGroup({
    name: new FormControl('', { validators: [Validators.required] }),
    address: new FormControl('', { validators: [Validators.required] }),
    phone: new FormControl('', { validators: [Validators.required] }),
    mobileNumber: new FormControl(''),
    email: new FormControl(''),
    latitude: new FormControl<number | null>(null),
    longitude: new FormControl<number | null>(null),
    establishedYear: new FormControl<string | number | null>(null, {
      validators: [
        (control) => {
          const raw = control.value;
          if (raw === null || raw === undefined || raw === '') return null;
          return /^\d{4}$/.test(String(raw).trim())
            ? null
            : { establishedYearDigits: true };
        },
      ],
    }),
    description: new FormControl(''),
    website: new FormControl(''),
    employeeCount: new FormControl<string | null>(null),
    economicCode: new FormControl('', { validators: [Validators.required] }),
    registrationNumber: new FormControl('', {
      validators: [Validators.required],
    }),
  });

  constructor(
    private _centerProfileService: CenterProfileService,
    private _localization: LocalizationService,
    private _labPermission: LabPermissionService
  ) {}

  get isLab(): boolean {
    return this.profile?.centerType === 'Lab';
  }

  get showCompanyDocuments(): boolean {
    return !this.isLab;
  }

  get canEdit(): boolean {
    return this._labPermission.can(this.entity, 'update');
  }

  get readOnly(): boolean {
    return this.viewMode || !this.canEdit;
  }

  get hasLogoFile(): boolean {
    return !!(this.logoUrl || this.profile?.hasLogo);
  }

  get hasNationalCardFile(): boolean {
    return !!(this.nationalCardUrl || this.profile?.hasNationalCard);
  }

  get hasLicenseFile(): boolean {
    return !!(this.licenseUrl || this.profile?.hasLicense);
  }

  get hasOfficialImageFile(): boolean {
    return !!(this.officialImageUrl || this.profile?.hasOfficialImage);
  }

  get hasTradeCardFile(): boolean {
    return !!(this.tradeCardUrl || this.profile?.hasTradeCard);
  }

  goToEdit(): void {
    void this._router.navigate(['/profile/center'], {
      queryParamsHandling: 'preserve',
    });
  }

  goToSmsCharge(): void {
    void this._router.navigate(['/profile/center/sms-charge'], {
      queryParamsHandling: 'preserve',
    });
  }

  setTab(tab: CenterProfileTab): void {
    this.activeTab = tab;
    if (tab === 'location') {
      setTimeout(() => this._locationPicker?.refreshMapSize(), 0);
    }
  }

  private _locationPicker?: LocationMapPickerComponent;

  @ViewChild(LocationMapPickerComponent)
  set locationPickerRef(ref: LocationMapPickerComponent | undefined) {
    this._locationPicker = ref;
    if (ref && this.activeTab === 'location') {
      ref.refreshMapSize();
    }
  }

  onLocationChange(location: { latitude: number; longitude: number }): void {
    this.form.patchValue({
      latitude: location.latitude,
      longitude: location.longitude,
    });
  }

  private async confirmApprovedEdit(): Promise<boolean> {
    if (this.isLab || !this.profile?.isApproved) return true;

    const confirmed = await firstValueFrom(
      this._dialog
        .open(BaseConfirmDialogComponent, {
          width: '26.5rem',
          maxWidth: '95vw',
          panelClass: BASE_DIALOG_PANEL_CLASS,
          data: {
            title: this._localization.translate(
              'modules.profile.center.confirmEditApprovedTitle'
            ),
            message: this._localization.translate(
              'modules.profile.center.confirmEditApproved'
            ),
            confirmLabel: this._localization.translate(
              'modules.profile.center.confirmEditApprovedAction'
            ),
            cancelLabel: this._localization.translate('shared.cancel'),
            warnConfirm: true,
            premium: true,
            icon: 'policy',
          },
        })
        .afterClosed()
    );

    return confirmed === true;
  }

  async ngOnInit(): Promise<void> {
    await this.load();
  }

  ngOnDestroy(): void {
    this.clearContactOtpCountdown();
    this.revokeUrls();
  }

  async load(preserveFormFields = false): Promise<void> {
    if (!preserveFormFields) {
      this.loading = true;
    }
    this.error = '';
    try {
      const result = await this._centerProfileService.getMyProfile();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.profile.center.errors.loadFailed'
            )
        );
      }
      this.applyProfileData(result.data, preserveFormFields);
      try {
        await this.loadPreviews();
      } catch {
        // Keep the profile form visible if a document preview cannot be loaded.
      }
      if (
        !preserveFormFields &&
        !this.readOnly &&
        this.statusClass() === 'is-incomplete'
      ) {
        this.highlightIncompleteFields();
      }
    } catch (e: unknown) {
      this.error = this.readErrorMessage(
        e,
        'modules.profile.center.errors.loadFailed'
      );
    } finally {
      if (!preserveFormFields) {
        this.loading = false;
      }
    }
  }

  private applyProfileData(
    data: CenterProfileDto,
    preserveFormFields: boolean
  ): void {
    const current = preserveFormFields ? this.form.getRawValue() : null;
    this.profile = data;
    if (!preserveFormFields) {
      this._originalMobile = (data.mobileNumber ?? '').trim();
      this._originalEmail = (data.email ?? '').trim();
      this.resetContactOtpState();
    }
    this.form.patchValue({
      name: data.name,
      address: data.address,
      phone: data.phone,
      mobileNumber: data.mobileNumber ?? '',
      email: data.email ?? '',
      latitude: data.latitude ?? null,
      longitude: data.longitude ?? null,
      establishedYear: data.establishedYear ?? null,
      description: data.description ?? '',
      website: data.website ?? '',
      employeeCount: data.employeeCount ?? null,
      economicCode: data.economicCode ?? '',
      registrationNumber: data.registrationNumber ?? '',
    });
    if (current) {
      this.form.patchValue({
        name: current.name || data.name,
        address: current.address || data.address,
        phone: current.phone || data.phone,
        mobileNumber: current.mobileNumber || data.mobileNumber || '',
        email: current.email || data.email || '',
        latitude: current.latitude ?? data.latitude ?? null,
        longitude: current.longitude ?? data.longitude ?? null,
        establishedYear:
          current.establishedYear ?? data.establishedYear ?? null,
        description: current.description || data.description || '',
        website: current.website || data.website || '',
        employeeCount: current.employeeCount || data.employeeCount || null,
        economicCode: current.economicCode || data.economicCode || '',
        registrationNumber:
          current.registrationNumber || data.registrationNumber || '',
      });
    }
    if (this.readOnly) {
      this.form.disable();
    } else {
      this.form.enable();
    }
    if (!preserveFormFields) {
      this.highlightMissing = false;
      this.form.markAsUntouched();
      this.form.markAsPristine();
    }
  }

  getMissingItems(): CenterProfileIncompleteItem[] {
    const value = this.form.getRawValue();
    const basicTab = 'modules.profile.center.tabs.basic';
    const docsTab = 'modules.profile.center.tabs.documents';
    const items: CenterProfileIncompleteItem[] = [];

    const pushIfEmpty = (
      raw: string | null | undefined,
      labelKey: string,
      tabLabelKey: string
    ): void => {
      if (!raw?.trim()) {
        items.push({ labelKey, tabLabelKey });
      }
    };

    pushIfEmpty(value.name, 'modules.profile.center.name', basicTab);
    pushIfEmpty(value.address, 'modules.profile.center.address', basicTab);
    pushIfEmpty(value.phone, 'modules.profile.center.phone', basicTab);
    pushIfEmpty(
      value.economicCode,
      'modules.profile.center.economicCode',
      basicTab
    );
    pushIfEmpty(
      value.registrationNumber,
      'modules.profile.center.registrationNumber',
      basicTab
    );

    if (!this.hasLogoFile) {
      items.push({
        labelKey: 'modules.profile.center.logo',
        tabLabelKey: basicTab,
      });
    }
    if (!this.hasNationalCardFile) {
      items.push({
        labelKey: 'modules.profile.center.nationalCard',
        tabLabelKey: docsTab,
      });
    }
    if (!this.hasLicenseFile) {
      items.push({
        labelKey: 'modules.profile.center.license',
        tabLabelKey: docsTab,
      });
    }

    return items;
  }

  openIncompleteDialog(): void {
    if (this.statusClass() !== 'is-incomplete') return;

    const items = this.getMissingItems();
    this._dialog.open(CenterProfileIncompleteDialogComponent, {
      width: '42rem',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        title: this._localization.translate(
          'modules.profile.center.incompleteDialog.title'
        ),
        items,
      },
    });
  }

  private highlightIncompleteFields(): void {
    this.highlightMissing = true;
    this.form.controls.name.markAsTouched();
    this.form.controls.address.markAsTouched();
    this.form.controls.phone.markAsTouched();
    this.form.controls.economicCode.markAsTouched();
    this.form.controls.registrationNumber.markAsTouched();
    this.form.controls.establishedYear.markAsTouched();
    this.form.updateValueAndValidity({ emitEvent: false });
  }

  /** Clears incomplete year and returns null; keeps valid 4-digit years. */
  private normalizeEstablishedYearOnSave(): number | null {
    const control = this.form.controls.establishedYear;
    const raw = control.value;
    if (raw === null || raw === undefined || raw === '') {
      return null;
    }
    const text = String(raw).trim();
    if (!/^\d{4}$/.test(text)) {
      control.setValue(null, { emitEvent: false });
      control.markAsTouched();
      control.setErrors({ establishedYearDigits: true });
      return null;
    }
    return Number(text);
  }

  async save(): Promise<void> {
    if (this.readOnly) return;
    if (!(await this.confirmApprovedEdit())) return;

    this.highlightIncompleteFields();
    const yearRaw = this.form.controls.establishedYear.value;
    const yearText =
      yearRaw === null || yearRaw === undefined || yearRaw === ''
        ? ''
        : String(yearRaw).trim();
    const yearClearedForDigits =
      yearText.length > 0 && !/^\d{4}$/.test(yearText);
    const establishedYear = this.normalizeEstablishedYearOnSave();
    this.error = '';
    this.success = '';
    const wasApproved = this.profile?.isApproved ?? false;
    const value = this.form.getRawValue();
    const mobile = (value.mobileNumber ?? '').trim();
    const email = (value.email ?? '').trim();
    const contactChanged = this.isContactChanged(mobile, email);

    if (contactChanged && !this.contactOtpPending) {
      await this.beginContactOtpFlow();
      return;
    }

    if (contactChanged) {
      const code = this.contactOtpControl.value?.trim() ?? '';
      if (code.length !== 6 || !this.contactOtpChannel) {
        this.contactOtpControl.setErrors({ required: true });
        this.contactOtpControl.markAsTouched();
        this.error = this._localization.translate(
          'modules.profile.center.errors.contactOtpRequired'
        );
        return;
      }
    }

    this.saving = true;
    try {
      const result = await this._centerProfileService.updateMyProfile({
        name: value.name ?? '',
        address: value.address ?? '',
        phone: value.phone ?? '',
        mobileNumber: mobile,
        email,
        latitude: value.latitude ?? null,
        longitude: value.longitude ?? null,
        establishedYear,
        description: value.description ?? '',
        website: value.website || null,
        employeeCount:
          (value.employeeCount as CenterProfileDto['employeeCount']) ?? null,
        economicCode: value.economicCode || null,
        registrationNumber: value.registrationNumber || null,
        code: contactChanged
          ? this.contactOtpControl.value?.trim() ?? ''
          : null,
        channel: contactChanged ? this.contactOtpChannel : null,
      });
      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.profile.center.errors.saveFailed'
            )
        );
      }
      this.success = this._localization.translate(
        wasApproved && !this.isLab
          ? 'modules.profile.center.success.savedPendingReapproval'
          : 'modules.profile.center.success.saved'
      );
      if (result.data) {
        this.applyProfileData(result.data, false);
        await this.loadPreviews();
      } else {
        await this.load(false);
      }
      if (this.getMissingItems().length) {
        this.highlightIncompleteFields();
      }
      if (yearClearedForDigits) {
        this.form.controls.establishedYear.setValue(null, { emitEvent: false });
        this.form.controls.establishedYear.markAsTouched();
        this.form.controls.establishedYear.setErrors({
          establishedYearDigits: true,
        });
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.center.errors.saveFailed'
            );
    } finally {
      this.saving = false;
    }
  }

  async resendContactOtp(): Promise<void> {
    if (
      !this.contactOtpChannel ||
      this.contactOtpCountdown > 0 ||
      this.sendingContactOtp
    )
      return;
    await this.sendContactOtp(this.contactOtpChannel);
  }

  get canResendContactOtp(): boolean {
    return this.contactOtpCountdown <= 0 && !this.sendingContactOtp;
  }

  private isContactChanged(mobile: string, email: string): boolean {
    const mobileChanged = !!mobile && mobile !== this._originalMobile.trim();
    const emailChanged =
      email.toLowerCase() !== this._originalEmail.trim().toLowerCase();
    return mobileChanged || emailChanged;
  }

  private async beginContactOtpFlow(): Promise<boolean> {
    const hasMobile = !!this._originalMobile.trim();
    const hasEmail = !!this._originalEmail.trim();
    if (!hasMobile && !hasEmail) {
      this.error = this._localization.translate(
        'modules.profile.center.errors.contactNoChannel'
      );
      return false;
    }

    let channel: UsernameChangeChannel | null = null;
    if (hasMobile && hasEmail) {
      channel = await this.askContactOtpChannel(hasEmail, hasMobile);
    } else if (hasMobile) {
      channel = 'Mobile';
    } else {
      channel = 'Email';
    }

    if (!channel) return false;
    return this.sendContactOtp(channel);
  }

  private async askContactOtpChannel(
    hasEmail: boolean,
    hasMobile: boolean
  ): Promise<UsernameChangeChannel | null> {
    const ref = this._dialog.open(UsernameChangeChannelDialogComponent, {
      width: '26.5rem',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        hasEmail,
        hasMobile,
        email: this._originalEmail,
        mobileNumber: this._originalMobile,
        titleKey: 'modules.profile.center.contactOtpChannelTitle',
        hintKey: 'modules.profile.center.contactOtpChannelHint',
        subhintKey: 'modules.profile.center.contactOtpChannelSubhint',
      },
    });
    return (await firstValueFrom(ref.afterClosed())) ?? null;
  }

  private async sendContactOtp(
    channel: UsernameChangeChannel
  ): Promise<boolean> {
    this.sendingContactOtp = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._centerProfileService.sendContactChangeOtp({
        channel,
      });
      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.profile.center.errors.contactOtpSendFailed'
            )
        );
      }
      this.contactOtpPending = true;
      this.contactOtpChannel = channel;
      this.contactOtpControl.setValue('');
      this.startContactOtpCountdown(120);
      this.success = this._localization.translate(
        channel === 'Mobile'
          ? 'modules.profile.center.contactOtpSentMobile'
          : 'modules.profile.center.contactOtpSentEmail'
      );
      return true;
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.center.errors.contactOtpSendFailed'
            );
      return false;
    } finally {
      this.sendingContactOtp = false;
    }
  }

  private resetContactOtpState(): void {
    this.contactOtpPending = false;
    this.contactOtpChannel = null;
    this.contactOtpControl.setValue('');
    this.clearContactOtpCountdown();
  }

  private startContactOtpCountdown(seconds: number): void {
    this.clearContactOtpCountdown();
    this.contactOtpCountdown = seconds;
    this._contactOtpTimer = setInterval(() => {
      this.contactOtpCountdown -= 1;
      if (this.contactOtpCountdown <= 0) {
        this.clearContactOtpCountdown();
      }
    }, 1000);
  }

  private clearContactOtpCountdown(): void {
    if (this._contactOtpTimer) {
      clearInterval(this._contactOtpTimer);
      this._contactOtpTimer = null;
    }
    this.contactOtpCountdown = 0;
  }

  async onFileSelected(kind: CenterProfileFileKind, file: File): Promise<void> {
    if (this.readOnly) return;
    if (!(await this.confirmApprovedEdit())) return;

    const wasApproved = this.profile?.isApproved ?? false;
    this.error = '';
    this.success = '';
    this.uploading = kind;
    try {
      const upload =
        kind === 'Logo'
          ? this._centerProfileService.uploadLogo(file)
          : kind === 'NationalCard'
          ? this._centerProfileService.uploadNationalCard(file)
          : kind === 'License'
          ? this._centerProfileService.uploadLicense(file)
          : kind === 'OfficialImage'
          ? this._centerProfileService.uploadOfficialImage(file)
          : this._centerProfileService.uploadTradeCard(file);

      const result = await upload;
      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.profile.center.errors.uploadFailed'
            )
        );
      }
      this.success = this._localization.translate(
        wasApproved && !this.isLab
          ? 'modules.profile.center.success.uploadedPendingReapproval'
          : 'modules.profile.center.success.uploaded'
      );
      if (result.data) {
        this.setFileName(kind, file.name);
        this.applyProfileData(result.data, true);
        await this.loadPreviews();
      } else {
        this.setFileName(kind, file.name);
        await this.load(true);
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.center.errors.uploadFailed'
            );
    } finally {
      this.uploading = null;
    }
  }

  async onFileCleared(kind: CenterProfileFileKind): Promise<void> {
    if (this.readOnly) return;
    if (!(await this.confirmApprovedEdit())) return;

    const wasApproved = this.profile?.isApproved ?? false;
    this.error = '';
    this.success = '';
    this.uploading = kind;
    try {
      const result = await this._centerProfileService.deleteFile(kind);
      if (!result.success) {
        throw new Error(
          result.message ??
            this._localization.translate(
              'modules.profile.center.errors.deleteFailed'
            )
        );
      }
      this.success = this._localization.translate(
        wasApproved && !this.isLab
          ? 'modules.profile.center.success.deletedPendingReapproval'
          : 'modules.profile.center.success.deleted'
      );
      this.setFileName(kind, '');
      if (result.data) {
        this.applyProfileData(result.data, true);
        await this.loadPreviews();
      } else {
        await this.load(true);
      }
    } catch (e: unknown) {
      this.error = this.readErrorMessage(
        e,
        'modules.profile.center.errors.deleteFailed'
      );
    } finally {
      this.uploading = null;
    }
  }

  statusKey(): string {
    if (!this.profile) return '';
    if (this.profile.isApproved)
      return 'modules.profile.center.status.approved';
    if (this.profile.rejectionReason && !this.profile.isComplete)
      return 'modules.profile.center.status.rejected';
    if (this.profile.isComplete)
      return 'modules.profile.center.status.awaitingApproval';
    return 'modules.profile.center.status.incomplete';
  }

  statusClass(): string {
    if (!this.profile) return '';
    if (this.profile.isApproved) return 'is-approved';
    if (this.profile.rejectionReason && !this.profile.isComplete)
      return 'is-rejected';
    if (this.profile.isComplete) return 'is-pending';
    return 'is-incomplete';
  }

  clearSuccess(): void {
    this.success = '';
  }

  private readErrorMessage(error: unknown, fallbackKey: string): string {
    if (error instanceof HttpErrorResponse) {
      const payload = error.error as { message?: string } | string | null;
      if (payload && typeof payload === 'object' && payload.message?.trim()) {
        return payload.message;
      }
      if (typeof payload === 'string' && payload.trim()) {
        return payload;
      }
    }
    if (error && typeof error === 'object' && 'error' in error) {
      const payload = (error as { error?: { message?: string } }).error;
      if (payload?.message?.trim()) {
        return payload.message;
      }
    }
    if (error && typeof error === 'object' && 'message' in error) {
      const message = (error as { message?: string }).message;
      if (message?.trim()) {
        return message;
      }
    }
    if (error instanceof Error && error.message.trim()) {
      return error.message;
    }
    return this._localization.translate(fallbackKey);
  }

  private setFileName(kind: CenterProfileFileKind, name: string): void {
    if (kind === 'Logo') this.logoFileName = name;
    else if (kind === 'NationalCard') this.nationalCardFileName = name;
    else if (kind === 'License') this.licenseFileName = name;
    else if (kind === 'OfficialImage') this.officialImageFileName = name;
    else this.tradeCardFileName = name;
  }

  private async loadPreviews(): Promise<void> {
    this.revokeUrls();
    if (!this.profile) return;

    if (this.profile.hasLogo) {
      this.logoUrl = await this.createObjectUrl(this.profile.id, 'Logo');
    }
    if (this.profile.hasNationalCard) {
      this.nationalCardUrl = await this.createObjectUrl(
        this.profile.id,
        'NationalCard'
      );
    }
    if (this.profile.hasLicense) {
      this.licenseUrl = await this.createObjectUrl(this.profile.id, 'License');
    }
    if (this.showCompanyDocuments && this.profile.hasOfficialImage) {
      this.officialImageUrl = await this.createObjectUrl(
        this.profile.id,
        'OfficialImage'
      );
    }
    if (this.showCompanyDocuments && this.profile.hasTradeCard) {
      this.tradeCardUrl = await this.createObjectUrl(
        this.profile.id,
        'TradeCard'
      );
    }
  }

  private async createObjectUrl(
    profileId: string,
    kind: CenterProfileFileKind
  ): Promise<string> {
    const blob = await this._centerProfileService.getFileBlob(profileId, kind);
    return URL.createObjectURL(blob);
  }

  private revokeUrls(): void {
    for (const url of [
      this.logoUrl,
      this.nationalCardUrl,
      this.licenseUrl,
      this.officialImageUrl,
      this.tradeCardUrl,
    ]) {
      if (url) URL.revokeObjectURL(url);
    }
    this.logoUrl = '';
    this.nationalCardUrl = '';
    this.licenseUrl = '';
    this.officialImageUrl = '';
    this.tradeCardUrl = '';
  }
}
