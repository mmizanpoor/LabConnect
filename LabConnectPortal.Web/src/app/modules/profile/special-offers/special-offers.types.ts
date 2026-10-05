export interface SpecialOfferLabMonthlyRow {
  labCodeNew: number;
  labName: string;
  offerCount: number;
}

export interface SpecialOfferDashboardStats {
  monthlyOffersByLab: SpecialOfferLabMonthlyRow[];
}

export interface SpecialOfferListItemDto {
  id: number;
  title: string;
  summary: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  testCount: number;
  requestCount: number;
  isExpired: boolean;
}

export interface SpecialOfferTestItemDto {
  testInfoId: number;
  cpnCode?: string | null;
  nationalCode?: string | null;
  fullName?: string | null;
  shortName?: string | null;
  sectionName?: string | null;
  approvePrice?: number | null;
  discount?: number | null;
  maxSamples?: number | null;
}

export interface SpecialOfferDetailDto {
  id: number;
  title: string;
  summary: string;
  fullBody: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  tests: SpecialOfferTestItemDto[];
}

export interface SaveSpecialOfferTestCommand {
  testInfoId: number;
  discount?: number | null;
  maxSamples?: number | null;
}

export interface CreateSpecialOfferCommand {
  title: string;
  summary: string;
  fullBody: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  tests: SaveSpecialOfferTestCommand[];
}

export interface UpdateSpecialOfferCommand extends CreateSpecialOfferCommand {
  id: number;
}

export interface SpecialOfferRequestDto {
  id: number;
  userId: string;
  userDisplayName: string;
  mobileNumber: string;
  description: string;
  status: SpecialOfferRequestStatus;
  rejectionReason?: string | null;
  requesterLabCodeNew: number;
  requesterLabName: string;
  labAgreementId?: number | null;
  createdAt: string;
  reviewedAt?: string | null;
}

export enum SpecialOfferRequestStatus {
  Pending = 0,
  Rejected = 1,
  AwaitingContractCreation = 2,
  ContractCreated = 3,
}

export interface SubmitSpecialOfferRequestCommand {
  description?: string | null;
}

export interface RejectSpecialOfferRequestCommand {
  requestId: number;
  reason: string;
}

export interface SpecialOfferRequestForAgreementDto {
  requestId: number;
  specialOfferId: number;
  offerTitle: string;
  offerStartDate: string;
  offerEndDate: string;
  description: string;
  primaryLabCodeNew: number;
  primaryLabName: string;
  receiverLabCodeNew: number;
  receiverLabName: string;
  isAddendum: boolean;
  parentId?: number | null;
  activeAgreementContractNumber?: string | null;
  activeAgreementExpDate?: string | null;
  tests: SpecialOfferTestItemDto[];
}

export interface CreateLabAgreementFromPortalCommand {
  specialOfferRequestId?: number | null;
  contractNumber: string;
  title: string;
  text: string;
  startDate: string;
  expDate: string;
  primaryAgreementLabCodeNew: number;
  receiverAgreementLabCodeNew: number;
  isAddendum?: boolean;
  parentId?: number | null;
  attachments: PortalLabAgreementAttachmentCommand[];
  testPrices: PortalLabAgreementTestPriceCommand[];
}

export interface PortalLabAgreementAttachmentCommand {
  remark?: string | null;
  fileName: string;
  contentType: string;
}

export interface PortalLabAgreementTestPriceCommand {
  testInfoId: number;
}

export interface PublicSpecialOfferCardDto {
  id: number;
  title: string;
  summary: string;
  endDate: string;
  labName: string;
  labProfileId?: string | null;
  hasLabLogo: boolean;
}

export interface PublicSpecialOfferDetailDto {
  id: number;
  title: string;
  summary: string;
  fullBody: string;
  startDate: string;
  endDate: string;
  labName: string;
  proposerFirstName: string;
  labCodeNew: number;
  hasSubmittedRequest: boolean;
  canSubmitRequest: boolean;
  isOwnLabOffer: boolean;
  tests: PublicSpecialOfferTestDto[];
}

export interface PublicSpecialOfferTestDto {
  cpnCode?: string | null;
  nationalCode?: string | null;
  fullName?: string | null;
  shortName?: string | null;
  sectionName?: string | null;
  approvePrice?: number | null;
  discount?: number | null;
  maxSamples?: number | null;
}

export interface SpecialOfferTestRow {
  testInfoId: number;
  cpnCode?: string | null;
  nationalCode?: string | null;
  fullName?: string | null;
  shortName?: string | null;
  sectionName?: string | null;
  approvePrice?: number | null;
  selected: boolean;
  discount: number | null;
  maxSamples: number | null;
}
