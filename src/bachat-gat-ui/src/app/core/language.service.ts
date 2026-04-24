import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { TranslateService } from '@ngx-translate/core';
import { HttpClient } from '@angular/common/http';

export type Language = 'en' | 'mr';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly LANGUAGE_KEY = 'bachat-gat-lang';
  private readonly LANGUAGE_COOKIE = 'bachat-gat-language';

  // Signal to track current language reactively
  currentLanguage = signal<Language>('en');

  constructor(private router: Router, private translate: TranslateService, private http: HttpClient) {
    this.setupTranslations();
    this.initializeLanguage();
  }

  /**
   * Load translation files for both languages
   */
  private setupTranslations(): void {
    this.http.get<any>('assets/i18n/en.json').subscribe(data => {
      this.translate.setTranslation('en', data, true);
    });
    this.http.get<any>('assets/i18n/mr.json').subscribe(data => {
      this.translate.setTranslation('mr', data, true);
    });
  }

  /**
   * Initialize language from storage, cookie, or browser detection
   */
  private initializeLanguage(): void {
    // Priority: URL path > localStorage > cookie > browser detect > default 'en'
    const urlLang = this.getLanguageFromUrl();
    const stored = urlLang || this.getStoredLanguage();
    const lang = stored || this.getBrowserLanguage() || 'en';
    this.setLanguage(lang, false);
  }

  /**
   * Get language from current URL path (/en/ or /mr/)
   */
  private getLanguageFromUrl(): Language | null {
    const path = this.router.url;
    if (path.startsWith('/en/') || path === '/en') return 'en';
    if (path.startsWith('/mr/') || path === '/mr') return 'mr';
    return null;
  }

  /**
   * Retrieve language from localStorage or cookie
   */
  private getStoredLanguage(): Language | null {
    // Try localStorage first
    const local = localStorage.getItem(this.LANGUAGE_KEY);
    if (local === 'en' || local === 'mr') return local;

    // Fall back to cookie
    const cookieValue = this.getCookie(this.LANGUAGE_COOKIE);
    if (cookieValue === 'en' || cookieValue === 'mr') return cookieValue;

    return null;
  }

  /**
   * Detect browser language and return 'mr' if Marathi, else 'en'
   */
  private getBrowserLanguage(): Language {
    const browserLang = navigator.language || '';
    return browserLang.toLowerCase().startsWith('mr') ? 'mr' : 'en';
  }

  /**
   * Utility: read a cookie value by name
   */
  private getCookie(name: string): string | null {
    const nameEQ = name + '=';
    const cookies = document.cookie.split(';');
    for (const cookie of cookies) {
      const trimmed = cookie.trim();
      if (trimmed.startsWith(nameEQ)) {
        return trimmed.substring(nameEQ.length);
      }
    }
    return null;
  }

  /**
   * Set language and update UI, routing, and persistence
   */
  setLanguage(lang: Language, navigate: boolean = true): void {
    if (lang === this.currentLanguage() && !navigate) return;

    // Update signal
    this.currentLanguage.set(lang);

    // Persist to localStorage and cookie
    localStorage.setItem(this.LANGUAGE_KEY, lang);
    this.setCookie(this.LANGUAGE_COOKIE, lang, 365);

    // Update ngx-translate
    this.translate.use(lang);

    // Navigate to language-prefixed route if needed
    if (navigate) {
      this.navigateToLanguagePath(lang);
    }
  }

  /**
   * Get current language
   */
  getLanguage(): Language {
    return this.currentLanguage();
  }

  /**
   * Utility: set a cookie
   */
  private setCookie(name: string, value: string, days: number): void {
    const date = new Date();
    date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
    const expires = 'expires=' + date.toUTCString();
    document.cookie = `${name}=${value}; ${expires}; path=/`;
  }

  /**
   * Navigate to the current route with language prefix
   */
  private navigateToLanguagePath(lang: Language): void {
    const currentUrl = this.router.url;

    // Remove old language prefix if present
    let pathWithoutLang = currentUrl.replace(/^\/(en|mr)(\/|$)/, '/');
    if (!pathWithoutLang.startsWith('/')) {
      pathWithoutLang = '/' + pathWithoutLang;
    }

    // Navigate with new language prefix
    const newUrl = `/${lang}${pathWithoutLang}`;
    this.router.navigateByUrl(newUrl);
  }

  /**
   * Toggle between English and Marathi
   */
  toggleLanguage(): void {
    const newLang = this.currentLanguage() === 'en' ? 'mr' : 'en';
    this.setLanguage(newLang);
  }
}
