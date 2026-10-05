import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseImageUploadComponent } from '@modules/base/components/base-image-upload/base-image-upload.component';
import { RichTextEditorComponent } from '@modules/base/components/rich-text-editor/rich-text-editor.component';
import { ContentService } from '../content.service';
import { ContentGroupDto, ContentPostDto, ContentPostType } from '../content.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

@Component({
  selector: 'app-post-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    BaseCheckboxComponent,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseImageUploadComponent,
    RichTextEditorComponent,
  ],
  templateUrl: './post-form.component.html',
})
export class PostFormComponent implements OnInit, OnDestroy {
  private readonly _route = inject(ActivatedRoute);

  readonly entity =
    (this._route.snapshot.data['systemEntity'] as string | undefined) ?? SystemEntity.Post;
  private readonly _systemEntityBinding = bindSystemEntity(this.entity);

  loading = false;
  saving = false;
  uploadingImage = false;
  error = '';
  isEdit = false;
  postId = '';
  listLink = '/admin/posts';
  fixedType: ContentPostType | null = null;
  groups: ContentGroupDto[] = [];
  imagePreviewUrl = '';
  imageFileName = '';
  private _pendingFile: File | null = null;
  private _objectPreviewUrl = '';
  private _existingPost: ContentPostDto | null = null;

  form = new FormGroup({
    contentGroupId: new FormControl<number | null>(null),
    title: new FormControl('', [Validators.required, Validators.maxLength(300)]),
    shortDescription: new FormControl('', Validators.maxLength(500)),
    fullBody: new FormControl(''),
    isActive: new FormControl(true, { nonNullable: true }),
    showOnHomePage: new FormControl(false, { nonNullable: true }),
    showAuthor: new FormControl(false, { nonNullable: true }),
    authorName: new FormControl('', Validators.maxLength(200)),
    publishedAtDate: new FormControl<Moment | null>(null),
    publishedAtTime: new FormControl(''),
    browserTitle: new FormControl('', Validators.maxLength(300)),
    metaKeywords: new FormControl('', Validators.maxLength(500)),
    metaDescription: new FormControl('', Validators.maxLength(500)),
    customMetaTags: new FormControl('', Validators.maxLength(2000)),
    externalLink: new FormControl('', Validators.maxLength(500)),
    viewCount: new FormControl(0, { nonNullable: true }),
  });

  constructor(
    private _router: Router,
    private _contentService: ContentService,
    private _localization: LocalizationService,
  ) {}

  /** Legacy /admin/posts requires a group. Typed menus do not use groups. */
  get requireGroup(): boolean {
    return this.fixedType == null;
  }

  get showGroupField(): boolean {
    return this.fixedType == null;
  }

  get groupOptions(): BaseFormSelectOption[] {
    return this.groups.map((group) => ({
      value: group.contentGroupId,
      label: group.title,
    }));
  }

  ngOnInit(): void {
    this.listLink = (this._route.snapshot.data['listLink'] as string | undefined) ?? '/admin/posts';
    const rawType = this._route.snapshot.data['contentType'];
    this.fixedType =
      typeof rawType === 'number' && rawType >= 1 && rawType <= 3 ? (rawType as ContentPostType) : null;

    if (this.requireGroup) {
      this.form.controls.contentGroupId.setValidators(Validators.required);
    } else {
      this.form.controls.contentGroupId.clearValidators();
      this.form.controls.contentGroupId.setValue(null);
    }
    this.form.controls.contentGroupId.updateValueAndValidity({ emitEvent: false });

    this.postId = this._route.snapshot.paramMap.get('id') ?? '';
    this.isEdit = this.postId !== '' && this.postId !== 'new';
    void this.load();
  }

  ngOnDestroy(): void {
    this.revokeObjectPreview();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      if (this.showGroupField) {
        const groupsResult = await this._contentService.getGroupOptions();
        if (!groupsResult.success) {
          throw new Error(
            groupsResult.message ?? this._localization.translate('modules.admin.posts.errors.loadGroupsFailed'),
          );
        }
        this.groups = groupsResult.data ?? [];

        if (this.requireGroup && !this.groups.length) {
          throw new Error(this._localization.translate('modules.admin.posts.errors.loadGroupsFailed'));
        }
      }

      if (this.isEdit) {
        const postResult = await this._contentService.getPostById(this.postId);
        if (!postResult.success || !postResult.data) {
          throw new Error(postResult.message ?? this._localization.translate('modules.admin.posts.errors.loadFailed'));
        }
        this._existingPost = postResult.data;
        this.patchForm(postResult.data);
      } else if (this.requireGroup && !this.form.controls.contentGroupId.value && this.groups[0]) {
        this.form.controls.contentGroupId.setValue(this.groups[0].contentGroupId);
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.admin.posts.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  patchForm(post: ContentPostDto): void {
    this.form.patchValue({
      contentGroupId: this.showGroupField ? (post.contentGroupId ?? null) : null,
      title: post.title,
      shortDescription: post.shortDescription,
      fullBody: post.fullBody,
      isActive: post.isActive,
      showOnHomePage: post.showOnHomePage,
      showAuthor: post.showAuthor,
      authorName: post.authorName ?? '',
      ...this.parsePublishedAt(post.publishedAt),
      browserTitle: post.browserTitle ?? '',
      metaKeywords: post.metaKeywords ?? '',
      metaDescription: post.metaDescription ?? '',
      customMetaTags: post.customMetaTags ?? '',
      externalLink: post.externalLink ?? '',
      viewCount: post.viewCount,
    });
    if (post.featuredImagePath) {
      this.imagePreviewUrl = this._contentService.getFeaturedImageUrl(post.featuredImagePath);
    }
  }

  onImageSelected(file: File): void {
    this.revokeObjectPreview();
    this._pendingFile = file;
    this.imageFileName = file.name;
    this._objectPreviewUrl = URL.createObjectURL(file);
    this.imagePreviewUrl = this._objectPreviewUrl;
  }

  async removeImage(): Promise<void> {
    if (this._pendingFile) {
      this._pendingFile = null;
      this.imageFileName = '';
      this.revokeObjectPreview();
      if (this._existingPost?.featuredImagePath) {
        this.imagePreviewUrl = this._contentService.getFeaturedImageUrl(this._existingPost.featuredImagePath);
      } else {
        this.imagePreviewUrl = '';
      }
      return;
    }

    if (!this.isEdit || !this._existingPost?.hasFeaturedImage) {
      this.imagePreviewUrl = '';
      return;
    }

    this.uploadingImage = true;
    this.error = '';
    try {
      const result = await this._contentService.deleteFeaturedImage(this.postId);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.posts.errors.uploadFailed'),
        );
      }
      this._existingPost = result.data;
      this.imagePreviewUrl = '';
      this.imageFileName = '';
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.admin.posts.errors.uploadFailed');
    } finally {
      this.uploadingImage = false;
    }
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = this._localization.translate('modules.admin.posts.errors.validationFailed');
      return;
    }
    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();
    const type = this.fixedType ?? this._existingPost?.type ?? ContentPostType.News;
    const command = {
      contentGroupId: this.showGroupField ? (value.contentGroupId ?? null) : null,
      type,
      title: value.title ?? '',
      shortDescription: value.shortDescription ?? '',
      fullBody: value.fullBody ?? '',
      isActive: value.isActive,
      showOnHomePage: value.showOnHomePage,
      showAuthor: value.showAuthor,
      authorName: value.authorName?.trim() || null,
      publishedAt: this.buildPublishedAtIso(value.publishedAtDate, value.publishedAtTime),
      browserTitle: value.browserTitle?.trim() || null,
      metaKeywords: value.metaKeywords?.trim() || null,
      metaDescription: value.metaDescription?.trim() || null,
      customMetaTags: value.customMetaTags?.trim() || null,
      externalLink: value.externalLink?.trim() || null,
      viewCount: value.viewCount,
    };

    try {
      const result = this.isEdit
        ? await this._contentService.updatePost({ ...command, contentPostId: this.postId })
        : await this._contentService.createPost(command);

      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.admin.posts.errors.saveFailed'));
      }

      let post = result.data;
      if (this._pendingFile) {
        this.uploadingImage = true;
        const uploadResult = await this._contentService.uploadFeaturedImage(post.contentPostId, this._pendingFile);
        this.uploadingImage = false;
        if (!uploadResult.success || !uploadResult.data) {
          throw new Error(
            uploadResult.message ?? this._localization.translate('modules.admin.posts.errors.uploadFailed'),
          );
        }
        post = uploadResult.data;
        this._pendingFile = null;
        this.revokeObjectPreview();
      }

      void this._router.navigate([this.listLink]);
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.admin.posts.errors.saveFailed');
    } finally {
      this.saving = false;
      this.uploadingImage = false;
    }
  }

  backToList(): void {
    void this._router.navigate([this.listLink]);
  }

  private parsePublishedAt(value?: string | null): {
    publishedAtDate: Moment | null;
    publishedAtTime: string;
  } {
    if (!value) {
      return { publishedAtDate: null, publishedAtTime: '' };
    }

    const parsed = jMoment(value).locale('fa');
    if (!parsed.isValid()) {
      return { publishedAtDate: null, publishedAtTime: '' };
    }

    const pad = (num: number) => String(num).padStart(2, '0');
    return {
      publishedAtDate: parsed.clone(),
      publishedAtTime: `${pad(parsed.hours())}:${pad(parsed.minutes())}`,
    };
  }

  private buildPublishedAtIso(date: Moment | null, time?: string | null): string | null {
    if (!date) return null;

    const [hours = 0, minutes = 0] = (time?.trim() || '00:00').split(':').map(Number);
    return date
      .clone()
      .hours(Number.isFinite(hours) ? hours : 0)
      .minutes(Number.isFinite(minutes) ? minutes : 0)
      .seconds(0)
      .milliseconds(0)
      .toISOString();
  }

  private revokeObjectPreview(): void {
    if (this._objectPreviewUrl) {
      URL.revokeObjectURL(this._objectPreviewUrl);
      this._objectPreviewUrl = '';
    }
  }
}
