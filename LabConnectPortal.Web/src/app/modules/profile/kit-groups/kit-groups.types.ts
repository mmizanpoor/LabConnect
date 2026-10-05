export interface KitGroupDto {
  id: number;
  title: string;
}

export interface CreateKitGroupCommand {
  title: string;
}

export interface UpdateKitGroupCommand {
  id: number;
  title: string;
}
