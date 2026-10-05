import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
  Injector,
  OnInit,
  ViewChild,
  afterNextRender,
  inject,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { ContentGroupDto, ContentPostType, PublicPostCardDto } from '../admin/content/content.types';
import { PostCardComponent } from './post-card.component';
import { PublicPostsService } from './public-posts.service';

@Component({
  selector: 'app-posts-list',
  standalone: true,
  imports: [ReactiveFormsModule, TranslocoPipe, MatIconModule, PostCardComponent],
  templateUrl: './posts-list.component.html',
  styleUrl: './posts-list.component.scss',
})
export class PostsListComponent implements OnInit, AfterViewInit {
  private _publicPostsService = inject(PublicPostsService);
  private _localization = inject(LocalizationService);
  private _route = inject(ActivatedRoute);
  private _router = inject(Router);
  private _destroyRef = inject(DestroyRef);
  private _injector = inject(Injector);

  @ViewChild('loadMoreSentinel') private _loadMoreSentinel?: ElementRef<HTMLElement>;
  @ViewChild('groupFilter') private _groupFilter?: ElementRef<HTMLElement>;

  loading = false;
  loadingMore = false;
  groupsLoading = false;
  groupFilterOpen = false;
  error = '';
  posts: PublicPostCardDto[] = [];
  groups: ContentGroupDto[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 15;
  contentGroupId?: number;
  groupTitle = '';
  searchTerm = '';
  fixedType: ContentPostType | null = null;
  titleKey = 'modules.posts.title';
  searchInput = new FormControl('', { nonNullable: true });
  private _infiniteScrollObserver?: IntersectionObserver;
  private readonly _scrollPrefetchRows = 5;
  private readonly _estimatedPostRowHeightPx = 320;

  get selectedGroupLabel(): string {
    if (!this.contentGroupId) {
      return this._localization.translate('modules.posts.allGroups');
    }
    return (
      this.groupTitle ||
      this.groups.find((g) => g.contentGroupId === this.contentGroupId)?.title ||
      this._localization.translate('modules.posts.allGroups')
    );
  }

  ngOnInit(): void {
    const rawType = this._route.snapshot.data['contentType'];
    this.fixedType =
      typeof rawType === 'number' && rawType >= 1 && rawType <= 3 ? (rawType as ContentPostType) : null;
    this.titleKey =
      (this._route.snapshot.data['titleKey'] as string | undefined) ?? 'modules.posts.title';

    void this.loadGroups();
    this._route.queryParamMap.pipe(takeUntilDestroyed(this._destroyRef)).subscribe((params) => {
      const groupParam = params.get('contentGroupId');
      const parsedGroupId = groupParam ? Number(groupParam) : undefined;
      this.contentGroupId = parsedGroupId && !Number.isNaN(parsedGroupId) ? parsedGroupId : undefined;
      this.groupTitle = params.get('groupTitle')?.trim() ?? '';
      this.searchTerm = params.get('search')?.trim() ?? '';
      this.searchInput.setValue(this.searchTerm, { emitEvent: false });
      this.syncGroupTitle();
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
        rootMargin: `0px 0px ${this._scrollPrefetchRows * this._estimatedPostRowHeightPx}px 0px`,
        threshold: 0,
      }
    );
    this.observeLoadMoreSentinel();
  }

  get hasMorePosts(): boolean {
    return this.posts.length < this.totalCount;
  }

  async loadGroups(): Promise<void> {
    this.groupsLoading = true;
    try {
      const result = await this._publicPostsService.getPublishedGroups();
      if (result.success && result.data) {
        this.groups = result.data;
        this.syncGroupTitle();
      }
    } catch {
      // Group filter is optional; posts list still renders.
    } finally {
      this.groupsLoading = false;
    }
  }

  async load(): Promise<void> {
    this.page = 1;
    await this.fetchPosts(true);
  }

  async loadMore(): Promise<void> {
    if (this.loading || this.loadingMore || !this.hasMorePosts) return;
    this.page += 1;
    await this.fetchPosts(false);
  }

  onSearch(event: Event): void {
    event.preventDefault();
    const term = this.searchInput.value.trim();
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: { search: term || null },
      queryParamsHandling: 'merge',
    });
  }

  toggleGroupFilter(): void {
    if (this.groupsLoading) return;
    this.groupFilterOpen = !this.groupFilterOpen;
  }

  selectGroup(groupId: number | null): void {
    this.groupFilterOpen = false;
    const group = groupId ? this.groups.find((item) => item.contentGroupId === groupId) : undefined;
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: groupId
        ? { contentGroupId: groupId, groupTitle: group?.title ?? null }
        : { contentGroupId: null, groupTitle: null },
      queryParamsHandling: 'merge',
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.groupFilterOpen) return;
    const el = this._groupFilter?.nativeElement;
    if (el && !el.contains(event.target as Node)) {
      this.groupFilterOpen = false;
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.groupFilterOpen = false;
  }

  clearGroupFilter(): void {
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: { contentGroupId: null, groupTitle: null },
      queryParamsHandling: 'merge',
    });
  }

  clearSearchFilter(): void {
    this.searchInput.setValue('');
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: { search: null },
      queryParamsHandling: 'merge',
    });
  }

  clearAllFilters(): void {
    this.searchInput.setValue('');
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: {
        search: null,
        contentGroupId: null,
        groupTitle: null,
      },
    });
  }

  hasActiveFilters(): boolean {
    return !!(this.searchTerm || this.contentGroupId);
  }

  private async fetchPosts(reset: boolean): Promise<void> {
    if (reset) {
      this.loading = true;
      this.error = '';
    } else this.loadingMore = true;

    try {
      const result = await this._publicPostsService.getPublishedPosts({
        page: this.page,
        pageSize: this.pageSize,
        contentGroupId: this.contentGroupId,
        type: this.fixedType ?? undefined,
        search: this.searchTerm || undefined,
      });
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.posts.errors.loadFailed'));
      }

      if (reset) {
        this.posts = result.data.items;
      } else {
        const existingIds = new Set(this.posts.map((post) => post.contentPostId));
        const newItems = result.data.items.filter((post) => !existingIds.has(post.contentPostId));
        this.posts = [...this.posts, ...newItems];
      }

      this.totalCount = result.data.totalCount;
    } catch (e: unknown) {
      if (reset) {
        this.error =
          e instanceof Error ? e.message : this._localization.translate('modules.posts.errors.loadFailed');
      } else if (this.page > 1) this.page -= 1;
    } finally {
      this.loading = false;
      this.loadingMore = false;
      this.observeLoadMoreSentinel();
    }
  }

  private observeLoadMoreSentinel(): void {
    afterNextRender(
      () => {
        this._infiniteScrollObserver?.disconnect();
        const sentinel = this._loadMoreSentinel?.nativeElement;
        if (sentinel && this.hasMorePosts) this._infiniteScrollObserver?.observe(sentinel);
      },
      { injector: this._injector }
    );
  }

  private syncGroupTitle(): void {
    if (!this.contentGroupId || this.groupTitle) return;
    const group = this.groups.find((item) => item.contentGroupId === this.contentGroupId);
    if (group) this.groupTitle = group.title;
  }
}
