import { AfterViewInit, Component, ElementRef, Input, OnDestroy, ViewChild } from '@angular/core';
import {
  CustomToolbarItemModel,
  DocumentEditorContainerAllModule,
  DocumentEditorContainerComponent,
} from '@syncfusion/ej2-angular-documenteditor';
import { ClickEventArgs } from '@syncfusion/ej2-navigations';
import { environment } from '@env/environment';
import { TokenService } from '@core/services/token/token.service';

@Component({
  selector: 'app-document-editor',
  standalone: true,
  imports: [DocumentEditorContainerAllModule],
  templateUrl: './document-editor.component.html',
  styleUrl: './document-editor.component.scss',
})
export class DocumentEditorComponent implements AfterViewInit, OnDestroy {
  @ViewChild('documentContainer') documentContainer!: DocumentEditorContainerComponent;

  @Input() content = '';
  @Input() readonly = false;
  @Input() enableRtl = true;

  protected customHeaders: Record<string, string>[] = [];
  protected actionSettings = { systemClipboard: 'SystemClipboard' };

  protected documentEditorSettings = {
    enableContextMenu: false,
    showRuler: true,
  };

  protected rtlToolbarItem: CustomToolbarItemModel = {
    prefixIcon: 'e-icons e-align-right',
    tooltipText: 'Right to Left',
    text: 'RTL',
    id: 'rtl',
  };

  protected ltrToolbarItem: CustomToolbarItemModel = {
    prefixIcon: 'e-icons e-align-left',
    tooltipText: 'Left to Right',
    text: 'LTR',
    id: 'ltr',
  };

  protected normalToolbarItems: (string | CustomToolbarItemModel)[] = [
    'Undo',
    'Redo',
    'Separator',
    'Image',
    'Table',
    'Separator',
    'Header',
    'Footer',
    'PageSetup',
    'PageNumber',
    'Break',
    'Separator',
    'Find',
    'Separator',
    this.rtlToolbarItem,
    this.ltrToolbarItem,
  ];

  protected restrictedToolbarItems: string[] = [];
  protected serviceUrl = `${environment.apiUrl}Syncfusion/`;

  constructor(
    private _tokenService: TokenService,
    private _el: ElementRef,
  ) {
    const token = this._tokenService.accessToken;
    if (token) {
      this.customHeaders = [{ Authorization: `Bearer ${token}` }];
    }
  }

  ngAfterViewInit(): void {
    const t = setTimeout(() => {
      clearTimeout(t);
      if (this.content) {
        this.documentContainer?.documentEditor?.open(this.content);
      }
      this.documentContainer?.resize();
      this.documentContainer?.documentEditor?.resize();
    }, 100);
  }

  ngOnDestroy(): void {}

  protected onToolbarClick(args: ClickEventArgs): void {
    if (args.item.id === 'rtl') {
      this.setRtl(true);
    } else if (args.item.id === 'ltr') {
      this.setRtl(false);
    }
  }

  protected resize(): void {
    this.documentContainer?.resize();
    this.documentContainer?.documentEditor?.resize();
  }

  getContent(): string {
    return this.documentContainer?.documentEditor?.serialize() ?? '';
  }

  isEmptyContent(): boolean {
    const editor = this.documentContainer?.documentEditor;
    if (!editor?.selection) return true;

    try {
      editor.selection.selectAll();
      const text = (editor.selection as { getText?: (trim?: boolean) => string }).getText?.(true) ?? editor.selection.text ?? '';
      editor.selection.moveToDocumentStart();
      if (text.trim().length > 0) return false;
    } catch {
      // fall through
    }

    const content = this.getContent();
    if (!content?.trim()) return true;

    try {
      return !this.hasSfdtTextContent(JSON.parse(content));
    } catch {
      return !content.trim();
    }
  }

  private hasSfdtTextContent(node: unknown): boolean {
    if (!node || typeof node !== 'object') return false;
    if (Array.isArray(node)) return node.some((item) => this.hasSfdtTextContent(item));

    const obj = node as Record<string, unknown>;
    for (const [key, value] of Object.entries(obj)) {
      if ((key === 'text' || key === 'tlp') && typeof value === 'string' && value.trim().length > 0) {
        return true;
      }
      if (this.hasSfdtTextContent(value)) return true;
    }
    return false;
  }

  private setRtl(rtl: boolean): void {
    const editor = this.documentContainer?.documentEditor;
    if (!editor?.editorModule) return;

    try {
      editor.editorModule.onApplyParagraphFormat('bidi', rtl, false, true);
      editor.editorModule.onApplyParagraphFormat('textAlignment', rtl ? 'Right' : 'Left', false, true);
    } catch {
      // ignore
    }
  }

  ngDoCheck(): void {
    if (this.documentContainer?.height != this._el?.nativeElement?.offsetHeight) {
      this.resize();
    }
  }
}
