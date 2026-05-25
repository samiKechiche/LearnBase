export interface User
{
  userId: string;
  username: string;
  email: string;
  password: string;
  createdAt: string;
  updatedAt: string;
}

export interface SignInRequest
{
  email: string;
  password: string;
}

export interface SignUpRequest
{
  username: string;
  email: string;
  password: string;
}