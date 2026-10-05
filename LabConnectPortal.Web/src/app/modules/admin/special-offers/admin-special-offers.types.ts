import { SpecialOfferDetailDto, SpecialOfferListItemDto } from '../../profile/special-offers/special-offers.types';

export interface AdminSpecialOfferListItem extends SpecialOfferListItemDto {
  labCodeNew: number;
  labName: string;
  requesterLabNames: string;
  createdAt: string;
}

export interface AdminSpecialOfferDetail extends SpecialOfferDetailDto {
  labCodeNew: number;
  labName: string;
  isExpired: boolean;
  requestCount: number;
  createdAt: string;
}
