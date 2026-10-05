import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { SiteSettingsService } from './site-settings.service';
import { SiteSettingsDto, SiteUsefulLinkDto } from './site-settings.types';
import { toUpdateSiteSettingsCommand } from './site-settings.mapper';
import {
  UsefulLinkFormDialogComponent,
  UsefulLinkFormDialogResult,
} from './useful-link-form-dialog.component';
import { DeleteUsefulLinkDialogComponent } from './delete-useful-link-dialog.component';
import { UsefulLinksColDef } from './useful-links.coldef';

@Component({
  selector: 'app-settings-useful-links',
  standalone: true,
  imports: [
    MatDialogModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseGridComponent,
  ],
  templateUrl: './settings-useful-links.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }
  `,
})
export class SettingsUsefulLinksComponent implements OnInit {
  readonly entity = SystemEntity.Settings;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Settings);
  private readonly _destroyRef = inject(DestroyRef);

  loading = false;
  saving = false;
  error = '';
  success = '';
  settings: SiteSettingsDto | null = null;
  rows: SiteUsefulLinkDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<SiteUsefulLinkDto[]>([]);

  constructor(
    private _siteSettingsService: SiteSettingsService,
    private _localization: LocalizationService,
    private _sitePermission: SitePermissionService,
    private _dialog: MatDialog,
    private _colDef: UsefulLinksColDef
  ) {}

  get canUpdate(): boolean {
    return this._sitePermission.can(this.entity, 'update');
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get(this.canUpdate);
    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt) => {
        if (evt?.type === 'delete' && evt.row) {
          void this.deleteLink(evt.row);
        }
      });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._siteSettingsService.get();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.settings.errors.loadFailed')
        );
      }
      this.settings = result.data;
      this.rows = [...(result.data.usefulLinks ?? [])];
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.settings.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  async openAddDialog(): Promise<void> {
    if (!this.canUpdate) return;
    this.error = '';
    this.success = '';

    const dialogRef = this._dialog.open(UsefulLinkFormDialogComponent, {
      width: '440px',
      maxWidth: '92vw',
      autoFocus: false,
      panelClass: BASE_DIALOG_PANEL_CLASS,
    });

    const result = (await firstValueFrom(dialogRef.afterClosed())) as
      | UsefulLinkFormDialogResult
      | undefined;
    if (!result) return;

    await this.persistLinks([
      ...this.rows,
      {
        id: '',
        title: result.title,
        url: result.url,
        sortOrder: this.rows.length,
      },
    ]);
    if (!this.error) {
      this.success = this._localization.translate(
        'modules.admin.settings.success.usefulLinkAdded'
      );
    }
  }

  async deleteLink(row: SiteUsefulLinkDto): Promise<void> {
    if (!this.canUpdate) return;

    const dialogRef = this._dialog.open(DeleteUsefulLinkDialogComponent, {
      width: '440px',
      maxWidth: '92vw',
      autoFocus: false,
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { title: row.title },
    });
    const confirmed = await firstValueFrom(dialogRef.afterClosed());
    if (!confirmed) return;

    this.error = '';
    this.success = '';
    const next = this.rows.filter((item) =>
      row.id ? item.id !== row.id : item !== row
    );
    await this.persistLinks(next);
    if (!this.error) {
      this.success = this._localization.translate(
        'modules.admin.settings.success.usefulLinkDeleted'
      );
    }
  }

  private async persistLinks(links: SiteUsefulLinkDto[]): Promise<void> {
    if (!this.settings) return;
    this.saving = true;
    try {
      const result = await this._siteSettingsService.update(
        toUpdateSiteSettingsCommand(this.settings, {
          usefulLinks: links.map((link, index) => ({
            id: link.id || null,
            title: link.title,
            url: link.url,
            sortOrder: index,
          })),
        })
      );
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.settings.errors.saveFailed')
        );
      }
      this.settings = result.data;
      this.rows = [...(result.data.usefulLinks ?? [])];
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.settings.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }
}
