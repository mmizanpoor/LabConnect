/**
 * Stable module identifiers used for future per-entity access control.
 * Values must stay in sync with LabConnectPortal.Api.Domain.SystemEntity.
 * Names are shared across admin/profile (not role-prefixed).
 */
export class SystemEntity {
  static readonly HeaderName = 'X-System-Entity';

  static readonly Profile = 'f4f47b06-f392-49ee-9c11-5e694b7fa345';
  static readonly Resume = 'd80d4c21-1e22-46ce-9ab8-da2e43977008';
  static readonly CenterProfile = '95d6e23c-744a-48ef-bcdb-a4321b7c1152';
  static readonly Location = 'b9246a45-0941-42a9-9fa3-5fcc08f1fbff';
  static readonly JobPosting = 'eec0d6a3-edaf-46ba-b76a-5562b4390367';
  static readonly ProductListing = '28a46134-e146-41a1-81c3-eadba93e6a4d';
  static readonly ProductOrder = 'd7af789a-4d3d-4dfa-95c8-b830b414c076';
  static readonly LabUser = '0d11744c-c2b6-4039-bada-b830a37423e8';
  static readonly DeviceGroup = '061610d2-cb2d-4dbd-8050-effaff7fe16e';
  static readonly KitGroup = '2282c3b6-aebf-4825-a112-3197078602b8';
  static readonly TestInfo = '70e98d14-1a84-4607-a4b1-97438ffe1226';
  static readonly SpecialOffer = '2f3cf390-447d-443c-8d66-6096c82c4e6d';
  static readonly Reception = 'f9208770-3e4b-4261-9fad-6015f5050b9e';
  static readonly LabAgreement = 'a795fa46-a728-47e5-b103-5d4f0c157972';
  static readonly Dashboard = '9b2dd53b-db50-42b3-a562-0828eb0fc19e';
  static readonly User = '69abf749-85b9-4c6b-ae5f-5a9998a1767d';
  static readonly Shop = 'c8b60aa4-2b97-45bb-bcc9-2e64628d1a1a';
  static readonly Laboratory = '1a30bfe0-5159-4fe3-bf9c-f0da657cabc0';
  static readonly ProductCategory = 'a2988d15-00e2-4b8c-b975-d8945e45ec8d';
  static readonly ProductAttribute = 'b1169934-8842-4f5c-8cf4-6b26ea086e55';
  static readonly Brand = 'e1b65f43-d142-4902-ad0c-70382dea535b';
  static readonly Product = '504586dc-bd75-4f6f-972b-c7fd583a9f8f';
  static readonly ProductReview = '90673da1-e1cb-42a2-82c4-08e2a2705822';
  static readonly ContentGroup = 'aecc303a-0a4e-4044-ab32-8cd99b7c4de4';
  static readonly Post = '21543e8f-3250-4bcd-8f8a-722ac83301aa';
  static readonly ContentNews = 'c4f1a8e2-3b57-4d91-9e26-7a0b5c8d2f14';
  static readonly ContentArticles = 'd5a2b9f3-4c68-4e02-af37-8b1c6d9e3025';
  static readonly ContentDocuments = 'e6b3c0a4-5d79-4f13-b048-9c2d7e0f4136';
  static readonly ContentAds = 'f7c4d1b5-6e8a-4024-c159-ad3e8f105247';
  static readonly SliderGroup = '82db59eb-1439-44f7-b4d8-eaed4880b804';
  static readonly Message = 'd6679b99-cf50-4474-ae8a-50889b800bcf';
  static readonly Settings = '7e320a52-136c-43cb-a65c-6753e959475d';
  static readonly SiteUser = 'c3e8a1f2-5b74-4d9e-9a62-1f8e0d4c7b35';
  static readonly ActivityLog = 'e5a1c8b3-7d4f-4e2a-9c61-8b0f3a5d2e17';
  static readonly LoginReport = 'b7c4e2a1-9f83-4d56-8e10-2a6c5d9b4f71';
  static readonly CompanyRegulation = 'a1d8e4f2-6c39-4b7a-9e15-3f8d0c2a5b64';
  static readonly SiteService = 'b2e9f5a3-7d4c-4e8b-9f21-4a7c6e0d3b58';

  /** Entities manageable via UserLab permissions (sync with API SystemEntity.LabPortalAll). */
  static readonly LabPortalAll: readonly string[] = [
    SystemEntity.CenterProfile,
    SystemEntity.Location,
    SystemEntity.JobPosting,
    SystemEntity.Product,
    SystemEntity.LabUser,
    SystemEntity.DeviceGroup,
    SystemEntity.KitGroup,
    SystemEntity.TestInfo,
    SystemEntity.SpecialOffer,
    SystemEntity.Reception,
    SystemEntity.LabAgreement,
    SystemEntity.Message,
    SystemEntity.ActivityLog,
  ];

  /** Entities assignable for UserType.Admin (sync with API SystemEntity.SiteAdminAll). */
  static readonly SiteAdminAll: readonly string[] = [
    SystemEntity.User,
    SystemEntity.Shop,
    SystemEntity.Laboratory,
    SystemEntity.ProductCategory,
    SystemEntity.ProductAttribute,
    SystemEntity.Brand,
    SystemEntity.Product,
    SystemEntity.ContentGroup,
    SystemEntity.Post,
    SystemEntity.ContentNews,
    SystemEntity.ContentArticles,
    SystemEntity.ContentDocuments,
    SystemEntity.ContentAds,
    SystemEntity.SliderGroup,
    SystemEntity.SpecialOffer,
    SystemEntity.Message,
    SystemEntity.Settings,
    SystemEntity.ActivityLog,
    SystemEntity.LoginReport,
    SystemEntity.CompanyRegulation,
  ];
}
