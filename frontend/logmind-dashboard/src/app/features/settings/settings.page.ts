import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { SettingsApi } from '../../core/api/settings.api';
import { AgentSettingsDto, TestAgentSettingsResponse } from '../../core/models/settings';
import { ApiBaseUrlService } from '../../core/services/api-base-url.service';
import { SpinnerComponent } from '../../shared/components/spinner.component';
import { RelativeTimePipe } from '../../shared/pipes/relative-time.pipe';

const FALLBACK_MODELS = [
  'llama-3.3-70b-versatile',
  'llama-3.1-8b-instant',
  'llama3-70b-8192',
  'llama3-8b-8192',
  'mixtral-8x7b-32768',
  'gemma2-9b-it',
  'deepseek-r1-distill-llama-70b',
  'qwen-2.5-32b'
];
const DEFAULT_MODEL = 'llama-3.3-70b-versatile';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, FormsModule, SpinnerComponent, RelativeTimePipe],
  template: `
    <div class="shrink-0 mx-auto w-full max-w-3xl px-6 pt-8 pb-5">
      <p class="text-[11px] font-semibold uppercase tracking-[0.2em] text-violet-300/80 mb-1.5">Configuration</p>
      <h1 class="text-2xl font-bold tracking-tight text-white">Settings</h1>
      <p class="mt-1 text-sm text-slate-400">
        Configure where the dashboard sends API requests, and the AI model used for issue analysis.
      </p>
    </div>

    <div class="flex-1 min-h-0 overflow-y-auto">
      <div class="mx-auto w-full max-w-3xl px-6 pb-8 space-y-6">

        <!-- ============================================ -->
        <!-- API Server URL — client-side, single SOT     -->
        <!-- ============================================ -->
        <section class="surface rounded-xl p-6 space-y-5">
          <header class="flex items-start justify-between gap-3">
            <div>
              <h2 class="text-base font-semibold text-white">Backend API Server</h2>
              <p class="mt-1 text-[11px] text-slate-500">
                The base URL for all <code class="text-slate-300">/api/*</code> requests. Stored locally per browser, applied immediately to the next request.
              </p>
            </div>
            @if (apiBaseUrl.isConfigured()) {
              <span class="rounded-full bg-emerald-500/10 px-2 py-0.5 text-[10px] font-bold tracking-wider text-emerald-300 ring-1 ring-emerald-500/30">
                ACTIVE
              </span>
            } @else {
              <span class="rounded-full bg-red-500/10 px-2 py-0.5 text-[10px] font-bold tracking-wider text-red-300 ring-1 ring-red-500/30">
                NOT CONFIGURED
              </span>
            }
          </header>

          <form (ngSubmit)="saveApiServerUrl()" class="space-y-3">
            <div>
              <label class="block text-[10px] font-semibold uppercase tracking-wider text-slate-500 mb-1.5" for="api-server-url">
                API Server URL
              </label>
              <input id="api-server-url" type="url" [(ngModel)]="apiServerUrlInput" name="apiServerUrl"
                     placeholder="https://localhost:7246"
                     class="w-full rounded-lg border border-white/10 bg-black/20 px-3 py-2 text-sm font-mono text-slate-100 placeholder:text-slate-600 focus:border-violet-400/50 focus:outline-none focus:ring-2 focus:ring-violet-500/20" />
              <p class="mt-1 text-[11px] text-slate-500">
                Example: <span class="font-mono text-slate-400">https://my-server.example.com</span> →
                requests go to <span class="font-mono text-slate-400">https://my-server.example.com/api/&lt;path&gt;</span>
              </p>
            </div>

            @if (apiBaseUrl.isConfigured()) {
              <div class="rounded-lg border border-white/5 bg-black/20 px-3 py-2 text-[11px]">
                <span class="text-slate-500">Current:</span>
                <span class="ml-2 font-mono text-slate-200">{{ apiBaseUrl.url() }}</span>
              </div>
            }

            @if (apiUrlSaved()) {
              <div class="rounded-lg border border-emerald-500/30 bg-emerald-500/10 px-3 py-2 text-xs text-emerald-200">
                Saved. The next request will use the new URL.
              </div>
            }

            <div class="flex items-center justify-end gap-2">
              @if (apiBaseUrl.isConfigured()) {
                <button type="button" (click)="clearApiServerUrl()"
                        class="rounded-lg border border-white/10 bg-white/[0.03] px-3 py-2 text-sm text-slate-300 transition hover:bg-white/[0.07]">
                  Clear
                </button>
              }
              <button type="submit"
                      class="rounded-lg bg-violet-500 px-4 py-2 text-sm font-medium text-white shadow-lg shadow-violet-500/30 transition hover:bg-violet-400 disabled:opacity-40"
                      [disabled]="!apiServerUrlInput.trim()">
                Save URL
              </button>
            </div>
          </form>
        </section>

        <!-- ============================================ -->
        <!-- AI Agent Settings — UNCHANGED                -->
        <!-- ============================================ -->
        <section class="space-y-5">
          <header>
            <h2 class="text-base font-semibold text-white">AI Agent</h2>
            <p class="mt-1 text-[11px] text-slate-500">Groq model used to analyze grouped issues. Persisted in the LogMind database.</p>
          </header>

          @if (loading()) {
            <app-spinner />
          } @else {
            @if (loadError()) {
              <div class="rounded-lg border border-red-500/30 bg-red-500/10 px-4 py-3 text-sm text-red-200">
                <p class="font-semibold mb-1">Could not load agent settings</p>
                <p class="text-xs opacity-90 break-all">{{ loadError() }}</p>
                @if (!apiBaseUrl.isConfigured()) {
                  <p class="text-xs opacity-70 mt-2">Set the API Server URL above first, then reload this page.</p>
                }
              </div>
            }

            @if (current(); as cur) {
              <div class="surface rounded-xl p-4 flex items-center gap-3">
                <span class="grid h-8 w-8 place-items-center rounded-full"
                      [class.bg-emerald-500\\/15]="cur.apiKeyConfigured"
                      [class.text-emerald-300]="cur.apiKeyConfigured"
                      [class.bg-amber-500\\/15]="!cur.apiKeyConfigured"
                      [class.text-amber-300]="!cur.apiKeyConfigured">
                  @if (cur.apiKeyConfigured) {
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="h-4 w-4">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7" />
                    </svg>
                  } @else {
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-4 w-4">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v4m0 4h.01" />
                    </svg>
                  }
                </span>
                <div>
                  <p class="text-sm font-semibold text-white">
                    {{ cur.apiKeyConfigured ? 'Agent configured' : 'API key required' }}
                  </p>
                  <p class="text-[11px] text-slate-500">
                    Source: <span class="text-slate-300">{{ cur.source }}</span>
                    @if (cur.updatedAt) {
                      · Updated {{ cur.updatedAt | relativeTime }} ({{ cur.updatedAt | date:'short' }})
                    }
                  </p>
                </div>
              </div>
            }

            <form (ngSubmit)="saveAgent()" class="surface rounded-xl p-6 space-y-5">
              <div>
                <label class="block text-[10px] font-semibold uppercase tracking-wider text-slate-500 mb-1.5" for="model-select">
                  Model
                </label>
                <div class="relative">
                  <select id="model-select" [(ngModel)]="model" name="model" required
                          class="w-full appearance-none rounded-lg border border-white/10 bg-black/20 px-3 py-2 pr-9 text-sm text-slate-100 focus:border-violet-400/50 focus:outline-none focus:ring-2 focus:ring-violet-500/20">
                    @for (m of models(); track m) {
                      <option [value]="m" class="bg-slate-900 text-slate-100">{{ m }}</option>
                    }
                  </select>
                  <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-slate-400">
                    <path stroke-linecap="round" stroke-linejoin="round" d="m6 9 6 6 6-6" />
                  </svg>
                </div>
                <p class="mt-1 text-[11px] text-slate-500">Groq-supported chat models. Selection is applied to the next analysis cycle without a restart.</p>
              </div>

              <div>
                <label class="block text-[10px] font-semibold uppercase tracking-wider text-slate-500 mb-1.5" for="api-key">
                  API Key
                </label>
                <input id="api-key" type="password" [(ngModel)]="apiKey" name="apiKey"
                       [placeholder]="apiKeyPlaceholder()"
                       autocomplete="off"
                       class="w-full rounded-lg border border-white/10 bg-black/20 px-3 py-2 text-sm font-mono text-slate-100 placeholder:text-slate-600 focus:border-violet-400/50 focus:outline-none focus:ring-2 focus:ring-violet-500/20" />
                <p class="mt-1 text-[11px] text-slate-500">Stored in the LogMind database. Leave empty to keep the current key.</p>
              </div>

              @if (testResult(); as t) {
                <div class="rounded-lg border px-3 py-2.5"
                     [class.border-emerald-500\\/30]="t.success"
                     [class.bg-emerald-500\\/10]="t.success"
                     [class.text-emerald-200]="t.success"
                     [class.border-red-500\\/30]="!t.success"
                     [class.bg-red-500\\/10]="!t.success"
                     [class.text-red-200]="!t.success">
                  <div class="flex items-start gap-2 text-xs">
                    <span class="font-semibold">{{ t.success ? 'Success' : 'Failed' }}:</span>
                    <span class="break-all">{{ t.message }}</span>
                    @if (t.latencyMs != null) {
                      <span class="ml-auto text-[10px] opacity-70 shrink-0">{{ t.latencyMs }}ms</span>
                    }
                  </div>
                </div>
              }

              @if (saveError()) {
                <div class="rounded-lg border border-red-500/30 bg-red-500/10 px-3 py-2.5 text-xs text-red-200">
                  {{ saveError() }}
                </div>
              }

              <div class="flex items-center justify-between gap-3 pt-2 border-t border-white/5">
                <button type="button" (click)="testAgent()" [disabled]="testing() || !canTest()"
                        class="inline-flex items-center gap-1.5 rounded-lg border border-white/10 bg-white/[0.03] px-3 py-2 text-sm text-slate-200 transition hover:bg-white/[0.07] disabled:opacity-40">
                  @if (testing()) {
                    <span class="h-3 w-3 animate-spin rounded-full border-2 border-slate-400 border-t-transparent"></span>
                  } @else {
                    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="h-3.5 w-3.5">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
                    </svg>
                  }
                  Test connection
                </button>

                <button type="submit" [disabled]="saving() || !canSave()"
                        class="inline-flex items-center gap-1.5 rounded-lg bg-violet-500 px-4 py-2 text-sm font-medium text-white shadow-lg shadow-violet-500/30 transition hover:bg-violet-400 disabled:opacity-40 disabled:hover:bg-violet-500">
                  @if (saving()) {
                    <span class="h-3 w-3 animate-spin rounded-full border-2 border-white border-t-transparent"></span>
                  }
                  Save agent settings
                </button>
              </div>
            </form>

            @if (agentSaved()) {
              <div class="rounded-lg border border-emerald-500/30 bg-emerald-500/10 px-3 py-2.5 text-xs text-emerald-200">
                Agent settings saved. The next analysis cycle will use the new configuration.
              </div>
            }
          }
        </section>

      </div>
    </div>
  `
})
export class SettingsPage {
  private readonly api = inject(SettingsApi);
  protected readonly apiBaseUrl = inject(ApiBaseUrlService);

  protected apiServerUrlInput = this.apiBaseUrl.url();

  protected model = DEFAULT_MODEL;
  protected apiKey = '';

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly testing = signal(false);
  protected readonly agentSaved = signal(false);
  protected readonly apiUrlSaved = signal(false);
  protected readonly current = signal<AgentSettingsDto | null>(null);
  protected readonly testResult = signal<TestAgentSettingsResponse | null>(null);
  protected readonly saveError = signal<string | null>(null);
  protected readonly loadError = signal<string | null>(null);

  protected readonly models = computed(() => {
    const fromServer = this.current()?.availableModels;
    return fromServer && fromServer.length > 0 ? fromServer : FALLBACK_MODELS;
  });

  protected readonly canTest = computed(() => !!this.model.trim());
  protected readonly canSave = computed(() => !!this.model.trim());

  protected readonly apiKeyPlaceholder = computed(() => {
    const cur = this.current();
    if (cur?.apiKeyConfigured) return `Leave empty to keep current key (${cur.apiKeyMasked})`;
    return 'Enter your Groq API key';
  });

  constructor() { this.loadAgent(); }

  protected saveApiServerUrl() {
    this.apiBaseUrl.set(this.apiServerUrlInput);
    this.apiServerUrlInput = this.apiBaseUrl.url();
    this.apiUrlSaved.set(true);
    setTimeout(() => this.apiUrlSaved.set(false), 3000);
    this.loadAgent();
  }

  protected clearApiServerUrl() {
    this.apiBaseUrl.clear();
    this.apiServerUrlInput = '';
    this.current.set(null);
    this.loadError.set(null);
  }

  protected saveAgent() {
    if (!this.canSave()) return;
    this.saving.set(true);
    this.saveError.set(null);
    this.agentSaved.set(false);
    this.api.updateAgent({
      model: this.model.trim(),
      apiKey: this.apiKey.trim() || undefined
    }).subscribe({
      next: (dto) => {
        this.applyDto(dto);
        this.apiKey = '';
        this.agentSaved.set(true);
        this.saving.set(false);
        setTimeout(() => this.agentSaved.set(false), 4000);
      },
      error: (err) => {
        this.saveError.set(this.formatError(err));
        this.saving.set(false);
      }
    });
  }

  protected testAgent() {
    if (!this.canTest()) return;
    this.testing.set(true);
    this.testResult.set(null);
    this.api.testAgent({
      model: this.model.trim(),
      apiKey: this.apiKey.trim() || undefined
    }).subscribe({
      next: (res) => {
        this.testResult.set(res);
        this.testing.set(false);
      },
      error: (err) => {
        this.testResult.set({ success: false, message: this.formatError(err) });
        this.testing.set(false);
      }
    });
  }

  private loadAgent() {
    this.loading.set(true);
    this.loadError.set(null);
    this.api.getAgent().subscribe({
      next: (dto) => {
        this.applyDto(dto);
        this.loading.set(false);
      },
      error: (err) => {
        this.loadError.set(this.formatError(err));
        this.loading.set(false);
      }
    });
  }

  private applyDto(dto: AgentSettingsDto) {
    this.current.set(dto);
    this.model = dto.model || DEFAULT_MODEL;
  }

  private formatError(err: unknown): string {
    const e = err as { status?: number; statusText?: string; error?: { message?: string; detail?: string }; message?: string };
    if (e?.error?.message) return e.error.message;
    if (e?.error?.detail) return e.error.detail;
    if (e?.status === 0) return 'API Server is unreachable. Verify the API Server URL.';
    if (e?.status) return `HTTP ${e.status} ${e.statusText ?? ''}`.trim();
    return e?.message ?? 'Request failed';
  }
}
