import { NgClass } from '@angular/common';
import { Component, DestroyRef, ElementRef, HostListener, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { filter } from 'rxjs';
import { PublicProductsService } from '@modules/products/products.service';
import {
  PublicCategoryFilterDto,
  PublicCategoryGroupFilterDto,
} from '@modules/products/products.types';

@Component({
  selector: 'app-header-category-menu',
  standalone: true,
  imports: [NgClass, RouterLink, MatIconModule, TranslocoPipe],
  templateUrl: './header-category-menu.component.html',
  host: { class: 'block self-stretch' },
})
export class HeaderCategoryMenuComponent implements OnInit {
  private _products = inject(PublicProductsService);
  private _router = inject(Router);
  private _destroyRef = inject(DestroyRef);
  private _elementRef = inject(ElementRef<HTMLElement>);

  isOpen = false;
  loading = false;
  loaded = false;
  groups: PublicCategoryGroupFilterDto[] = [];
  categories: PublicCategoryFilterDto[] = [];
  activeGroup: PublicCategoryGroupFilterDto | null = null;
  private _closeTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    this._router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this._destroyRef),
      )
      .subscribe(() => this.closeMenu());

    this._destroyRef.onDestroy(() => this.clearCloseTimer());
  }

  get activeCategories(): PublicCategoryFilterDto[] {
    if (!this.activeGroup) return [];
    return this.categories.filter(
      (category) => category.productCategoryGroupId === this.activeGroup?.productCategoryGroupId,
    );
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.isOpen) return;
    if (!this._elementRef.nativeElement.contains(event.target as Node)) {
      this.closeMenu();
    }
  }

  openMenu(): void {
    this.clearCloseTimer();
    this.isOpen = true;
    void this.ensureLoaded();
  }

  scheduleClose(): void {
    this.clearCloseTimer();
    this._closeTimer = setTimeout(() => this.closeMenu(), 150);
  }

  closeMenu(): void {
    this.clearCloseTimer();
    this.isOpen = false;
  }

  onTriggerClick(event: Event): void {
    if (window.matchMedia('(hover: hover) and (pointer: fine)').matches) return;
    event.preventDefault();
    event.stopPropagation();
    if (this.isOpen) this.closeMenu();
    else this.openMenu();
  }

  setActiveGroup(group: PublicCategoryGroupFilterDto): void {
    this.activeGroup = group;
  }

  groupQueryParams(group: PublicCategoryGroupFilterDto) {
    return {
      categoryGroupId: group.productCategoryGroupId,
      categoryGroupTitle: group.title,
    };
  }

  categoryQueryParams(category: PublicCategoryFilterDto) {
    const group =
      this.activeGroup && this.activeGroup.productCategoryGroupId === category.productCategoryGroupId
        ? this.activeGroup
        : this.groups.find((item) => item.productCategoryGroupId === category.productCategoryGroupId);

    return {
      categoryGroupId: group?.productCategoryGroupId ?? category.productCategoryGroupId,
      categoryGroupTitle: group?.title ?? category.categoryGroupTitle,
      categoryId: category.productCategoryId,
      categoryTitle: category.title,
    };
  }

  private async ensureLoaded(): Promise<void> {
    if (this.loaded || this.loading) return;
    this.loading = true;
    try {
      const result = await this._products.getListingFilters();
      if (result.success && result.data) {
        this.groups = result.data.categoryGroups ?? [];
        this.categories = result.data.categories ?? [];
        this.activeGroup = this.groups[0] ?? null;
      }
      this.loaded = true;
    } catch {
      this.groups = [];
      this.categories = [];
    } finally {
      this.loading = false;
    }
  }

  private clearCloseTimer(): void {
    if (this._closeTimer) {
      clearTimeout(this._closeTimer);
      this._closeTimer = undefined;
    }
  }
}
