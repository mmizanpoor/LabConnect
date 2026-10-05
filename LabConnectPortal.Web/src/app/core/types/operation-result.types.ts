export interface OperationResult<T = unknown> {
  status: boolean;
  success: boolean;
  message?: string;
  errors?: string[];
  data?: T;
}
