import { Routes } from '@angular/router';
import { BREADCRUMB_DATA_KEY } from '@core/services/breadcrumb/breadcrumb.types';
import { ContentPostType } from '../admin/content/content.types';

function typedPostsRoutes(contentType: ContentPostType, navKey: string, listUrl: string): Routes {
  return [
    {
      path: '',
      data: {
        contentType,
        titleKey: navKey,
        [BREADCRUMB_DATA_KEY]: navKey,
      },
      loadComponent: () => import('./posts-list.component').then((m) => m.PostsListComponent),
    },
    {
      path: ':id',
      data: {
        contentType,
        titleKey: navKey,
        listUrl,
        [BREADCRUMB_DATA_KEY]: [{ i18nKey: navKey, url: listUrl }, { dynamic: true }],
      },
      loadComponent: () => import('./post-detail.component').then((m) => m.PostDetailComponent),
    },
  ];
}

/** Legacy all-types list (kept for links/compat). */
export const postsRoutes: Routes = [
  {
    path: '',
    data: { [BREADCRUMB_DATA_KEY]: 'modules.home.nav.posts' },
    loadComponent: () => import('./posts-list.component').then((m) => m.PostsListComponent),
  },
  {
    path: ':id',
    data: {
      [BREADCRUMB_DATA_KEY]: [
        { i18nKey: 'modules.home.nav.posts', url: '/posts' },
        { dynamic: true },
      ],
    },
    loadComponent: () => import('./post-detail.component').then((m) => m.PostDetailComponent),
  },
];

export const newsRoutes: Routes = typedPostsRoutes(
  ContentPostType.News,
  'modules.home.nav.news',
  '/news',
);

export const articlesRoutes: Routes = typedPostsRoutes(
  ContentPostType.Articles,
  'modules.home.nav.articles',
  '/articles',
);

export const documentsRoutes: Routes = typedPostsRoutes(
  ContentPostType.Documents,
  'modules.home.nav.documents',
  '/documents',
);
