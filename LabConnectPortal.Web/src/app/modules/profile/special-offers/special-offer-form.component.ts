import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { RichTextEditorComponent } from '@modules/base/components/rich-text-editor/rich-text-editor.component';
import { TestInfosService } from '../test-infos/test-infos.service';
import { SpecialOfferFormColDef } from './special-offer-form.coldef';
import { SpecialOfferSelectionState } from './special-offer-form-tests.renderer';
import { SpecialOffersService } from './special-offers.service';
import { SpecialOfferTestRow } from './special-offers.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

@Component({
  selector: 'app-special-offer-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BaseCheckboxComponent,
    MatIconModule,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseGridComponent,
    RichTextEditorComponent,
  ],
  templateUrl: './special-offer-form.component.html',
  styleUrl: './special-offer-form.component.scss',
})
export class SpecialOfferFormComponent implements OnInit {
  readonly entity = SystemEntity.SpecialOffer;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.SpecialOffer);

  private _destroyRef = inject(DestroyRef);

  loading = false;
  saving = false;
  error = '';
  isEdit = false;
  offerId = 0;
  testRows: SpecialOfferTestRow[] = [];
  filteredTestRows: SpecialOfferTestRow[] = [];
  colDef: ColDef<SpecialOfferTestRow>[] = [];
  readonly tests$ = new BehaviorSubject<SpecialOfferTestRow[]>([]);
  readonly selectionState$ = new BehaviorSubject<SpecialOfferSelectionState>({
    allSelected: false,
    someSelected: false,
  });
  bulkDiscountControl = new FormControl<number | null>(null);
  searchControl = new FormControl('', { nonNullable: true });

  form = new FormGroup({
    title: new FormControl('', [Validators.required, Validators.maxLength(300)]),
    summary: new FormControl('', [Validators.required, Validators.maxLength(1000)]),
    fullBody: new FormControl(''),
    startDate: new FormControl<Moment | null>(null, Validators.required),
    endDate: new FormControl<Moment | null>(null, Validators.required),
    isActive: new FormControl(true, { nonNullable: true }),
  });

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _service: SpecialOffersService,
    private _testInfosService: TestInfosService,
    private _localization: LocalizationService,
    private _labPermission: LabPermissionService,
    private _colDef: SpecialOfferFormColDef,
  ) {}

  get canSave(): boolean {
    return this._labPermission.can(
      this.entity,
      this.isEdit ? 'update' : 'create'
    );
  }

  get allSelected(): boolean {
    return this.filteredTestRows.length > 0 && this.filteredTestRows.every((row) => row.selected);
  }

  get someSelected(): boolean {
    return this.filteredTestRows.some((row) => row.selected) && !this.allSelected;
  }

  ngOnInit(): void {
    const idParam = this._route.snapshot.paramMap.get('id') ?? '';
    this.isEdit = idParam !== '' && idParam !== 'new';
    this.offerId = this.isEdit ? Number(idParam) : 0;

    this.colDef = this._colDef.get({
      selectionState$: this.selectionState$,
      bulkDiscountControl: this.bulkDiscountControl,
      onToggleAll: (checked) => this.toggleSelectAll(checked),
      onToggleRow: (row, checked) => this.toggleRow(row, checked),
      onDiscountChange: (row, value) => this.onDiscountChange(row, value),
      onMaxSamplesChange: (row, value) => this.onMaxSamplesChange(row, value),
      onBulkDiscountInput: (value) => this.onBulkDiscountInput(value),
    });

    this.searchControl.valueChanges.pipe(takeUntilDestroyed(this._destroyRef)).subscribe(() => {
      this.applyFilter();
    });

    this.bulkDiscountControl.valueChanges.pipe(takeUntilDestroyed(this._destroyRef)).subscribe((value) => {
      if (value == null || Number.isNaN(value)) return;
      this.applyBulkDiscount(value);
    });

    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const testsResult = await this._testInfosService.getAll();
      if (!testsResult.success || !testsResult.data) {
        throw new Error(
          testsResult.message ?? this._localization.translate('modules.profile.specialOffers.errors.loadTestsFailed'),
        );
      }

      const selectedMap = new Map<number, { discount: number | null; maxSamples: number | null }>();

      if (this.isEdit) {
        const offerResult = await this._service.getById(this.offerId);
        if (!offerResult.success || !offerResult.data) {
          throw new Error(
            offerResult.message ?? this._localization.translate('modules.profile.specialOffers.errors.loadFailed'),
          );
        }
        const offer = offerResult.data;
        this.form.patchValue({
          title: offer.title,
          summary: offer.summary,
          fullBody: offer.fullBody,
          startDate: this.parseDate(offer.startDate),
          endDate: this.parseDate(offer.endDate),
          isActive: offer.isActive,
        });
        for (const test of offer.tests) {
          selectedMap.set(test.testInfoId, {
            discount: test.discount ?? null,
            maxSamples: test.maxSamples ?? null,
          });
        }
      }

      this.testRows = testsResult.data.map((test) => {
        const selected = selectedMap.get(test.id);
        return {
          testInfoId: test.id,
          cpnCode: test.cpnCode,
          nationalCode: test.nationalCode,
          fullName: test.fullName,
          shortName: test.shortName,
          sectionName: test.sectionName,
          approvePrice: test.approvePrice,
          selected: !!selected,
          discount: selected?.discount ?? null,
          maxSamples: selected?.maxSamples ?? null,
        };
      });
      this.applyFilter();
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.profile.specialOffers.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  applyFilter(): void {
    const term = this.normalize(this.searchControl.value);
    this.filteredTestRows = !term
      ? [...this.testRows]
      : this.testRows.filter((row) => {
          const haystack = [
            row.fullName,
            row.shortName,
            row.sectionName,
            row.cpnCode,
            row.nationalCode,
          ]
            .map((value) => this.normalize(value))
            .join(' ');
          return haystack.includes(term);
        });
    this.tests$.next(this.filteredTestRows);
    this.syncHeaderSelection();
  }

  toggleSelectAll(checked: boolean): void {
    const visibleIds = new Set(this.filteredTestRows.map((row) => row.testInfoId));
    this.testRows = this.testRows.map((row) =>
      visibleIds.has(row.testInfoId) ? { ...row, selected: checked } : row,
    );
    this.applyFilter();
  }

  toggleRow(row: SpecialOfferTestRow, checked: boolean): void {
    this.testRows = this.testRows.map((item) =>
      item.testInfoId === row.testInfoId ? { ...item, selected: checked } : item,
    );
    this.applyFilter();
  }

  private syncHeaderSelection(): void {
    this.selectionState$.next({
      allSelected: this.allSelected,
      someSelected: this.someSelected,
    });
  }

  applyBulkDiscount(value: number | null): void {
    if (value == null || Number.isNaN(value)) return;

    this.testRows = this.testRows.map((row) => ({ ...row, discount: value }));
    this.applyFilter();
  }

  onBulkDiscountInput(value: string): void {
    const parsed = value.trim() === '' ? null : Number(value);
    if (parsed != null && Number.isFinite(parsed)) {
      this.applyBulkDiscount(parsed);
    }
  }

  onDiscountChange(row: SpecialOfferTestRow, value: string | number | null): void {
    const text = value == null ? '' : String(value).trim();
    const parsed = text === '' ? null : Number(text);
    row.discount = parsed != null && Number.isFinite(parsed) ? parsed : null;
  }

  onMaxSamplesChange(row: SpecialOfferTestRow, value: string | number | null): void {
    const text = value == null ? '' : String(value).trim();
    const parsed = text === '' ? null : Number(text);
    row.maxSamples = parsed != null && Number.isFinite(parsed) ? Math.trunc(parsed) : null;
  }

  async save(): Promise<void> {
    if (!this.canSave) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const selectedTests = this.testRows.filter((row) => row.selected);
    if (!selectedTests.length) {
      this.error = this._localization.translate('modules.profile.specialOffers.errors.noTestsSelected');
      return;
    }

    const startDate = this.buildDateIso(this.form.controls.startDate.value);
    const endDate = this.buildDateIso(this.form.controls.endDate.value);
    if (!startDate || !endDate) {
      this.error = this._localization.translate('modules.profile.specialOffers.errors.invalidDates');
      return;
    }

    this.saving = true;
    this.error = '';
    try {
      const payload = {
        title: this.form.controls.title.value?.trim() ?? '',
        summary: this.form.controls.summary.value?.trim() ?? '',
        fullBody: this.form.controls.fullBody.value ?? '',
        startDate,
        endDate,
        isActive: this.form.controls.isActive.value,
        tests: selectedTests.map((row) => ({
          testInfoId: row.testInfoId,
          discount: row.discount,
          maxSamples: row.maxSamples,
        })),
      };

      const result = this.isEdit
        ? await this._service.update({ ...payload, id: this.offerId })
        : await this._service.create(payload);

      if (!result.success) {
        throw new Error(
          result.message ?? this._localization.translate('modules.profile.specialOffers.errors.saveFailed'),
        );
      }

      void this._router.navigate(['/profile/special-offers'], {
        queryParamsHandling: 'preserve',
      });
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.profile.specialOffers.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  formatPrice(value: number | null | undefined): string {
    if (value == null) return '—';
    return new Intl.NumberFormat('fa-IR').format(value);
  }

  private parseDate(value: string): Moment | null {
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.clone() : null;
  }

  private buildDateIso(date: Moment | null): string | null {
    if (!date) return null;
    return date.clone().startOf('day').toISOString();
  }

  private normalize(value: string | null | undefined): string {
    return (value ?? '').trim().toLowerCase();
  }
}
