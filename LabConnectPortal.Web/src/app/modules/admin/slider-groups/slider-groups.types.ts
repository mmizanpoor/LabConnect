export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface GetSliderGroupsQuery {
  title?: string;
  isActive?: boolean | null;
  page?: number;
  pageSize?: number;
}

export interface SliderGroupListItemDto {
  id: string;
  title: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  sortOrder: number;
  slideCount: number;
  updatedAt: string;
}

export interface SliderSlideDto {
  id: string;
  sliderGroupId: string;
  imagePath: string;
  linkUrl?: string | null;
  title: string;
  sortOrder: number;
}

export interface SliderGroupDto {
  id: string;
  title: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  sortOrder: number;
  createdAt: string;
  updatedAt: string;
  slides: SliderSlideDto[];
}

export interface SaveSliderGroupCommand {
  title: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  sortOrder: number;
}

export interface UpdateSliderGroupCommand extends SaveSliderGroupCommand {
  id: string;
}

export interface UpdateSliderSlideCommand {
  id: string;
  sliderGroupId: string;
  linkUrl?: string | null;
  title: string;
  sortOrder: number;
}

export interface DeleteSliderSlideCommand {
  id: string;
  sliderGroupId: string;
}

export interface ReorderSlidesCommand {
  sliderGroupId: string;
  slideIds: string[];
}

export interface ActiveSliderSlideDto {
  id: string;
  imagePath: string;
  linkUrl?: string | null;
  title: string;
  sortOrder: number;
}
