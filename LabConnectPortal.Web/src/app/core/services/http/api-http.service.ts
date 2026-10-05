import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '@env/environment';
import { OperationResult } from '@core/types/operation-result.types';

@Injectable({ providedIn: 'root' })
export class ApiHttpService {
  constructor(private _http: HttpClient) {}

  private url(controller: string, action: string): string {
    return `${environment.apiUrl}${controller}/${action}`;
  }

  async post<T>(
    controller: string,
    action: string,
    body: unknown,
    params?: Record<string, string | number>,
  ): Promise<OperationResult<T>> {
    const result = await firstValueFrom(
      this._http.post<OperationResult<T>>(this.url(controller, action), body, {
        params: params as Record<string, string>,
      }),
    );
    return this.normalize(result);
  }

  async get<T>(controller: string, action: string, params?: Record<string, string | number>): Promise<OperationResult<T>> {
    const result = await firstValueFrom(
      this._http.get<OperationResult<T>>(this.url(controller, action), { params: params as Record<string, string> })
    );
    return this.normalize(result);
  }

  async delete<T>(controller: string, action: string, params?: Record<string, string | number>): Promise<OperationResult<T>> {
    const result = await firstValueFrom(
      this._http.delete<OperationResult<T>>(this.url(controller, action), { params: params as Record<string, string> })
    );
    return this.normalize(result);
  }

  async postForm<T>(
    controller: string,
    action: string,
    formData: FormData,
    params?: Record<string, string | number>,
  ): Promise<OperationResult<T>> {
    const result = await firstValueFrom(
      this._http.post<OperationResult<T>>(this.url(controller, action), formData, {
        params: params as Record<string, string>,
      }),
    );
    return this.normalize(result);
  }

  async getBlob(controller: string, action: string, params?: Record<string, string | number>): Promise<Blob> {
    const response = await firstValueFrom(
      this._http.get(this.url(controller, action), {
        params: params as Record<string, string>,
        responseType: 'blob',
        observe: 'response',
      }),
    );

    const body = response.body;
    if (!body || body.size === 0) {
      throw { status: response.status || 404 };
    }

    if (body.type.includes('json')) {
      const text = await body.text();
      let message: string | undefined;
      try {
        message = JSON.parse(text)?.message;
      } catch {
        message = undefined;
      }
      throw { status: response.status, message };
    }

    return body;
  }

  private normalize<T>(result: OperationResult<T>): OperationResult<T> {
    return {
      ...result,
      success: result.success ?? result.status,
      status: result.status ?? result.success,
    };
  }
}
