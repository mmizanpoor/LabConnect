import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { ContentService } from '../content.service';
import { ContentGroupDto, ContentPostListItemDto, ContentPostType } from '../content.types';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PostsColDef } from './posts.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SitePermissionService } from '@core/services/auth/site-permission.service';

@Component({
  selector: 'app-posts',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './posts.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }

    :host ::ng-deep .mat-drawer-container {
      height: 100%;
    }
  `,
})
export class PostsComponent implements OnInit {
  readonly entity =
    (inject(ActivatedRoute).snapshot.data['systemEntity'] as string | undefined) ?? SystemEntity.Post;
  private readonly _systemEntityBinding = bindSystemEntity(this.entity);
  private readonly _route = inject(ActivatedRoute);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;
  private readonly _destroyRef = inject(DestroyRef);
  loading = false;
  error = '';
  rows: ContentPostListItemDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ContentPostListItemDto[]>([]);
  groups: ContentGroupDto[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  listLink = '/admin/posts';
  fixedType: ContentPostType | null = null;

  titleControl = new FormControl('');
  groupControl = new FormControl<number | null>(null);

  constructor(
    private _contentService: ContentService,
    private _router: Router,
    private _localization: LocalizationService,
    private _colDef: PostsColDef,
    private _sitePermission: SitePermissionService,
  ) {}

  get canCreate(): boolean {
    return this._sitePermission.can(this.entity, 'create');
  }

  get canUpdate(): boolean {
    return this._sitePermission.can(this.entity, 'update');
  }

  get canDelete(): boolean {
    return this._sitePermission.can(this.entity, 'delete');
  }

  get showGroupFilter(): boolean {
    return this.fixedType == null;
  }

  ngOnInit(): void {
    this.listLink = (this._route.snapshot.data['listLink'] as string | undefined) ?? '/admin/posts';
    const rawType = this._route.snapshot.data['contentType'];
    this.fixedType =
      typeof rawType === 'number' && rawType >= 1 && rawType <= 3 ? (rawType as ContentPostType) : null;

    this.colDef = this._colDef.getForScope({
      showType: this.fixedType == null,
      showGroup: this.fixedType == null,
    });
    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt) => {
        if (!evt?.row) return;
        if (evt.type === 'edit') {
          if (!this.canUpdate) return;
          this.openEdit(evt.row);
        }
        if (evt.type === 'delete') {
          if (!this.canDelete) return;
          void this.deletePost(evt.row);
        }
      });
    void this.load();
  }

  get groupFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: null, label: this._localization.translate('shared.all') },
      ...this.groups.map((group) => ({
        value: group.contentGroupId,
        label: group.title,
      })),
    ];
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const [postsResult, groupsResult] = await Promise.all([
        this._contentService.getPosts({
          title: this.titleControl.value || undefined,
          contentGroupId: this.showGroupFilter ? (this.groupControl.value ?? undefined) : undefined,
          type: this.fixedType ?? undefined,
          page: this.page,
          pageSize: this.pageSize,
        }),
        this.showGroupFilter ? this._contentService.getGroupOptions() : Promise.resolve({ success: true, data: [] as ContentGroupDto[] }),
      ]);

      if (!postsResult.success || !postsResult.data) {
        throw new Error(
          postsResult.message ?? this._localization.translate('modules.admin.posts.errors.loadFailed'),
        );
      }

      this.rows = postsResult.data.items;
      this.totalCount = postsResult.data.totalCount;
      this.list$.next(this.rows);
      if (groupsResult.success && groupsResult.data) {
        this.groups = groupsResult.data;
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.admin.posts.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  applyFilters(): void {
    this.page = 1;
    void this.filterDrawer?.close();
    void this.load();
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
  }

  openCreate(): void {
    if (!this.canCreate) return;
    void this._router.navigate([this.listLink, 'new']);
  }

  openEdit(row: ContentPostListItemDto): void {
    if (!this.canUpdate) return;
    void this._router.navigate([this.listLink, row.contentPostId]);
  }

  async deletePost(row: ContentPostListItemDto): Promise<void> {
    if (!this.canDelete) return;
    const message = this._localization.translate('modules.admin.posts.confirmDelete', { title: row.title });
    if (!confirm(message)) return;

    const result = await this._contentService.deletePost(row.contentPostId);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.admin.posts.errors.deleteFailed');
      return;
    }
    await this.load();
  }
}
