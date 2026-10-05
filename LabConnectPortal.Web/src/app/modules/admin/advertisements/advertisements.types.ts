export interface AdvertisementDto {
  advertisementId: string;
  title: string;
  shortDescription: string;
  imagePath?: string | null;
  hasImage: boolean;
  startAt: string;
  endAt: string;
  isActive: boolean;
  createdAt: string;
  createdByUserId: string;
  createdByUserName: string;
}

export interface SaveAdvertisementCommand {
  title: string;
  shortDescription: string;
  startAt: string;
  endAt: string;
  isActive: boolean;
}

export interface UpdateAdvertisementCommand extends SaveAdvertisementCommand {
  advertisementId: string;
}

export interface PublicAdvertisementCardDto {
  advertisementId: string;
  title: string;
  shortDescription: string;
  imagePath?: string | null;
  productImagePath?: string | null;
}
