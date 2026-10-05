import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  Injector,
  OnInit,
  ViewChild,
  afterNextRender,
  inject,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import {
  CONTRACT_TYPES,
  ContractType,
  DEGREE_LEVELS,
  DegreeLevel,
  getEnumName,
} from '../profile/resume/resume.types';
import { GENDER_REQUIREMENTS } from '../profile/job-postings/job-postings.types';
import { PublicJobsService } from './jobs.service';
import {
  GenderRequirement,
  PublicJobPostingCardDto,
  PublicJobPostingFiltersDto,
} from './jobs.types';
import { JobCardComponent } from './job-card.component';

@Component({
  selector: 'app-jobs-list',
  standalone: true,
  imports: [ReactiveFormsModule, TranslocoPipe, MatIconModule, JobCardComponent],
  templateUrl: './jobs-list.component.html',
  styleUrl: './jobs-list.component.scss',
})
export class JobsListComponent implements OnInit, AfterViewInit {
  private _publicJobsService = inject(PublicJobsService);
  private _localization = inject(LocalizationService);
  private _route = inject(ActivatedRoute);
  private _router = inject(Router);
  private _destroyRef = inject(DestroyRef);
  private _injector = inject(Injector);

  @ViewChild('loadMoreSentinel') private _loadMoreSentinel?: ElementRef<HTMLElement>;

  loading = false;
  loadingMore = false;
  filtersLoading = false;
  error = '';
  jobs: PublicJobPostingCardDto[] = [];
  filterOptions: PublicJobPostingFiltersDto = createDefaultJobFilterOptions();
  totalCount = 0;
  page = 1;
  pageSize = 15;
  searchTerm = '';
  selectedProvinceIds: number[] = [];
  selectedSalaryRangeIds: number[] = [];
  selectedContractTypes: ContractType[] = [];
  selectedGenderRequirements: GenderRequirement[] = [];
  selectedDegreeLevels: DegreeLevel[] = [];
  searchInput = new FormControl('', { nonNullable: true });
  provinceSearchInput = new FormControl('', { nonNullable: true });
  expandedSections = {
    province: true,
    salary: true,
    contract: true,
    gender: true,
    degree: true,
  };
  private _infiniteScrollObserver?: IntersectionObserver;
  private readonly _scrollPrefetchRows = 5;
  private readonly _estimatedJobRowHeightPx = 280;

  ngOnInit(): void {
    void this.loadFilterOptions();

    this.searchInput.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed(this._destroyRef))
      .subscribe((term) => {
        void this._router.navigate([], {
          relativeTo: this._route,
          queryParams: { search: term.trim() || null },
          queryParamsHandling: 'merge',
        });
      });

    this._route.queryParamMap.pipe(takeUntilDestroyed(this._destroyRef)).subscribe((params) => {
      this.searchTerm = params.get('search')?.trim() ?? '';
      this.searchInput.setValue(this.searchTerm, { emitEvent: false });
      this.selectedProvinceIds = this.parseIdCsv(params.get('provinceIds'));
      this.selectedSalaryRangeIds = this.parseIdCsv(params.get('salaryRangeIds'));
      this.selectedContractTypes = this.parseContractTypes(params.get('contractTypes'));
      this.selectedGenderRequirements = this.parseGenderRequirements(params.get('genders'));
      this.selectedDegreeLevels = this.parseDegreeLevels(params.get('degreeLevels'));
      this.page = 1;
      void this.load();
    });

    this._destroyRef.onDestroy(() => this._infiniteScrollObserver?.disconnect());
  }

  ngAfterViewInit(): void {
    this._infiniteScrollObserver = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) void this.loadMore();
      },
      {
        root: null,
        rootMargin: `0px 0px ${this._scrollPrefetchRows * this._estimatedJobRowHeightPx}px 0px`,
        threshold: 0,
      }
    );
    this.observeLoadMoreSentinel();
  }

  get hasMoreJobs(): boolean {
    return this.jobs.length < this.totalCount;
  }

  get filteredProvinces() {
    return this.filterListItems(this.filterOptions?.provinces ?? [], this.provinceSearchInput.value);
  }

  async loadFilterOptions(): Promise<void> {
    this.filtersLoading = true;
    try {
      const result = await this._publicJobsService.getPostingFilters();
      if (result.success && result.data) this.filterOptions = this.mergeFilterOptions(result.data);
    } catch {
      // Keep default filter options when API is unavailable.
    } finally {
      this.filtersLoading = false;
    }
  }

  async load(): Promise<void> {
    this.page = 1;
    await this.fetchJobs(true);
  }

  async loadMore(): Promise<void> {
    if (this.loading || this.loadingMore || !this.hasMoreJobs) return;
    this.page += 1;
    await this.fetchJobs(false);
  }

  toggleSection(section: keyof typeof this.expandedSections): void {
    this.expandedSections[section] = !this.expandedSections[section];
  }

  isProvinceSelected(provinceId: number): boolean {
    return this.selectedProvinceIds.includes(provinceId);
  }

  isSalaryRangeSelected(salaryRangeId: number): boolean {
    return this.selectedSalaryRangeIds.includes(salaryRangeId);
  }

  isContractTypeSelected(contractType: ContractType | string): boolean {
    const normalized = this.normalizeContractType(contractType);
    return this.selectedContractTypes.includes(normalized);
  }

  isGenderSelected(gender: GenderRequirement): boolean {
    return this.selectedGenderRequirements.includes(gender);
  }

  isDegreeSelected(degree: DegreeLevel | string): boolean {
    const normalized = this.normalizeDegreeLevel(degree);
    return this.selectedDegreeLevels.includes(normalized);
  }

  onProvinceChange(provinceId: number, checked: boolean): void {
    const next = checked
      ? [...this.selectedProvinceIds, provinceId]
      : this.selectedProvinceIds.filter((id) => id !== provinceId);
    this.updateFilterQuery({ provinceIds: this.serializeIds(next) });
  }

  onSalaryRangeChange(salaryRangeId: number, checked: boolean): void {
    const next = checked
      ? [...this.selectedSalaryRangeIds, salaryRangeId]
      : this.selectedSalaryRangeIds.filter((id) => id !== salaryRangeId);
    this.updateFilterQuery({ salaryRangeIds: this.serializeIds(next) });
  }

  onContractTypeChange(contractType: ContractType | string, checked: boolean): void {
    const normalized = this.normalizeContractType(contractType);
    const next = checked
      ? [...this.selectedContractTypes, normalized]
      : this.selectedContractTypes.filter((value) => value !== normalized);
    this.updateFilterQuery({ contractTypes: this.serializeContractTypes(next) });
  }

  onGenderChange(gender: GenderRequirement, checked: boolean): void {
    const next = checked
      ? [...this.selectedGenderRequirements, gender]
      : this.selectedGenderRequirements.filter((value) => value !== gender);
    this.updateFilterQuery({ genders: this.serializeGenderRequirements(next) });
  }

  onDegreeChange(degree: DegreeLevel | string, checked: boolean): void {
    const normalized = this.normalizeDegreeLevel(degree);
    const next = checked
      ? [...this.selectedDegreeLevels, normalized]
      : this.selectedDegreeLevels.filter((value) => value !== normalized);
    this.updateFilterQuery({ degreeLevels: this.serializeDegreeLevels(next) });
  }

  clearSearchFilter(): void {
    this.searchInput.setValue('');
  }

  clearProvinceSearch(): void {
    this.provinceSearchInput.setValue('');
  }

  clearAllFilters(): void {
    this.searchInput.setValue('');
    this.provinceSearchInput.setValue('');
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: {
        search: null,
        provinceIds: null,
        salaryRangeIds: null,
        contractTypes: null,
        genders: null,
        degreeLevels: null,
      },
    });
  }

  hasActiveFilters(): boolean {
    return !!(
      this.searchTerm ||
      this.selectedProvinceIds.length ||
      this.selectedSalaryRangeIds.length ||
      this.selectedContractTypes.length ||
      this.selectedGenderRequirements.length ||
      this.selectedDegreeLevels.length
    );
  }

  contractTypeLabel(contractType: ContractType | string): string {
    const name = getEnumName(ContractType, this.normalizeContractType(contractType));
    return this._localization.translate(`modules.profile.resume.enums.contractType.${name}`);
  }

  genderLabel(gender: GenderRequirement): string {
    return this._localization.translate(`enum.genderRequirement.${gender}`);
  }

  degreeLabel(degree: DegreeLevel | string): string {
    const name = getEnumName(DegreeLevel, this.normalizeDegreeLevel(degree));
    return this._localization.translate(`modules.profile.resume.enums.degreeLevel.${name}`);
  }

  private async fetchJobs(reset: boolean): Promise<void> {
    if (reset) {
      this.loading = true;
      this.error = '';
    } else this.loadingMore = true;

    try {
      const result = await this._publicJobsService.getActivePostings({
        page: this.page,
        pageSize: this.pageSize,
        search: this.searchTerm || undefined,
        provinceIds: this.selectedProvinceIds.length ? this.selectedProvinceIds : undefined,
        salaryRangeIds: this.selectedSalaryRangeIds.length ? this.selectedSalaryRangeIds : undefined,
        contractTypes: this.selectedContractTypes.length ? this.selectedContractTypes : undefined,
        genderRequirements: this.selectedGenderRequirements.length
          ? this.selectedGenderRequirements
          : undefined,
        minimumDegreeLevels: this.selectedDegreeLevels.length ? this.selectedDegreeLevels : undefined,
      });
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.jobs.errors.loadFailed')
        );
      }

      if (reset) {
        this.jobs = result.data.items;
      } else {
        const existingIds = new Set(this.jobs.map((job) => job.jobPostingId));
        const newItems = result.data.items.filter((job) => !existingIds.has(job.jobPostingId));
        this.jobs = [...this.jobs, ...newItems];
      }

      this.totalCount = result.data.totalCount;
    } catch (e: unknown) {
      if (reset) {
        this.error =
          e instanceof Error
            ? e.message
            : this._localization.translate('modules.jobs.errors.loadFailed');
      } else if (this.page > 1) this.page -= 1;
    } finally {
      this.loading = false;
      this.loadingMore = false;
      this.observeLoadMoreSentinel();
    }
  }

  private mergeFilterOptions(data: PublicJobPostingFiltersDto): PublicJobPostingFiltersDto {
    const defaults = createDefaultJobFilterOptions();
    return {
      provinces: data.provinces?.length ? data.provinces : defaults.provinces,
      salaryRanges: data.salaryRanges?.length ? data.salaryRanges : defaults.salaryRanges,
      contractTypes: (data.contractTypes?.length ? data.contractTypes : defaults.contractTypes).map((value) =>
        this.normalizeContractType(value)
      ),
      genderRequirements: data.genderRequirements?.length
        ? data.genderRequirements
        : defaults.genderRequirements,
      minimumDegreeLevels: (data.minimumDegreeLevels?.length
        ? data.minimumDegreeLevels
        : defaults.minimumDegreeLevels
      ).map((value) => this.normalizeDegreeLevel(value)),
    };
  }

  private normalizeContractType(value: ContractType | string): ContractType {
    if (typeof value === 'number') return value as ContractType;
    const numeric = Number(value);
    if (!Number.isNaN(numeric) && ContractType[numeric] !== undefined) return numeric as ContractType;
    const direct = ContractType[value as keyof typeof ContractType];
    if (typeof direct === 'number') return direct;
    const pascal = value.charAt(0).toUpperCase() + value.slice(1);
    const fromPascal = ContractType[pascal as keyof typeof ContractType];
    if (typeof fromPascal === 'number') return fromPascal;
    return numeric as ContractType;
  }

  private normalizeDegreeLevel(value: DegreeLevel | string): DegreeLevel {
    if (typeof value === 'number') return value as DegreeLevel;
    const numeric = Number(value);
    if (!Number.isNaN(numeric) && DegreeLevel[numeric] !== undefined) return numeric as DegreeLevel;
    const direct = DegreeLevel[value as keyof typeof DegreeLevel];
    if (typeof direct === 'number') return direct;
    const pascal = value.charAt(0).toUpperCase() + value.slice(1);
    const fromPascal = DegreeLevel[pascal as keyof typeof DegreeLevel];
    if (typeof fromPascal === 'number') return fromPascal;
    return numeric as DegreeLevel;
  }

  private observeLoadMoreSentinel(): void {
    afterNextRender(
      () => {
        this._infiniteScrollObserver?.disconnect();
        const sentinel = this._loadMoreSentinel?.nativeElement;
        if (sentinel && this.hasMoreJobs) this._infiniteScrollObserver?.observe(sentinel);
      },
      { injector: this._injector }
    );
  }

  private updateFilterQuery(partial: Record<string, string | null>): void {
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: partial,
      queryParamsHandling: 'merge',
    });
  }

  private filterListItems<T extends { name: string }>(items: T[], term: string): T[] {
    const normalized = term.trim().toLowerCase();
    if (!normalized) return items;
    return items.filter((item) => item.name.toLowerCase().includes(normalized));
  }

  private parseIdCsv(param: string | null): number[] {
    if (!param) return [];
    return param
      .split(',')
      .map((value) => Number(value.trim()))
      .filter((value) => !Number.isNaN(value));
  }

  private parseContractTypes(param: string | null): ContractType[] {
    if (!param) return [];
    return param
      .split(',')
      .map((value) => value.trim())
      .filter((value) => value in ContractType)
      .map((value) => ContractType[value as keyof typeof ContractType] as ContractType);
  }

  private parseGenderRequirements(param: string | null): GenderRequirement[] {
    if (!param) return [];
    const allowed = new Set<string>(GENDER_REQUIREMENTS);
    return param
      .split(',')
      .map((value) => value.trim())
      .filter((value): value is GenderRequirement => allowed.has(value));
  }

  private parseDegreeLevels(param: string | null): DegreeLevel[] {
    if (!param) return [];
    return param
      .split(',')
      .map((value) => value.trim())
      .filter((value) => value in DegreeLevel)
      .map((value) => DegreeLevel[value as keyof typeof DegreeLevel] as DegreeLevel);
  }

  private serializeIds(ids: number[]): string | null {
    return ids.length ? ids.join(',') : null;
  }

  private serializeContractTypes(values: ContractType[]): string | null {
    return values.length ? values.map((value) => ContractType[value]).join(',') : null;
  }

  private serializeGenderRequirements(values: GenderRequirement[]): string | null {
    return values.length ? values.join(',') : null;
  }

  private serializeDegreeLevels(values: DegreeLevel[]): string | null {
    return values.length ? values.map((value) => DegreeLevel[value]).join(',') : null;
  }
}

function createDefaultJobFilterOptions(): PublicJobPostingFiltersDto {
  return {
    provinces: [],
    salaryRanges: [],
    contractTypes: [...CONTRACT_TYPES],
    genderRequirements: [...GENDER_REQUIREMENTS],
    minimumDegreeLevels: [...DEGREE_LEVELS],
  };
}
