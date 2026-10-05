import { Injectable, inject } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { filter } from 'rxjs/operators';
import { BreadcrumbItem, BreadcrumbRouteEntry, BREADCRUMB_DATA_KEY } from './breadcrumb.types';

@Injectable({ providedIn: 'root' })
export class BreadcrumbService {
  private _router = inject(Router);

  private _items$ = new BehaviorSubject<BreadcrumbItem[]>([]);
  private _dynamicLabel: string | null = null;

  readonly items$ = this._items$.asObservable();

  constructor() {
    this._router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe(() => {
      this._dynamicLabel = null;
      this.rebuild();
    });

    queueMicrotask(() => this.rebuild());
  }

  setDynamicLabel(label: string): void {
    this._dynamicLabel = label.trim() || null;
    this.rebuild();
  }

  clearDynamicLabel(): void {
    this._dynamicLabel = null;
    this.rebuild();
  }

  private rebuild(): void {
    const collected = this.collect(this._router.routerState.root, '');
    if (!collected.length) {
      this._items$.next([]);
      return;
    }

    const items: BreadcrumbItem[] = [
      { i18nKey: 'modules.home.nav.home', url: '/home' },
      ...collected,
    ];

    if (items.length > 0) {
      items[items.length - 1].url = null;
    }

    this._items$.next(items);
  }

  private collect(route: ActivatedRoute, url: string): BreadcrumbItem[] {
    const items: BreadcrumbItem[] = [];

    if (!route.children?.length) {
      return items;
    }

    for (const child of route.children) {
      if (!child?.snapshot) {
        continue;
      }

      const urlSegments = child.snapshot.url ?? [];
      const segment = urlSegments.map((part) => part.path).join('/');
      const nextUrl = segment ? `${url}/${segment}`.replace(/\/+/g, '/') : url;
      const breadcrumb = child.snapshot.data[BREADCRUMB_DATA_KEY] as BreadcrumbRouteEntry | undefined;

      if (breadcrumb) {
        items.push(...this.mapBreadcrumbEntry(breadcrumb, nextUrl));
      }

      items.push(...this.collect(child, nextUrl));
    }

    return items;
  }

  private mapBreadcrumbEntry(entry: BreadcrumbRouteEntry, url: string): BreadcrumbItem[] {
    if (entry === 'skip') {
      return [];
    }

    if (entry === 'dynamic') {
      return [{ label: this._dynamicLabel ?? '…', url }];
    }

    if (Array.isArray(entry)) {
      return entry.map((item, index) => {
        if (item.dynamic) {
          return {
            label: this._dynamicLabel ?? '…',
            url: index === entry.length - 1 ? url : item.url ?? null,
          };
        }

        return {
          i18nKey: item.i18nKey,
          label: item.label,
          url: item.url ?? (index === entry.length - 1 ? url : item.url ?? null),
        };
      });
    }

    return [{ i18nKey: entry, url }];
  }
}
