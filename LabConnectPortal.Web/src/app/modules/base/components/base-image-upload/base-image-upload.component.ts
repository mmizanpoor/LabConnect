import { Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'base-image-upload',
  standalone: true,
  imports: [MatIconModule, MatTooltipModule, TranslocoPipe],
  templateUrl: './base-image-upload.component.html',
})
export class BaseImageUploadComponent {
  @ViewChild('fileInput') fileInput?: ElementRef<HTMLInputElement>;

  @Input() label = '';
  @Input() labelAlign: 'right' | 'center' = 'right';
  @Input() imageUrl = '';
  @Input() fileName = '';
  @Input() disabled = false;
  @Input() loading = false;
  @Input() accept = 'image/*';
  @Input() hint = '';
  @Input() dropText = '';
  @Input() previewShape: 'circle' | 'square' = 'square';
  @Input() previewSize = 100;
  @Input() clearable = false;
  @Input() compact = false;
  @Input() showFileName = true;
  /** Preview fills the whole drop zone; height stays fixed via host styles. */
  @Input() fill = false;

  @Output() fileSelected = new EventEmitter<File>();
  @Output() cleared = new EventEmitter<void>();

  isDragging = false;

  onBrowseClick(): void {
    if (this.disabled || this.loading) return;
    this.fileInput?.nativeElement.click();
  }

  onClearClick(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    if (this.disabled || this.loading) return;
    this.cleared.emit();
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (file) this.emitFile(file);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (this.disabled || this.loading) return;
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging = false;
    if (this.disabled || this.loading) return;

    const file = event.dataTransfer?.files?.[0];
    if (file?.type.startsWith('image/')) this.emitFile(file);
  }

  private emitFile(file: File): void {
    this.fileSelected.emit(file);
  }
}
