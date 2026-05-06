import { Injectable, computed, signal } from '@angular/core';

const STORAGE_KEY = 'logmind.apiServerUrl';

@Injectable({ providedIn: 'root' })
export class ApiBaseUrlService {
  private readonly _url = signal<string>(this.read());

  readonly url = this._url.asReadonly();
  readonly isConfigured = computed(() => this._url().length > 0);

  set(url: string): void {
    const normalized = this.normalize(url);
    if (normalized.length === 0) {
      this.clear();
      return;
    }
    localStorage.setItem(STORAGE_KEY, normalized);
    this._url.set(normalized);
  }

  clear(): void {
    localStorage.removeItem(STORAGE_KEY);
    this._url.set('');
  }

  private read(): string {
    try {
      return this.normalize(localStorage.getItem(STORAGE_KEY) ?? '');
    } catch {
      return '';
    }
  }

  private normalize(value: string): string {
    return (value ?? '').trim().replace(/\/+$/, '');
  }
}
