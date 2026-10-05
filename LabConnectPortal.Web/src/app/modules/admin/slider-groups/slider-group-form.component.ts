import { CdkDragDrop, DragDropModule, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import {
  BaseMenuActionItem,
  BaseMenuActionsComponent,
} from '@modules/base/components/base-menu-actions/base-menu-actions.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SliderGroupsService } from './slider-groups.service';
import { SliderGroupDto, SliderSlideDto } from './slider-groups.types';

@Component({
  selector: 'app-slider-group-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    MatIconModule,
    DragDropModule,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseMenuActionsComponent,
  ],
  templateUrl: './slider-group-form.component.html',
  styleUrl: './slider-group-form.component.scss',
})
export class SliderGroupFormComponent implements OnInit {
  readonly entity = SystemEntity.SliderGroup;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.SliderGroup);

  loading = false;
  saving = false;
  savingSlideId: string | null = null;
  reordering = false;
  uploadingSlide = false;
  error = '';
  success = '';
  isNew = true;
  groupId = '';
  slides: SliderSlideDto[] = [];
  editingSlideId: string | null = null;
  private _slideForms = new Map<string, FormGroup>();

  form = new FormGroup({
    title: new FormControl('', Validators.required),
    startDate: new FormControl<Moment | null>(null, Validators.required),
    endDate: new FormControl<Moment | null>(null, Validators.required),
    isActive: new FormControl<boolean | string>(true, Validators.required),
    sortOrder: new FormControl(0, Validators.required),
  });

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _sliderGroupsService: SliderGroupsService,
    private _localization: LocalizationService,
  ) {}

  get isActiveOptions(): BaseFormSelectOption[] {
    return [
      { value: true, label: this._localization.translate('modules.admin.sliderGroups.active') },
      { value: false, label: this._localization.translate('modules.admin.sliderGroups.inactive') },
    ];
  }

  get isGroupActive(): boolean {
    const value = this.form.controls.isActive.value;
    return value === true || value === 'true';
  }

  slideControl(slide: SliderSlideDto, field: 'title' | 'linkUrl' | 'sortOrder'): FormControl {
    return this.getSlideForm(slide).controls[field] as FormControl;
  }

  slideForm(slide: SliderSlideDto): FormGroup {
    return this.getSlideForm(slide);
  }

  slideActions(slide: SliderSlideDto): BaseMenuActionItem[] {
    const isEditing = this.editingSlideId === slide.id;
    return [
      {
        label: this._localization.translate(
          isEditing
            ? 'modules.admin.sliderGroups.cancelEdit'
            : 'modules.admin.sliderGroups.editSlide',
        ),
        icon: isEditing ? 'heroicons_outline:x-mark' : 'heroicons_outline:pencil-square',
        iconClass: 'text-blue-500',
        action: () => this.toggleEditSlide(slide),
      },
      {
        label: this._localization.translate('shared.delete'),
        icon: 'heroicons_outline:trash',
        iconClass: 'text-red-500',
        danger: true,
        action: () => void this.deleteSlide(slide),
      },
    ];
  }

  ngOnInit(): void {
    this.groupId = this._route.snapshot.paramMap.get('id') ?? '';
    this.isNew = !this.groupId || this.groupId === 'new';
    void this.load();
  }

  slideImageUrl(imagePath: string): string {
    return SliderGroupsService.slideImageUrl(imagePath);
  }

  async load(): Promise<void> {
    if (this.isNew) {
      this.form.patchValue({
        startDate: jMoment(),
        endDate: jMoment().add(1, 'jYear'),
      });
      return;
    }

    this.loading = true;
    this.error = '';
    try {
      const result = await this._sliderGroupsService.getById(this.groupId);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.sliderGroups.errors.loadFailed'),
        );
      }
      this.applyGroup(result.data);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.sliderGroups.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  private applyGroup(group: SliderGroupDto): void {
    this._slideForms.clear();
    this.editingSlideId = null;
    this.form.patchValue({
      title: group.title,
      startDate: jMoment(group.startDate),
      endDate: jMoment(group.endDate),
      isActive: group.isActive,
      sortOrder: group.sortOrder,
    });
    this.slides = [...group.slides].sort((a, b) => a.sortOrder - b.sortOrder);
  }

  private buildCommand() {
    const value = this.form.getRawValue();
    return {
      title: (value.title ?? '').trim(),
      startDate: this.toApiDate(value.startDate),
      endDate: this.toApiDate(value.endDate),
      isActive: value.isActive === true || value.isActive === 'true',
      sortOrder: Number(value.sortOrder ?? 0),
    };
  }

  /** Serialize Moment with Latin digits so ASP.NET can parse the date. */
  private toApiDate(value: Moment | string | null | undefined): string {
    if (!value) return '';
    const date = jMoment.isMoment(value) ? value.clone() : jMoment(value);
    if (!date.isValid()) return '';
    return `${date.locale('en').format('YYYY-MM-DD')}T00:00:00`;
  }

  async saveGroup(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = this._localization.translate(
        'modules.admin.sliderGroups.errors.validationFailed',
      );
      return;
    }
    this.saving = true;
    this.error = '';
    this.success = '';
    try {
      const command = this.buildCommand();
      if (!command.startDate || !command.endDate) {
        throw new Error(
          this._localization.translate('modules.admin.sliderGroups.errors.validationFailed'),
        );
      }

      const result = this.isNew
        ? await this._sliderGroupsService.create(command)
        : await this._sliderGroupsService.update({ ...command, id: this.groupId });

      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.sliderGroups.errors.saveFailed'),
        );
      }

      if (this.isNew) {
        void this._router.navigate(['/admin/slider-groups', result.data.id]);
        return;
      }

      this.applyGroup(result.data);
      this.success = this._localization.translate('modules.admin.sliderGroups.success.saved');
    } catch (e: unknown) {
      this.error = this.resolveErrorMessage(
        e,
        'modules.admin.sliderGroups.errors.saveFailed',
      );
    } finally {
      this.saving = false;
    }
  }

  clearSuccess(): void {
    this.success = '';
  }

  private resolveErrorMessage(e: unknown, fallbackKey: string): string {
    if (e instanceof Error && e.message) return e.message;
    if (e && typeof e === 'object' && 'message' in e) {
      const message = (e as { message?: unknown }).message;
      if (typeof message === 'string' && message.trim()) return message;
    }
    return this._localization.translate(fallbackKey);
  }

  async onSlideImageSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.groupId) return;

    this.uploadingSlide = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._sliderGroupsService.uploadSlideImage(this.groupId, file);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.sliderGroups.errors.uploadFailed'),
        );
      }
      this.slides = [...this.slides, result.data].sort((a, b) => a.sortOrder - b.sortOrder);
      this.editingSlideId = result.data.id;
      this.resetSlideForm(result.data);
      this.success = this._localization.translate(
        'modules.admin.sliderGroups.success.slideUploaded',
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.sliderGroups.errors.uploadFailed');
    } finally {
      this.uploadingSlide = false;
      input.value = '';
    }
  }

  toggleEditSlide(slide: SliderSlideDto): void {
    if (this.editingSlideId === slide.id) {
      this.editingSlideId = null;
      return;
    }
    this.resetSlideForm(slide);
    this.editingSlideId = slide.id;
  }

  async saveSlide(slide: SliderSlideDto): Promise<void> {
    this.error = '';
    this.success = '';
    this.savingSlideId = slide.id;
    const slideForm = this._slideForms.get(slide.id);
    if (slideForm) {
      const value = slideForm.getRawValue();
      slide.title = value.title ?? '';
      slide.linkUrl = value.linkUrl || null;
      slide.sortOrder = Number(value.sortOrder ?? 0);
    }
    try {
      const result = await this._sliderGroupsService.updateSlide({
        id: slide.id,
        sliderGroupId: this.groupId,
        title: slide.title ?? '',
        linkUrl: slide.linkUrl || null,
        sortOrder: Number(slide.sortOrder ?? 0),
      });
      if (!result.success || !result.data) {
        this.error =
          result.message ??
          this._localization.translate('modules.admin.sliderGroups.errors.saveFailed');
        return;
      }
      this.slides = this.slides
        .map((s) => (s.id === slide.id ? result.data! : s))
        .sort((a, b) => a.sortOrder - b.sortOrder);
      this._slideForms.delete(slide.id);
      this.resetSlideForm(result.data!);
      this.editingSlideId = null;
      this.success = this._localization.translate(
        'modules.admin.sliderGroups.success.slideSaved',
      );
    } finally {
      this.savingSlideId = null;
    }
  }

  async deleteSlide(slide: SliderSlideDto): Promise<void> {
    if (!confirm(this._localization.translate('modules.admin.sliderGroups.confirmDeleteSlide'))) {
      return;
    }

    const result = await this._sliderGroupsService.deleteSlide({
      id: slide.id,
      sliderGroupId: this.groupId,
    });
    if (!result.success) {
      this.error =
        result.message ??
        this._localization.translate('modules.admin.sliderGroups.errors.deleteFailed');
      return;
    }
    this.slides = this.slides.filter((s) => s.id !== slide.id);
    this._slideForms.delete(slide.id);
    if (this.editingSlideId === slide.id) this.editingSlideId = null;
  }

  async onSlideDrop(event: CdkDragDrop<SliderSlideDto[]>): Promise<void> {
    if (event.previousIndex === event.currentIndex || this.reordering) return;

    this.editingSlideId = null;
    const previous = this.slides.map((s) => ({ ...s }));

    moveItemInArray(this.slides, event.previousIndex, event.currentIndex);
    for (let i = 0; i < this.slides.length; i++) {
      this.slides[i].sortOrder = i;
    }
    // Fresh array so @for re-renders cleanly after CDK finishes DOM mutations.
    this.slides = this.slides.slice();

    this.reordering = true;
    this.error = '';
    try {
      const result = await this._sliderGroupsService.reorderSlides({
        sliderGroupId: this.groupId,
        slideIds: this.slides.map((s) => s.id),
      });
      if (!result.success) {
        this.slides = previous;
        this.error =
          result.message ??
          this._localization.translate('modules.admin.sliderGroups.errors.reorderFailed');
      }
    } catch (e: unknown) {
      this.slides = previous;
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.sliderGroups.errors.reorderFailed');
    } finally {
      this.reordering = false;
    }
  }

  /** Returns the slide form without overwriting user edits on each change-detection pass. */
  private getSlideForm(slide: SliderSlideDto): FormGroup {
    let form = this._slideForms.get(slide.id);
    if (!form) {
      form = this.createSlideForm(slide);
      this._slideForms.set(slide.id, form);
    }
    return form;
  }

  private createSlideForm(slide: SliderSlideDto): FormGroup {
    return new FormGroup({
      title: new FormControl(slide.title ?? ''),
      linkUrl: new FormControl(slide.linkUrl ?? ''),
      sortOrder: new FormControl(slide.sortOrder ?? 0),
    });
  }

  /** Sync form values from the slide model when opening edit or after server refresh. */
  private resetSlideForm(slide: SliderSlideDto): FormGroup {
    const form = this.createSlideForm(slide);
    this._slideForms.set(slide.id, form);
    return form;
  }
}
