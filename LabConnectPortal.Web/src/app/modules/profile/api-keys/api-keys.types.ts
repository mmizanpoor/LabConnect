export interface ApiKeyDto {
  id: string;
  name: string;
  keyName: string;
  createDate: string;
  allowAdd: boolean;
  allowEdit: boolean;
  allowView: boolean;
  centerProfileId: string;
  centerName: string;
}

export interface CreateApiKeyCommand {
  name: string;
  allowAdd: boolean;
  allowEdit: boolean;
  allowView: boolean;
}

export interface UpdateApiKeyCommand extends CreateApiKeyCommand {
  id: string;
}
