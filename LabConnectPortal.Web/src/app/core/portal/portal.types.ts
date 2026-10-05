export type PortalKind = 'lab' | 'store' | 'user';

export function isStoreCenterType(centerType: unknown): boolean {
  if (centerType == null || centerType === '') {
    return false;
  }
  const value = String(centerType).trim().toLowerCase();
  return value === 'store' || value === '1';
}
