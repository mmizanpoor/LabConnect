export interface DeviceGroupDto {
  id: number;
  title: string;
}

export interface CreateDeviceGroupCommand {
  title: string;
}

export interface UpdateDeviceGroupCommand {
  id: number;
  title: string;
}
