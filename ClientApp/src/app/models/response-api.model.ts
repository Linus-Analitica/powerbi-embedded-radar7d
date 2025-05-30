export interface ResponseApi<T> {
  succeeded:boolean;
  data: T;
  message: string;
}

