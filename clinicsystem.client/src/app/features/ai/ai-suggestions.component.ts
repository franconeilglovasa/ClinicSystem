import { Component, Input, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { AISuggestion } from '../../core/models/models';

@Component({
  selector: 'app-ai-suggestions',
  templateUrl: './ai-suggestions.component.html'
})
export class AiSuggestionsComponent implements OnInit {
  @Input() visitId!: string;

  suggestions: AISuggestion[] = [];
  additionalContext = '';
  loading = false;
  savingSuggestionId: string | null = null;
  editingSuggestionId: string | null = null;
  editedResponse = '';

  constructor(private api: ApiService, public auth: AuthService) {}

  ngOnInit(): void { this.load(); }

  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  load(): void {
    this.api.getAISuggestions(this.visitId).subscribe(r => this.suggestions = r);
  }

  generate(): void {
    this.loading = true;
    this.api.generateAISuggestion(this.visitId, { additionalContext: this.additionalContext }).subscribe({
      next: () => {
        this.loading = false;
        this.additionalContext = '';
        this.load();
      },
      error: () => this.loading = false
    });
  }

  delete(suggestionId: string): void {
    if (!confirm('Delete this AI suggestion?')) return;
    this.api.deleteAISuggestion(this.visitId, suggestionId).subscribe(() => {
      this.suggestions = this.suggestions.filter(s => s.suggestionId !== suggestionId);
    });
  }

  startEdit(suggestion: AISuggestion): void {
    this.editingSuggestionId = suggestion.suggestionId;
    this.editedResponse = suggestion.response ?? '';
  }

  cancelEdit(): void {
    this.editingSuggestionId = null;
    this.editedResponse = '';
  }

  saveEdit(suggestion: AISuggestion): void {
    const response = this.editedResponse.trim();
    if (!response) {
      alert('Suggestion response cannot be empty.');
      return;
    }

    this.savingSuggestionId = suggestion.suggestionId;
    this.api.updateAISuggestion(this.visitId, suggestion.suggestionId, { response }).subscribe({
      next: updated => {
        this.suggestions = this.suggestions.map(s =>
          s.suggestionId === updated.suggestionId ? updated : s
        );
        this.savingSuggestionId = null;
        this.cancelEdit();
      },
      error: () => {
        this.savingSuggestionId = null;
      }
    });
  }
}
