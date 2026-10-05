export interface SRLabNameDto {
  intLabId: number;
  intLabIdNew?: number | null;
  vchLabName?: string | null;
}

/** Matches API `AgreementDirection` (Received = 1, Sent = 2). */
export enum AgreementDirection {
  Received = 1,
  Sent = 2,
}

export interface GetLabAgreementQuery {
  agreementDirection: AgreementDirection;
  primaryLabCodeNew: number;
  startDateTime: string;
  laboratoryAgreementStates?: (number | null)[] | null;
}

export interface LabAgreementAttachmentDto {
  id?: number | null;
  remark?: string | null;
  fileName: string;
  contentType?: string | null;
}

export interface LabAgreementTestPriceDto {
  id: number;
  testId: number;
  testName: string;
  approved: number;
  baseTariffApproved: number;
  firstAdditions: number;
  secondAdditions: number;
  urgentAmount: number;
  cpnCode?: string | null;
  nationalCode?: string | null;
  addendumTitle?: string | null;
}

export enum PartyActionType {
  None = 0,
  Submitted = 1,
  Seen = 2,
  Signed = 3,
  Rejected = 4,
  Suspended = 5,
  Terminated = 6,
  CanceledRejected = 7,
  CanceledSuspend = 8,
  CanceledTermination = 9,
}

export interface LabAgreementDto {
  id?: number | null;
  samanehId?: number | null;
  startDate: string;
  expDate: string;
  contractNumber: string;
  receiverAgreementLabCodeNew: number;
  receiverAgreementLabId?: number;
  primaryAgreementLabCodeNew: number;
  title: string;
  text?: string | null;
  laboratoryAgreementState: number;
  primaryAgreementId?: number;
  primaryAgreementSign?: string | null;
  primaryAgreementSignDateTime?: string | null;
  receiverAgreementSign?: string | null;
  receiverAgreementSignDateTime?: string | null;
  receiverAgreementUsername?: string | null;
  primaryReturnCause?: string | null;
  receiverReturnCause?: string | null;
  parentId?: number | null;
  isAddendum?: boolean | null;
  getSampling?: boolean | null;
  getRecept?: boolean | null;
  primaryAction?: number | null;
  receiverAction?: number | null;
  primaryActionUserName?: string | null;
  receiverActionUserName?: string | null;
  primaryActionDateTime?: string | null;
  receiverActionDateTime?: string | null;
  childrenCount: number;
  attachmentCount: number;
  testPriceCount?: number;
  children?: LabAgreementDto[] | null;
  attachments?: LabAgreementAttachmentDto[] | null;
  testPrices?: LabAgreementTestPriceDto[] | null;
  mergedTestPrices?: LabAgreementTestPriceDto[] | null;
}

export interface LabAgreementStats {
  activeCount: number;
  expiredCount: number;
  pendingCount: number;
}

export interface LabAgreementLabStats {
  sent: LabAgreementStats;
  received: LabAgreementStats;
}
