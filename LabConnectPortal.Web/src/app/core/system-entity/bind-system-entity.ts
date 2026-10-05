import { DestroyRef, inject } from '@angular/core';
import { SystemEntityContextService } from './system-entity-context.service';

/**
 * Registers the page SystemEntity for API headers and clears it on destroy.
 * Must be called from an injection context (constructor or field initializer).
 */
export function bindSystemEntity(entity: string): true {
  const destroyRef = inject(DestroyRef);
  const context = inject(SystemEntityContextService);
  context.set(entity);
  destroyRef.onDestroy(() => context.clearIf(entity));
  return true;
}
