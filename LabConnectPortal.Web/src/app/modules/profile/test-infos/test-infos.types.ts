export interface TestInfoListItemDto {
  id: number;
  cpnCode?: string | null;
  nationalCode?: string | null;
  fullName?: string | null;
  shortName?: string | null;
  sectionName?: string | null;
  approvePrice?: number | null;
}

export interface TestInfoDetailDto {
  id: number;
  labCode: number;
  labCodeNew: number;
  cpnCode?: string | null;
  nationalCode?: string | null;
  measurName?: string | null;
  fullName?: string | null;
  shortName?: string | null;
  sectionName?: string | null;
  similarName?: string | null;
  kd?: string | null;
  volume?: string | null;
  minVolume?: string | null;
  maintenance?: string | null;
  transportation?: string | null;
  needs?: string | null;
  guidance?: string | null;
  patientInfo?: string | null;
  denial?: string | null;
  preparation?: string | null;
  clinicalInfo?: string | null;
  sources?: string | null;
  comment?: string | null;
  caution?: string | null;
  sClinical?: string | null;
  detail?: string | null;
  date?: string | null;
  resultDuration?: string | null;
  maxDurResult?: string | null;
  maintenanceDur?: string | null;
  testId: number;
  criteria?: string | null;
  freezer?: string | null;
  deliveryCondition?: string | null;
  approvePrice?: number | null;
  kitGroupId?: number | null;
  kitGroupTitle?: string | null;
  deviceGroupId?: number | null;
  deviceGroupTitle?: string | null;
}

export interface UpdateTestInfoCommand {
  id: number;
  approvePrice?: number | null;
  kitGroupId?: number | null;
  deviceGroupId?: number | null;
}

export interface UpdateTestInfoApprovePriceCommand {
  id: number;
  approvePrice?: number | null;
}
