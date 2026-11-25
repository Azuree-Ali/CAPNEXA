export interface LoginDto {
  email: string;
  password: string;
}
export interface AuthResponseDto {
  accessToken: string;
  tokenType: string;
  expiresInSeconds: number;
  refreshToken: string;
  user: UserResponseDto;
}
export interface UserResponseDto {
  id: string;
  email: string | null;
  firstName: string;
  lastName: string;
  roles: string[];
}
