import { NgTemplateOutlet } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  contentPostDetailCommands,
  PublicPostCardDto,
} from '../admin/content/content.types';
import { PublicPostsService } from './public-posts.service';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [RouterLink, NgTemplateOutlet],
  templateUrl: './post-card.component.html',
})
export class PostCardComponent {
  @Input({ required: true }) post!: PublicPostCardDto;
  @Input() layout: 'default' | 'home' = 'default';

  detailLink(): string[] {
    return contentPostDetailCommands(this.post.type, this.post.contentPostId);
  }

  imageUrl(): string {
    const width = this.layout === 'home' ? 480 : undefined;
    return PublicPostsService.featuredImageUrl(this.post.featuredImagePath, width);
  }

  imageAspectRatio(): string {
    return this.layout === 'home' ? '4 / 3' : '16 / 9';
  }

  formattedDate(): string {
    if (!this.post.publishedAt) return '';
    return new Intl.DateTimeFormat('fa-IR', { dateStyle: 'medium' }).format(new Date(this.post.publishedAt));
  }
}
