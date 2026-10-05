import { Component, OnInit, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SeoService } from '@core/services/site/seo.service';
import { PublicPostDetailDto, ContentGroupDto, contentPostListPath } from '../admin/content/content.types';
import { PublicPostsService } from './public-posts.service';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  imports: [TranslocoPipe, RouterLink],
  templateUrl: './post-detail.component.html',
  styleUrl: './post-detail.component.scss',
})
export class PostDetailComponent implements OnInit {
  private _sanitizer = inject(DomSanitizer);

  loading = false;
  error = '';
  post: PublicPostDetailDto | null = null;
  groups: ContentGroupDto[] = [];
  listUrl = '/posts';

  constructor(
    private _route: ActivatedRoute,
    private _publicPostsService: PublicPostsService,
    private _seoService: SeoService,
    private _localization: LocalizationService,
  ) {}

  ngOnInit(): void {
    const rawType = this._route.snapshot.data['contentType'];
    const routeListUrl = this._route.snapshot.data['listUrl'] as string | undefined;
    if (typeof rawType === 'number' && rawType >= 1 && rawType <= 3) {
      this.listUrl = routeListUrl ?? contentPostListPath(rawType);
    } else if (typeof rawType === 'string') {
      this.listUrl = routeListUrl ?? contentPostListPath(rawType);
    } else if (routeListUrl) {
      this.listUrl = routeListUrl;
    }

    void this.loadGroups();
    this._route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (!id) return;
      void this.load(id);
    });
  }

  async loadGroups(): Promise<void> {
    try {
      const result = await this._publicPostsService.getPublishedGroups();
      if (result.success && result.data) {
        this.groups = result.data;
      }
    } catch {
      // Sidebar groups are optional; post detail still renders.
    }
  }

  async load(contentPostId: string): Promise<void> {
    this.loading = true;
    this.error = '';
    this.post = null;
    try {
      const result = await this._publicPostsService.getById(contentPostId);
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.posts.errors.loadFailed'));
      }

      if (result.data.externalLink?.trim()) {
        window.location.href = result.data.externalLink.trim();
        return;
      }

      this.post = result.data;
      if (result.data.type) {
        this.listUrl = contentPostListPath(result.data.type);
      }
      this._seoService.applyPost(result.data);
      void this._publicPostsService.recordView(contentPostId);
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.posts.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  featuredImageUrl(): string {
    return this.post ? PublicPostsService.featuredImageUrl(this.post.featuredImagePath) : '';
  }

  formattedDate(): string {
    if (!this.post?.publishedAt) return '';
    const parts = new Intl.DateTimeFormat('fa-IR-u-nu-latn', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).formatToParts(new Date(this.post.publishedAt));
    const day = parts.find((p) => p.type === 'day')?.value ?? '';
    const month = parts.find((p) => p.type === 'month')?.value ?? '';
    const year = parts.find((p) => p.type === 'year')?.value ?? '';
    return `${day} ${month} ${year}`.trim();
  }

  hasSummary(): boolean {
    return !!this.post?.shortDescription?.trim();
  }

  hasFullBody(): boolean {
    const html = this.post?.fullBody ?? '';
    const text = html.replace(/<[^>]*>/g, '').replace(/&nbsp;/g, ' ').trim();
    return text.length > 0;
  }

  safeBody(): SafeHtml {
    const html = this.post?.fullBody ?? '';
    return this._sanitizer.bypassSecurityTrustHtml(html);
  }
}