import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'base-paging',
  standalone: true,
  imports: [MatIconModule, TranslocoPipe],
  template: `
    @if (totalPages > 1) {
      <div class="mt-5 flex flex-wrap items-center justify-between gap-3 rounded-xl border border-gray-100 bg-white px-4 py-3 shadow-sm" dir="rtl">
        <span class="text-sm font-medium text-gray-600">
          {{
            'paging.pageInfo'
              | transloco: { page: page, totalPages: totalPages, totalCount: totalCount }
          }}
        </span>
        <div class="flex items-center gap-2">
          <button
            type="button"
            class="inline-flex h-10 w-10 items-center justify-center rounded-full border border-gray-200 bg-white text-gray-600 shadow-sm transition-all duration-200 hover:border-gray-300 hover:bg-gray-50 hover:text-gray-900 active:scale-95 disabled:cursor-not-allowed disabled:border-gray-100 disabled:bg-gray-50 disabled:text-gray-300 disabled:shadow-none"
            [disabled]="page <= 1"
            (click)="changePage(page - 1)"
            [attr.aria-label]="'paging.previous' | transloco"
          >
            <mat-icon class="!h-5 !w-5 !text-[1.25rem]">chevron_right</mat-icon>
          </button>
          <span class="min-w-[3rem] text-center text-sm font-semibold text-gray-700">{{ page }}</span>
          <button
            type="button"
            class="inline-flex h-10 w-10 items-center justify-center rounded-full border border-gray-200 bg-white text-gray-600 shadow-sm transition-all duration-200 hover:border-gray-300 hover:bg-gray-50 hover:text-gray-900 active:scale-95 disabled:cursor-not-allowed disabled:border-gray-100 disabled:bg-gray-50 disabled:text-gray-300 disabled:shadow-none"
            [disabled]="page >= totalPages"
            (click)="changePage(page + 1)"
            [attr.aria-label]="'paging.next' | transloco"
          >
            <mat-icon class="!h-5 !w-5 !text-[1.25rem]">chevron_left</mat-icon>
          </button>
        </div>
      </div>
    }
  `,
})
export class BasePagingComponent {
  @Input() page = 1;
  @Input() pageSize = 20;
  @Input() totalCount = 0;
  @Output() pageChange = new EventEmitter<number>();

  get totalPages(): number {
    if (this.pageSize <= 0 || this.totalCount <= 0) return 0;
    return Math.ceil(this.totalCount / this.pageSize);
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) this.pageChange.emit(page);
  }
}
