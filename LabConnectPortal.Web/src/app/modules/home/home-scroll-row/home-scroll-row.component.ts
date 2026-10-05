import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  Input,
  NgZone,
  OnDestroy,
  ViewChild,
  ViewEncapsulation,
  inject,
} from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

const DRAG_THRESHOLD_PX = 6;

@Component({
  selector: 'app-home-scroll-row',
  standalone: true,
  imports: [MatIconModule],
  templateUrl: './home-scroll-row.component.html',
  styleUrl: './home-scroll-row.component.scss',
  encapsulation: ViewEncapsulation.None,
  host: {
    class: 'block min-w-0 w-full',
    '[class.home-scroll-row--products]': 'variant === "products"',
  },
})
export class HomeScrollRowComponent implements AfterViewInit, OnDestroy {
  @Input() variant: 'default' | 'products' = 'default';
  @ViewChild('viewport', { static: true }) viewport!: ElementRef<HTMLElement>;
  @ViewChild('track', { static: true }) track!: ElementRef<HTMLElement>;

  canPrev = false;
  canNext = false;
  dragging = false;

  private _cdr = inject(ChangeDetectorRef);
  private _zone = inject(NgZone);
  private resizeObserver?: ResizeObserver;
  private mutationObserver?: MutationObserver;
  private _pointerId: number | null = null;
  private _startX = 0;
  private _startScrollLeft = 0;
  private _didDrag = false;

  ngAfterViewInit(): void {
    const node = this.viewport.nativeElement;
    this.resizeObserver = new ResizeObserver(() =>
      this.scheduleScrollStateUpdate()
    );
    this.resizeObserver.observe(node);
    this.mutationObserver = new MutationObserver(() =>
      this.scheduleScrollStateUpdate()
    );
    this.mutationObserver.observe(this.track.nativeElement, {
      childList: true,
    });
    queueMicrotask(() => this.updateScrollState());
    requestAnimationFrame(() => this.updateScrollState());

    this._zone.runOutsideAngular(() => {
      node.addEventListener('pointerdown', this._onPointerDown);
      node.addEventListener('pointermove', this._onPointerMove);
      node.addEventListener('pointerup', this._onPointerUp);
      node.addEventListener('pointercancel', this._onPointerUp);
      node.addEventListener('click', this._onClickCapture, true);
      node.addEventListener('dragstart', this._onDragStart);
    });
  }

  ngOnDestroy(): void {
    this.resizeObserver?.disconnect();
    this.mutationObserver?.disconnect();
    const node = this.viewport?.nativeElement;
    if (!node) return;
    node.removeEventListener('pointerdown', this._onPointerDown);
    node.removeEventListener('pointermove', this._onPointerMove);
    node.removeEventListener('pointerup', this._onPointerUp);
    node.removeEventListener('pointercancel', this._onPointerUp);
    node.removeEventListener('click', this._onClickCapture, true);
    node.removeEventListener('dragstart', this._onDragStart);
  }

  scrollPrev(): void {
    this.scrollByDirection(-1);
  }

  scrollNext(): void {
    this.scrollByDirection(1);
  }

  onScroll(): void {
    this.updateScrollState();
  }

  private readonly _onPointerDown = (event: PointerEvent): void => {
    if (event.pointerType !== 'mouse' || event.button !== 0) return;

    const node = this.viewport.nativeElement;
    this._pointerId = event.pointerId;
    this._startX = event.clientX;
    this._startScrollLeft = node.scrollLeft;
    this._didDrag = false;
  };

  private readonly _onPointerMove = (event: PointerEvent): void => {
    if (this._pointerId !== event.pointerId) return;

    const dx = event.clientX - this._startX;
    if (!this._didDrag && Math.abs(dx) < DRAG_THRESHOLD_PX) return;

    if (!this._didDrag) {
      this._didDrag = true;
      this.viewport.nativeElement.setPointerCapture(event.pointerId);
      this._zone.run(() => {
        this.dragging = true;
        this._cdr.markForCheck();
      });
    }

    event.preventDefault();
    this.viewport.nativeElement.scrollLeft = this._startScrollLeft - dx;
  };

  private readonly _onPointerUp = (event: PointerEvent): void => {
    if (this._pointerId !== event.pointerId) return;
    const didDrag = this._didDrag;
    this._stopDragging();
    // Desktop mouse-drag: snap to a full card boundary (mobile uses native scroll-snap).
    if (didDrag && this.isDesktopSnapMode()) {
      this.snapToNearestCard();
    }
    window.setTimeout(() => {
      this._didDrag = false;
    }, 0);
  };

  private readonly _onClickCapture = (event: Event): void => {
    if (!this._didDrag) return;
    event.preventDefault();
    event.stopImmediatePropagation();
    this._didDrag = false;
  };

  private readonly _onDragStart = (event: Event): void => {
    event.preventDefault();
  };

  private _stopDragging(): void {
    const node = this.viewport.nativeElement;
    const pointerId = this._pointerId;
    this._pointerId = null;
    if (pointerId !== null && node.hasPointerCapture(pointerId)) {
      node.releasePointerCapture(pointerId);
    }
    if (this.dragging) {
      this._zone.run(() => {
        this.dragging = false;
        this._cdr.markForCheck();
      });
    }
  }

  private scheduleScrollStateUpdate(): void {
    this._zone.run(() => this.updateScrollState());
  }

  private isDesktopSnapMode(): boolean {
    return (
      typeof window !== 'undefined' &&
      window.matchMedia('(min-width: 640px)').matches
    );
  }

  /** Align viewport to the nearest full card (desktop drag / button settle). */
  private snapToNearestCard(behavior: ScrollBehavior = 'smooth'): void {
    const node = this.viewport.nativeElement;
    const track = this.track.nativeElement;
    const children = Array.from(track.children) as HTMLElement[];
    if (!children.length) return;

    const gap =
      Number.parseFloat(getComputedStyle(track).columnGap || '0') || 0;
    const step =
      (children[0].getBoundingClientRect().width || node.clientWidth) + gap;
    if (step <= 0) return;

    const fromStart = this.scrollFromStart(node);
    const page = Math.round(fromStart / step);
    const maxPage = Math.max(0, children.length - 1);
    const target = Math.max(0, Math.min(maxPage, page)) * step;
    this.scrollToFromStart(node, target, behavior);
    requestAnimationFrame(() => this.updateScrollState());
  }

  private scrollFromStart(node: HTMLElement): number {
    const maxScroll = Math.max(0, node.scrollWidth - node.clientWidth);
    if (maxScroll <= 0) return 0;
    const rtl = getComputedStyle(node).direction === 'rtl';
    if (!rtl) return Math.max(0, Math.min(maxScroll, node.scrollLeft));
    if (node.scrollLeft <= 0) {
      return Math.max(0, Math.min(maxScroll, -node.scrollLeft));
    }
    return Math.max(0, Math.min(maxScroll, maxScroll - node.scrollLeft));
  }

  private scrollToFromStart(
    node: HTMLElement,
    fromStart: number,
    behavior: ScrollBehavior
  ): void {
    const maxScroll = Math.max(0, node.scrollWidth - node.clientWidth);
    const clamped = Math.max(0, Math.min(fromStart, maxScroll));
    const rtl = getComputedStyle(node).direction === 'rtl';
    const left = rtl ? -clamped : clamped;
    node.scrollTo({ left, behavior });
  }

  private scrollByDirection(direction: 1 | -1): void {
    const node = this.viewport.nativeElement;
    const track = this.track.nativeElement;
    const card = track.children.item(0) as HTMLElement | null;
    const gap =
      Number.parseFloat(getComputedStyle(track).columnGap || '0') || 0;
    const distance =
      (card?.getBoundingClientRect().width ?? node.clientWidth) + gap;
    const fromStart = this.scrollFromStart(node);
    const currentPage = Math.round(fromStart / distance);
    const maxPage = Math.max(0, track.children.length - 1);
    const targetPage = Math.max(0, Math.min(maxPage, currentPage + direction));
    this.scrollToFromStart(node, targetPage * distance, 'smooth');
    requestAnimationFrame(() => this.updateScrollState());
  }

  private updateScrollState(): void {
    const node = this.viewport.nativeElement;
    const maxScroll = Math.max(0, node.scrollWidth - node.clientWidth);
    const overflowing = maxScroll > 4;
    const rtl = getComputedStyle(node).direction === 'rtl';
    const fromStart = rtl
      ? node.scrollLeft <= 0
        ? -node.scrollLeft
        : Math.max(0, maxScroll - node.scrollLeft)
      : node.scrollLeft;

    this.canPrev = overflowing && fromStart > 4;
    this.canNext = overflowing && fromStart < maxScroll - 4;
    this._cdr.markForCheck();
  }
}
