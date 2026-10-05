import { booleanAttribute, Component, Input } from '@angular/core';

@Component({
  selector: 'app-resume-info-item',
  standalone: true,
  host: { class: 'block' },
  template: `
    <div
      class="rounded-[5px] bg-slate-50"
      [class.px-4]="!compact"
      [class.py-3]="!compact"
      [class.px-3]="compact"
      [class.py-2]="compact"
    >
      <p
        class="text-slate-500"
        [class.text-xs]="!compact"
        [class.text-[0.6875rem]]="compact"
      >
        {{ label }}
      </p>
      <div
        class="mt-1 break-words font-medium text-slate-800"
        [class.text-sm]="compact"
      >
        <ng-content />
      </div>
    </div>
  `,
})
export class ResumeInfoItemComponent {
  @Input({ required: true }) label = '';
  @Input({ transform: booleanAttribute }) compact = false;
}
