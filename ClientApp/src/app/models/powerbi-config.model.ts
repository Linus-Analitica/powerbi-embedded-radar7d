
export interface PowerBiConfig {
    TenantID: string;
    ClientID: string;
    ClientSecret: string; // ¡Cuidado con exponer esto!
    WorkspaceId: string;
    ReportCurrentId: string;
    ReportArchivedId: string;
  }
