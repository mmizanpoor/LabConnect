import { ProductResumeApplicationStatus } from '../../products/products.types';

export function isPendingResumeApplication(
  status: ProductResumeApplicationStatus | null | undefined,
): boolean {
  return status == null || status === 'Pending' || status === 0;
}

export function resumeApplicationStatusKey(
  status: ProductResumeApplicationStatus | null | undefined,
): 'pending' | 'approved' | 'rejected' {
  if (status === 'Approved' || status === 1) return 'approved';
  if (status === 'Rejected' || status === 2) return 'rejected';
  return 'pending';
}
