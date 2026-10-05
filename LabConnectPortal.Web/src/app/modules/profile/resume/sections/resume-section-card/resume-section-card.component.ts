import { Component, Input, booleanAttribute } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'app-resume-section-card',
  standalone: true,
  imports: [MatIconModule, TranslocoPipe],
  host: { class: 'block' },
  template: `
    <section
      class="overflow-hidden rounded-[5px] border bg-white shadow-sm"
      [class.border-emerald-200]="complete"
      [class.border-amber-200]="!complete"
    >
      <div
        class="flex items-center justify-between gap-3 border-b px-4 py-2.5"
        [class.border-emerald-100]="complete"
        [class.bg-emerald-50]="complete"
        [class.border-amber-100]="!complete"
        [class.bg-amber-50]="!complete"
      >
        <div class="flex min-w-0 items-center gap-2.5">
          <div
            class="flex h-8 w-8 shrink-0 items-center justify-center rounded-[5px] bg-white"
            [class.text-teal-700]="complete"
            [class.text-amber-700]="!complete"
          >
            <mat-icon class="!h-[1.125rem] !w-[1.125rem] !text-[1.125rem]">{{ icon }}</mat-icon>
          </div>
          <h2 class="truncate text-sm font-bold text-slate-800">{{ title }}</h2>
        </div>
        <div class="flex shrink-0 items-center gap-2">
          <span
            class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[0.6875rem] font-medium"
            [class.bg-emerald-100]="complete"
            [class.text-emerald-800]="complete"
            [class.bg-amber-100]="!complete"
            [class.text-amber-800]="!complete"
          >
            <mat-icon class="icon-sm">{{ complete ? 'check_circle' : 'error_outline' }}</mat-icon>
            {{
              (complete
                ? 'modules.profile.resume.section.complete'
                : 'modules.profile.resume.section.incomplete'
              ) | transloco
            }}
          </span>
          <ng-content select="[sectionActions]" />
        </div>
      </div>
      <div class="p-4">
        <ng-content />
      </div>
    </section>
  `,
  styles: `
    .icon-sm {
      width: 0.875rem;
      height: 0.875rem;
      font-size: 14px;
    }
  `,
})
export class ResumeSectionCardComponent {
  @Input({ required: true }) icon = '';
  @Input({ required: true }) title = '';
  @Input({ transform: booleanAttribute }) complete = false;
}
