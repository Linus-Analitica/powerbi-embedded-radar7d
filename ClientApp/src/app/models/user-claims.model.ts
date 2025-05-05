export class UserClaims {
    personID: string;
    userType: string;
    payrollID: string;
    email: string;
    profiles: string;
}

export interface SessionUser {
  user: UserClaims;
  token: string;
  userId: string;
  payrollId: string;
}
