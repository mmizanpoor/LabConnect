import {
  Component,
  EventEmitter,
  Input,
  Output,
  booleanAttribute,
  forwardRef,
} from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'base-checkbox',
  standalone: true,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => BaseCheckboxComponent),
      multi: true,
    },
  ],
  template: `
    <label
      class="base-checkbox"
      [class.base-checkbox--checked]="checked && !indeterminate"
      [class.base-checkbox--indeterminate]="indeterminate"
      [class.base-checkbox--disabled]="disabled"
    >
      <input
        type="checkbox"
        class="base-checkbox__input"
        [checked]="checked"
        [disabled]="disabled"
        [attr.aria-checked]="indeterminate ? 'mixed' : checked"
        (change)="onInputChange($event)"
        (blur)="onTouched()"
      />
      <span class="base-checkbox__box" aria-hidden="true">
        @if (indeterminate) {
          <span class="base-checkbox__dash"></span>
        } @else {
          <svg
            class="base-checkbox__check"
            viewBox="0 0 16 16"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M3.5 8.2L6.4 11.1L12.5 4.9"
              stroke="currentColor"
              stroke-width="2.2"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
        }
      </span>
      <span class="base-checkbox__label">
        <ng-content></ng-content>
      </span>
    </label>
  `,
  styleUrl: './base-checkbox.component.scss',
})
export class BaseCheckboxComponent implements ControlValueAccessor {
  @Input({ transform: booleanAttribute }) disabled = false;
  @Input({ transform: booleanAttribute }) indeterminate = false;

  @Input()
  set checked(value: boolean | null | undefined) {
    this._checked = !!value;
  }
  get checked(): boolean {
    return this._checked;
  }

  @Output() checkedChange = new EventEmitter<boolean>();

  private _checked = false;
  private _onChange: (value: boolean) => void = () => undefined;
  onTouched: () => void = () => undefined;

  writeValue(value: boolean | null): void {
    this._checked = !!value;
  }

  registerOnChange(fn: (value: boolean) => void): void {
    this._onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this._checked = input.checked;
    this.indeterminate = false;
    this._onChange(this._checked);
    this.checkedChange.emit(this._checked);
  }
}
