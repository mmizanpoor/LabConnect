import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { firstValueFrom } from 'rxjs';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseConfirmDialogComponent } from '@modules/base/components/base-dialog/base-confirm-dialog.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { ApiKeyCreateDialogComponent } from './api-key-create-dialog.component';
import { ApiKeysService } from './api-keys.service';
import { ApiKeyDto } from './api-keys.types';

@Component({
  selector: 'app-api-keys-list',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatIconModule,
    RouterLink,
    TranslocoPipe,
    BaseButtonComponent,
  ],
  template: `
    <section class="mx-auto flex h-full min-h-0 w-full flex-col p-4" dir="rtl">
      <div class="mb-4 flex shrink-0 flex-wrap items-center justify-between gap-3">
        <div>
          <h1 class="m-0 text-xl font-bold text-slate-800">
            {{ (adminMode ? 'shared.apiKeys.adminTitle' : 'shared.apiKeys.title') | transloco }}
          </h1>
          <p class="mb-0 mt-1 text-sm text-slate-500">{{ 'shared.apiKeys.description' | transloco }}</p>
        </div>
        @if (!adminMode) {
          <div class="flex flex-wrap items-center gap-2">
            <a
              routerLink="/profile/api-keys/guide"
              class="inline-flex h-9 cursor-pointer items-center justify-center gap-2 rounded-md border-0 bg-blue-600 px-4 text-sm font-medium text-white no-underline transition hover:bg-blue-700"
            >
              <mat-icon class="!h-5 !w-5 !text-[1.25rem]">menu_book</mat-icon>
              {{ 'shared.apiKeys.guide' | transloco }}
            </a>
            <div class="w-36">
              <base-button size="sm" color="green" (click)="openCreate()">
                {{ 'shared.apiKeys.create' | transloco }}
              </base-button>
            </div>
          </div>
        }
      </div>

      <div class="min-h-0 flex-1 overflow-auto rounded-md border border-slate-200 bg-white shadow-sm">
        <table class="w-full min-w-[760px] border-collapse text-sm">
          <thead class="sticky top-0 bg-slate-50 text-slate-600">
            <tr>
              @if (adminMode) { <th class="border-b p-3 text-right">{{ 'shared.apiKeys.center' | transloco }}</th> }
              <th class="border-b p-3 text-right">{{ 'shared.apiKeys.name' | transloco }}</th>
              <th class="border-b p-3 text-right">{{ 'shared.apiKeys.key' | transloco }}</th>
              <th class="border-b p-3 text-center">{{ 'shared.apiKeys.permissions' | transloco }}</th>
              <th class="border-b p-3 text-center">{{ 'shared.apiKeys.createdAt' | transloco }}</th>
              <th class="w-28 border-b p-3 text-center">{{ 'shared.actions' | transloco }}</th>
            </tr>
          </thead>
          <tbody>
            @for (item of items; track item.id) {
              <tr class="border-b border-slate-100 last:border-0 hover:bg-slate-50">
                @if (adminMode) { <td class="p-3 font-medium text-slate-800">{{ item.centerName }}</td> }
                <td class="p-3 text-slate-800">{{ item.name }}</td>
                <td class="p-3" dir="ltr"><code class="text-xs text-slate-600">{{ item.keyName }}</code></td>
                <td class="p-3 text-center text-slate-600">{{ permissionLabel(item) }}</td>
                <td class="p-3 text-center text-slate-600" dir="ltr">{{ formatJalaliDateTime(item.createDate) }}</td>
                <td class="p-3 text-center">
                  <button
                    type="button"
                    class="ml-2 inline-flex h-9 w-9 cursor-pointer items-center justify-center rounded-md border-0 bg-blue-50 text-blue-600 hover:bg-blue-100"
                    (click)="openEdit(item)"
                    [attr.aria-label]="'shared.edit' | transloco"
                  >
                    <mat-icon class="!h-5 !w-5 !text-[1.25rem]">edit</mat-icon>
                  </button>
                  <button
                    type="button"
                    class="inline-flex h-9 w-9 cursor-pointer items-center justify-center rounded-md border-0 bg-red-50 text-red-600 hover:bg-red-100"
                    (click)="deleteItem(item)"
                    [attr.aria-label]="'shared.delete' | transloco"
                  >
                    <mat-icon class="!h-5 !w-5 !text-[1.25rem]">delete_outline</mat-icon>
                  </button>
                </td>
              </tr>
            } @empty {
              <tr>
                <td [attr.colspan]="adminMode ? 6 : 5" class="p-10 text-center text-slate-500">
                  {{ (loading ? 'shared.loading' : 'shared.apiKeys.empty') | transloco }}
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    </section>
  `,
})
export class ApiKeysListComponent implements OnInit {
  readonly adminMode: boolean;
  items: ApiKeyDto[] = [];
  loading = false;

  constructor(
    route: ActivatedRoute,
    private _apiKeys: ApiKeysService,
    private _dialog: MatDialog,
    private _localization: LocalizationService,
  ) {
    this.adminMode = !!route.snapshot.data['adminMode'];
  }

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    try {
      const result = this.adminMode ? await this._apiKeys.getAll() : await this._apiKeys.getMine();
      this.items = result.success && result.data ? result.data : [];
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    const ref = this._dialog.open(ApiKeyCreateDialogComponent, {
      width: '500px',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
    });
    ref.afterClosed().subscribe((created) => {
      if (created) void this.load();
    });
  }

  openEdit(item: ApiKeyDto): void {
    const ref = this._dialog.open(ApiKeyCreateDialogComponent, {
      width: '500px',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { item, adminMode: this.adminMode },
    });
    ref.afterClosed().subscribe((updated) => {
      if (updated) void this.load();
    });
  }

  async deleteItem(item: ApiKeyDto): Promise<void> {
    const ref = this._dialog.open(BaseConfirmDialogComponent, {
      width: '440px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        title: this._localization.translate('shared.apiKeys.deleteTitle'),
        message: this._localization.translate('shared.apiKeys.confirmDelete'),
        confirmLabel: this._localization.translate('shared.delete'),
        premium: true,
        icon: 'key_off',
      },
    });
    if (!(await firstValueFrom(ref.afterClosed()))) return;

    const result = this.adminMode
      ? await this._apiKeys.delete(item.id)
      : await this._apiKeys.deleteMine(item.id);
    if (result.success) await this.load();
  }

  permissionLabel(item: ApiKeyDto): string {
    const permissions: string[] = [];
    if (item.allowView) permissions.push(this._localization.translate('shared.apiKeys.allowView'));
    if (item.allowAdd) permissions.push(this._localization.translate('shared.apiKeys.allowAdd'));
    if (item.allowEdit) permissions.push(this._localization.translate('shared.apiKeys.allowEdit'));
    return permissions.length
      ? permissions.join('، ')
      : this._localization.translate('shared.apiKeys.noAccess');
  }

  formatJalaliDateTime(value: string | null | undefined): string {
    if (!value?.trim()) return '—';
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid()
      ? `\u2066${parsed.format('jYYYY/jMM/jDD HH:mm')}\u2069`
      : value.trim();
  }
}
