export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/** Matches API ContentPostType enum. */
export const ContentPostType = {
  News: 1,
  Articles: 2,
  Documents: 3,
} as const;

export type ContentPostType = (typeof ContentPostType)[keyof typeof ContentPostType];

export type ContentPostTypeValue = ContentPostType | `${keyof typeof ContentPostType}` | string | number;

export const CONTENT_POST_TYPE_SLUGS: Record<ContentPostType, string> = {
  [ContentPostType.News]: 'news',
  [ContentPostType.Articles]: 'articles',
  [ContentPostType.Documents]: 'documents',
};

export function normalizeContentPostType(
  type: ContentPostTypeValue | null | undefined,
): ContentPostType {
  switch (type) {
    case ContentPostType.News:
    case 'News':
    case 1:
    case '1':
      return ContentPostType.News;
    case ContentPostType.Articles:
    case 'Articles':
    case 2:
    case '2':
      return ContentPostType.Articles;
    case ContentPostType.Documents:
    case 'Documents':
    case 3:
    case '3':
      return ContentPostType.Documents;
    default:
      return ContentPostType.News;
  }
}

export function contentPostTypeFromSlug(slug: string | null | undefined): ContentPostType | null {
  switch ((slug ?? '').toLowerCase()) {
    case 'news':
      return ContentPostType.News;
    case 'articles':
      return ContentPostType.Articles;
    case 'documents':
      return ContentPostType.Documents;
    default:
      return null;
  }
}

export function contentPostListPath(type: ContentPostTypeValue | null | undefined): string {
  return `/${CONTENT_POST_TYPE_SLUGS[normalizeContentPostType(type)]}`;
}

export function contentPostDetailCommands(
  type: ContentPostTypeValue | null | undefined,
  contentPostId: string,
): string[] {
  return [contentPostListPath(type), contentPostId];
}

export interface ContentGroupDto {
  contentGroupId: number;
  code?: string | null;
  title: string;
}

export interface SaveContentGroupCommand {
  code?: string | null;
  title: string;
}

export interface UpdateContentGroupCommand extends SaveContentGroupCommand {
  contentGroupId: number;
}

export interface GetContentGroupsQuery {
  title?: string;
  page?: number;
  pageSize?: number;
}

export interface ContentPostListItemDto {
  contentPostId: string;
  contentGroupId?: number | null;
  groupTitle: string;
  type: ContentPostTypeValue;
  typeTitle: string;
  title: string;
  isActive: boolean;
  showOnHomePage: boolean;
  publishedAt?: string | null;
  viewCount: number;
  hasFeaturedImage: boolean;
  featuredImagePath?: string | null;
}

export interface ContentPostDto {
  contentPostId: string;
  contentGroupId?: number | null;
  groupTitle: string;
  type: ContentPostTypeValue;
  typeTitle: string;
  title: string;
  shortDescription: string;
  fullBody: string;
  featuredImagePath?: string | null;
  hasFeaturedImage?: boolean;
  isActive: boolean;
  showOnHomePage: boolean;
  showAuthor: boolean;
  authorName?: string | null;
  publishedAt?: string | null;
  browserTitle?: string | null;
  metaKeywords?: string | null;
  metaDescription?: string | null;
  customMetaTags?: string | null;
  externalLink?: string | null;
  viewCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface SaveContentPostCommand {
  contentGroupId?: number | null;
  type: ContentPostTypeValue;
  title: string;
  shortDescription: string;
  fullBody: string;
  isActive: boolean;
  showOnHomePage: boolean;
  showAuthor: boolean;
  authorName?: string | null;
  publishedAt?: string | null;
  browserTitle?: string | null;
  metaKeywords?: string | null;
  metaDescription?: string | null;
  customMetaTags?: string | null;
  externalLink?: string | null;
  viewCount: number;
}

export interface UpdateContentPostCommand extends SaveContentPostCommand {
  contentPostId: string;
}

export interface GetContentPostsQuery {
  title?: string;
  contentGroupId?: number;
  type?: ContentPostType;
  page?: number;
  pageSize?: number;
}

export interface GetPublishedPostsQuery {
  contentGroupId?: number;
  type?: ContentPostType;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface PublicPostCardDto {
  contentPostId: string;
  contentGroupId?: number | null;
  groupTitle: string;
  type: ContentPostTypeValue;
  typeTitle: string;
  title: string;
  shortDescription: string;
  featuredImagePath?: string | null;
  showAuthor: boolean;
  authorName?: string | null;
  publishedAt?: string | null;
  viewCount: number;
  externalLink?: string | null;
}

export interface PublicPostDetailDto extends PublicPostCardDto {
  fullBody: string;
  browserTitle?: string | null;
  metaKeywords?: string | null;
  metaDescription?: string | null;
  customMetaTags?: string | null;
}
