export interface OrganizationLocationDto {
  locationId: string;
  locationName: string;
  provinceId: number;
  provinceName: string;
  address: string;
  phoneNumber: string;
}

export interface CreateOrganizationLocationCommand {
  locationName: string;
  provinceId: number;
  address: string;
  phoneNumber: string;
}

export interface UpdateOrganizationLocationCommand extends CreateOrganizationLocationCommand {
  locationId: string;
}
