import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SystemEntityContextService {
  private _current: string | null = null;

  get current(): string | null {
    return this._current;
  }

  set(entity: string): void {
    this._current = entity;
  }

  clearIf(entity: string): void {
    if (this._current === entity) {
      this._current = null;
    }
  }

  clear(): void {
    this._current = null;
  }
}
