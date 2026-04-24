import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LanguageService, Language } from '../../../core/language.service';

@Component({
  selector: 'app-language-toggle',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './language-toggle.component.html',
  styleUrl: './language-toggle.component.scss'
})
export class LanguageToggleComponent {
  constructor(private languageService: LanguageService) {}

  get currentLanguage() {
    return this.languageService.currentLanguage;
  }

  /**
   * Set language explicitly
   */
  setLanguage(lang: Language): void {
    if (lang !== this.currentLanguage()) {
      this.languageService.setLanguage(lang);
    }
  }

  /**
   * Toggle to the opposite language
   */
  toggleLanguage(): void {
    this.languageService.toggleLanguage();
  }
}
