export interface LoginRequestDto {
  userName: string;
  password: string;
  rememberMe: boolean;
}

export interface LoginResponseDto {
  userName: string;
  displayName: string;
  token: string;
  expiresAtUtc: string;
}

export interface AuthSession {
  userName: string;
  displayName: string;
  token: string;
  expiresAtUtc: string;
}
