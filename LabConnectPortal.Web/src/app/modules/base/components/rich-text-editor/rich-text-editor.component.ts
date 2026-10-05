import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Input,
  OnChanges,
  SimpleChanges,
  ViewEncapsulation,
  forwardRef,
} from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import {
  Alignment,
  Autoformat,
  BlockQuote,
  Bold,
  ClassicEditor,
  Code,
  CodeBlock,
  Essentials,
  FindAndReplace,
  FontBackgroundColor,
  FontColor,
  FontFamily,
  FontSize,
  Heading,
  Highlight,
  HorizontalLine,
  Image,
  ImageCaption,
  ImageInsertViaUrl,
  ImageResize,
  ImageStyle,
  ImageToolbar,
  Indent,
  IndentBlock,
  Italic,
  Link,
  LinkImage,
  List,
  ListProperties,
  MediaEmbed,
  PageBreak,
  Paragraph,
  PasteFromOffice,
  RemoveFormat,
  ShowBlocks,
  SourceEditing,
  SpecialCharacters,
  SpecialCharactersEssentials,
  Strikethrough,
  Subscript,
  Superscript,
  Table,
  TableCaption,
  TableCellProperties,
  TableColumnResize,
  TableProperties,
  TableToolbar,
  TodoList,
  Underline,
  Undo,
} from 'ckeditor5';

export type RichTextEditorVariant = 'basic' | 'extended';

const ESTEDAD_FONT_FAMILY = 'Estedad-FD, Tahoma, sans-serif';

@Component({
  selector: 'app-rich-text-editor',
  standalone: true,
  imports: [CKEditorModule, FormsModule],
  template: `
    <ckeditor
      [editor]="Editor"
      [config]="editorConfig"
      (change)="onEditorChange($event)"
      (ready)="onReady($event)"
    />
  `,
  styles: `
    :host {
      display: block;
    }

    :host .ck-editor__editable,
    :host .ck-editor__editable_inline {
      min-height: 220px;
      font-family: 'Estedad-FD', Tahoma, sans-serif;
    }

    :host .ck-content {
      font-family: 'Estedad-FD', Tahoma, sans-serif;
    }

    app-rich-text-editor .ck-content,
    app-rich-text-editor .ck-content p,
    app-rich-text-editor .ck-content h1,
    app-rich-text-editor .ck-content h2,
    app-rich-text-editor .ck-content h3,
    app-rich-text-editor .ck-content h4,
    app-rich-text-editor .ck-content h5,
    app-rich-text-editor .ck-content h6,
    app-rich-text-editor .ck-content li,
    app-rich-text-editor .ck-content blockquote,
    app-rich-text-editor .ck-content figcaption,
    app-rich-text-editor .ck-content td,
    app-rich-text-editor .ck-content th {
      /* Resolve each paragraph from its first strong character without forcing dir. */
      unicode-bidi: plaintext;
    }

    :host.extended .ck-editor__editable,
    :host.extended .ck-editor__editable_inline {
      min-height: 360px;
    }

    :host.rte-sized .ck-editor__editable,
    :host.rte-sized .ck-editor__editable_inline {
      min-height: var(--rte-min-height, 8.75rem) !important;
    }

    :host .ck-toolbar {
      flex-wrap: wrap;
    }
  `,
  host: {
    '[class.extended]': 'variant === "extended"',
    '[class.rte-sized]': 'rows != null && rows > 0',
    '[style.--rte-min-height]': 'rowsMinHeight',
  },
  encapsulation: ViewEncapsulation.None,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => RichTextEditorComponent),
      multi: true,
    },
  ],
})
export class RichTextEditorComponent implements ControlValueAccessor, OnChanges {
  @Input() variant: RichTextEditorVariant = 'basic';
  /** Approximate editable height in text rows (overrides default min-height when set). */
  @Input() rows: number | null = null;

  protected readonly Editor = ClassicEditor;
  protected editorConfig = this.buildConfig('basic');

  value = '';
  disabled = false;

  private _editor: ClassicEditor | null = null;
  private _isApplyingExternalValue = false;
  private _onChange: (value: string) => void = () => undefined;
  private _onTouched: () => void = () => undefined;

  constructor(private _changeDetectorRef: ChangeDetectorRef) {}

  get rowsMinHeight(): string | null {
    if (this.rows == null || this.rows <= 0) return null;
    // ~1.5 line-height per row + editor padding
    return `${this.rows * 1.5 + 1.25}rem`;
  }

  private get resolvedMinHeight(): string {
    return this.rowsMinHeight ?? (this.variant === 'extended' ? '360px' : '220px');
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['variant']) {
      this.editorConfig = this.buildConfig(this.variant);
      this._changeDetectorRef.markForCheck();
    }
    if (changes['rows'] || changes['variant']) {
      this.applyEditableMinHeight();
    }
  }

  writeValue(value: string | null): void {
    this.value = value ?? '';

    if (this._editor && this._editor.getData() !== this.value) {
      this.setEditorData(this.value);
    }

    this._changeDetectorRef.markForCheck();
  }

  registerOnChange(fn: (value: string) => void): void {
    this._onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this._onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
    this._changeDetectorRef.markForCheck();
  }

  onEditorChange(event: { editor: { getData: () => string } }): void {
    this.value = event.editor.getData();

    if (this._isApplyingExternalValue) return;

    this._onChange(this.value);
    this._onTouched();
  }

  onReady(editor: ClassicEditor): void {
    this._editor = editor;
    if (editor.getData() !== this.value) {
      this.setEditorData(this.value);
    }
    this.applyEditableMinHeight();
    this._changeDetectorRef.markForCheck();
  }

  private setEditorData(value: string): void {
    if (!this._editor) return;

    this._isApplyingExternalValue = true;
    try {
      this._editor.setData(value);
    } finally {
      this._isApplyingExternalValue = false;
    }
  }

  private applyEditableMinHeight(): void {
    const editor = this._editor;
    if (!editor) return;

    const minHeight = this.resolvedMinHeight;
    const root = editor.editing.view.document.getRoot();
    if (root) {
      editor.editing.view.change((writer) => {
        writer.setStyle('min-height', minHeight, root);
      });
    }

    const editableEl = editor.ui.getEditableElement();
    if (editableEl) {
      editableEl.style.minHeight = minHeight;
    }
  }

  private buildConfig(variant: RichTextEditorVariant) {
    if (variant === 'extended') {
      return {
        licenseKey: 'GPL',
        plugins: [
          Essentials,
          Paragraph,
          Autoformat,
          PasteFromOffice,
          Bold,
          Italic,
          Underline,
          Strikethrough,
          Code,
          Subscript,
          Superscript,
          Heading,
          FontFamily,
          FontSize,
          FontColor,
          FontBackgroundColor,
          Highlight,
          Link,
          List,
          ListProperties,
          TodoList,
          Alignment,
          Indent,
          IndentBlock,
          BlockQuote,
          CodeBlock,
          HorizontalLine,
          PageBreak,
          Table,
          TableToolbar,
          TableProperties,
          TableCellProperties,
          TableCaption,
          TableColumnResize,
          Image,
          ImageToolbar,
          ImageCaption,
          ImageStyle,
          ImageResize,
          ImageInsertViaUrl,
          LinkImage,
          MediaEmbed,
          SpecialCharacters,
          SpecialCharactersEssentials,
          FindAndReplace,
          ShowBlocks,
          SourceEditing,
          RemoveFormat,
          Undo,
        ],
        toolbar: {
          items: [
            'undo',
            'redo',
            '|',
            'sourceEditing',
            'showBlocks',
            'findAndReplace',
            '|',
            'heading',
            '|',
            'fontFamily',
            'fontSize',
            'fontColor',
            'fontBackgroundColor',
            'highlight',
            '|',
            'bold',
            'italic',
            'underline',
            'strikethrough',
            'code',
            'subscript',
            'superscript',
            'removeFormat',
            '|',
            'alignment',
            '|',
            'bulletedList',
            'numberedList',
            'todoList',
            'outdent',
            'indent',
            '|',
            'link',
            'insertImage',
            'mediaEmbed',
            'insertTable',
            'blockQuote',
            'codeBlock',
            'specialCharacters',
            'horizontalLine',
            'pageBreak',
          ],
          shouldNotGroupWhenFull: true,
        },
        image: {
          toolbar: [
            'imageTextAlternative',
            'toggleImageCaption',
            '|',
            'imageStyle:inline',
            'imageStyle:block',
            'imageStyle:side',
            '|',
            'resizeImage',
            'linkImage',
          ],
        },
        table: {
          contentToolbar: [
            'tableColumn',
            'tableRow',
            'mergeTableCells',
            'tableProperties',
            'tableCellProperties',
            'toggleTableCaption',
          ],
        },
        list: {
          properties: {
            styles: true,
            startIndex: true,
            reversed: true,
          },
        },
        fontFamily: {
          options: [
            'default',
            {
              title: 'Estedad',
              model: ESTEDAD_FONT_FAMILY,
            },
            'Tahoma, Geneva, sans-serif',
            'Arial, Helvetica, sans-serif',
            'Courier New, Courier, monospace',
          ],
          supportAllValues: true,
        },
        fontSize: {
          options: [10, 12, 14, 'default', 18, 20, 24, 28, 32],
          supportAllValues: true,
        },
        mediaEmbed: {
          previewsInData: true,
        },
      };
    }

    return {
      licenseKey: 'GPL',
      plugins: [Essentials, Paragraph, Bold, Italic, Heading, Link, List, Undo],
      toolbar: [
        'undo',
        'redo',
        '|',
        'heading',
        '|',
        'bold',
        'italic',
        'link',
        '|',
        'bulletedList',
        'numberedList',
      ],
    };
  }
}
