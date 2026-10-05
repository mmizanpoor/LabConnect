import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
} from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { ProductUserReviewDto } from './products.types';

@Component({
  selector: 'app-product-comments',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    TranslocoPipe,
    BaseButtonComponent,
  ],
  templateUrl: './product-comments.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProductCommentsComponent implements OnChanges {
  @Input() reviews: ProductUserReviewDto[] = [];
  @Input() showReplyForm = false;
  @Input() canSubmitReview = false;
  @Input() hasPendingReview = false;
  @Input() isAuthenticated = false;
  @Input() submitLoading = false;
  @Input() submitError = '';
  @Input() submitSuccess = '';
  @Input() replyLoading = false;
  @Input() replyError = '';
  @Input() replySuccess = '';

  @Output() submitReview = new EventEmitter<{ rating: number; comment: string }>();
  @Output() submitReply = new EventEmitter<{ reviewId: string; comment: string }>();

  protected ratingControl = new FormControl(0, [Validators.required, Validators.min(1), Validators.max(5)]);
  protected commentControl = new FormControl('', Validators.required);
  protected replyReviewId = '';
  protected replyCommentControl = new FormControl('', Validators.required);
  protected hoveredStar = 0;

  constructor(private _cdr: ChangeDetectorRef) {}

  protected formatJalaliDate(value: string): string {
    return new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }).format(new Date(value));
  }

  protected reviewReplies(review: ProductUserReviewDto) {
    return review.replies ?? [];
  }

  protected canReplyTo(review: ProductUserReviewDto): boolean {
    return this.showReplyForm && this.reviewReplies(review).length === 0;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['submitSuccess']?.currentValue) {
      this.resetSubmitForm();
    }
    if (changes['replyLoading']?.previousValue === true && this.replyLoading === false && !this.replyError) {
      this.cancelReply();
    }
    this._cdr.markForCheck();
  }

  protected setRating(value: number): void {
    this.ratingControl.setValue(value);
  }

  protected starClass(value: number): string {
    const activeRating = this.hoveredStar || this.ratingControl.value || 0;
    return value <= activeRating ? 'is-filled' : '';
  }

  protected onSubmitReview(): void {
    if (this.commentControl.invalid || this.ratingControl.invalid) return;
    this.submitReview.emit({
      rating: this.ratingControl.value ?? 0,
      comment: this.commentControl.value ?? '',
    });
  }

  protected resetSubmitForm(): void {
    this.ratingControl.setValue(0);
    this.commentControl.setValue('');
    this.hoveredStar = 0;
  }

  protected startReply(review: ProductUserReviewDto): void {
    if (!this.canReplyTo(review)) return;
    this.replyReviewId = review.id;
    this.replyCommentControl.setValue('');
    this._cdr.markForCheck();
  }

  protected cancelReply(): void {
    this.replyReviewId = '';
    this.replyCommentControl.setValue('');
    this._cdr.markForCheck();
  }

  protected onSubmitReply(): void {
    if (!this.replyReviewId || this.replyCommentControl.invalid) return;
    this.submitReply.emit({
      reviewId: this.replyReviewId,
      comment: this.replyCommentControl.value ?? '',
    });
  }
}
