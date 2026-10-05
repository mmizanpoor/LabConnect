import {
  Component,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
} from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { ActiveSliderSlideDto } from '@modules/admin/slider-groups/slider-groups.types';
import { PublicSiteService } from '@core/services/site/public-site.service';

@Component({
  selector: 'app-home-slider',
  standalone: true,
  imports: [TranslocoPipe],
  templateUrl: './home-slider.component.html',
})
export class HomeSliderComponent implements OnChanges, OnDestroy {
  @Input() slides: ActiveSliderSlideDto[] = [];
  @Input() fallbackTitle = '';
  @Input() fallbackTagline = '';

  currentIndex = 0;
  trackIndex = 0;
  isPaused = false;
  isDragging = false;
  dragOffsetPx = 0;
  transitionEnabled = false;

  private _timer: ReturnType<typeof setInterval> | null = null;
  private _pointerId: number | null = null;
  private _dragStartX = 0;
  private _dragStartY = 0;
  private _dragStartTime = 0;
  private _viewportWidth = 0;
  private _didDrag = false;
  private _loopResetPending = false;

  private static readonly DragStartThresholdPx = 8;
  private static readonly SwipeVelocityThreshold = 0.45;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['slides']) {
      this.currentIndex = 0;
      this.trackIndex = this.slides.length > 1 ? 1 : 0;
      this.transitionEnabled = false;
      requestAnimationFrame(() => {
        this.transitionEnabled = true;
      });
      this.restartAutoplay();
    }
  }

  ngOnDestroy(): void {
    this.stopAutoplay();
  }

  slideImageUrl(imagePath: string): string {
    return PublicSiteService.slideImageUrl(imagePath, 2880);
  }

  slideImageSrcSet(imagePath: string): string {
    return PublicSiteService.slideImageSrcSet(imagePath);
  }

  isPrioritySlide(renderedIndex: number): boolean {
    return Math.abs(renderedIndex - this.trackIndex) <= 1;
  }

  isCurrentSlide(renderedIndex: number): boolean {
    return renderedIndex === this.trackIndex;
  }

  get renderedSlides(): ActiveSliderSlideDto[] {
    if (this.slides.length <= 1) return this.slides;
    return [
      this.slides[this.slides.length - 1],
      ...this.slides,
      this.slides[0],
    ];
  }

  get trackTransform(): string {
    return `translate3d(calc(-${this.trackIndex * 100}% + ${this.dragOffsetPx}px), 0, 0)`;
  }

  get trackTransition(): string {
    return this.isDragging || !this.transitionEnabled
      ? 'none'
      : 'transform 750ms cubic-bezier(0.4, 0, 0.2, 1)';
  }

  goTo(index: number): void {
    if (!this.slides.length || this._loopResetPending) return;
    const target =
      ((index % this.slides.length) + this.slides.length) % this.slides.length;

    if (this.currentIndex === this.slides.length - 1 && target === 0) {
      this.moveToFirstClone();
      return;
    }
    if (this.currentIndex === 0 && target === this.slides.length - 1) {
      this.moveToLastClone();
      return;
    }

    this.currentIndex = target;
    this.trackIndex = target + (this.slides.length > 1 ? 1 : 0);
  }

  next(): void {
    if (this._loopResetPending) return;
    if (this.currentIndex === this.slides.length - 1) {
      this.moveToFirstClone();
      return;
    }
    this.goTo(this.currentIndex + 1);
  }

  prev(): void {
    if (this._loopResetPending) return;
    if (this.currentIndex === 0) {
      this.moveToLastClone();
      return;
    }
    this.goTo(this.currentIndex - 1);
  }

  onMouseEnter(): void {
    this.isPaused = true;
    this.stopAutoplay();
  }

  onMouseLeave(): void {
    this.isPaused = false;
    if (!this.isDragging) this.startAutoplay();
  }

  onPointerDown(event: PointerEvent): void {
    if (this.slides.length <= 1 || this._loopResetPending) return;
    if (event.pointerType === 'mouse' && event.button !== 0) return;

    this._pointerId = event.pointerId;
    this._dragStartX = event.clientX;
    this._dragStartY = event.clientY;
    this._dragStartTime = performance.now();
    this._viewportWidth = (event.currentTarget as HTMLElement).clientWidth;
    this._didDrag = false;
    this.stopAutoplay();
  }

  onPointerMove(event: PointerEvent): void {
    if (event.pointerId !== this._pointerId) return;

    const deltaX = event.clientX - this._dragStartX;
    const deltaY = event.clientY - this._dragStartY;

    if (!this.isDragging) {
      if (
        Math.abs(deltaX) < HomeSliderComponent.DragStartThresholdPx &&
        Math.abs(deltaY) < HomeSliderComponent.DragStartThresholdPx
      ) {
        return;
      }

      // Leave vertical gestures to normal page scrolling.
      if (Math.abs(deltaY) > Math.abs(deltaX)) {
        this._pointerId = null;
        this.startAutoplay();
        return;
      }

      this.isDragging = true;
      this._didDrag = true;
      (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
    }

    event.preventDefault();
    this.dragOffsetPx = deltaX;
  }

  onPointerUp(event: PointerEvent): void {
    if (event.pointerId !== this._pointerId) return;

    const elapsed = Math.max(1, performance.now() - this._dragStartTime);
    const velocity = this.dragOffsetPx / elapsed;
    const threshold = Math.min(100, this._viewportWidth * 0.18);
    const shouldGoNext =
      this.dragOffsetPx < -threshold ||
      velocity < -HomeSliderComponent.SwipeVelocityThreshold;
    const shouldGoPrevious =
      this.dragOffsetPx > threshold ||
      velocity > HomeSliderComponent.SwipeVelocityThreshold;

    this.finishPointerInteraction(event);

    if (shouldGoNext) this.next();
    else if (shouldGoPrevious) this.prev();

    this.startAutoplay();
  }

  onPointerCancel(event: PointerEvent): void {
    if (event.pointerId !== this._pointerId) return;
    this.finishPointerInteraction(event);
    this.startAutoplay();
  }

  onTrackClick(event: MouseEvent): void {
    if (!this._didDrag) return;
    event.preventDefault();
    event.stopPropagation();
    this._didDrag = false;
  }

  onTrackTransitionEnd(event: TransitionEvent): void {
    if (
      event.target !== event.currentTarget ||
      event.propertyName !== 'transform' ||
      !this._loopResetPending
    ) {
      return;
    }

    this.transitionEnabled = false;
    this.trackIndex =
      this.currentIndex === 0 ? 1 : this.slides.length;
    this._loopResetPending = false;

    // Render the real slide at the exact clone position without animation,
    // then restore transitions for the next interaction.
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        this.transitionEnabled = true;
      });
    });
  }

  private restartAutoplay(): void {
    this.stopAutoplay();
    this.startAutoplay();
  }

  private startAutoplay(): void {
    if (this.isPaused || this.slides.length <= 1) return;
    this._timer = setInterval(() => this.next(), 15_000);
  }

  private stopAutoplay(): void {
    if (this._timer) {
      clearInterval(this._timer);
      this._timer = null;
    }
  }

  private moveToFirstClone(): void {
    if (this.slides.length <= 1) return;
    this.transitionEnabled = true;
    this._loopResetPending = true;
    this.currentIndex = 0;
    this.trackIndex = this.slides.length + 1;
  }

  private moveToLastClone(): void {
    if (this.slides.length <= 1) return;
    this.transitionEnabled = true;
    this._loopResetPending = true;
    this.currentIndex = this.slides.length - 1;
    this.trackIndex = 0;
  }

  private finishPointerInteraction(event: PointerEvent): void {
    const target = event.currentTarget as HTMLElement;
    if (target.hasPointerCapture(event.pointerId)) {
      target.releasePointerCapture(event.pointerId);
    }

    this._pointerId = null;
    this.isDragging = false;
    this.dragOffsetPx = 0;

    // A click is dispatched immediately after pointerup. Keep this flag until
    // that click is suppressed, then clear it if no click was produced.
    setTimeout(() => {
      this._didDrag = false;
    });
  }
}
