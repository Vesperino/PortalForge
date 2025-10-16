export interface User {
  id: string
  email: string
  role?: UserRole
  createdAt?: string
}

export enum UserRole {
  Admin = 'admin',
  Manager = 'manager',
  HR = 'hr',
  Marketing = 'marketing',
  Employee = 'employee'
}

export interface LoginCredentials {
  email: string
  password: string
}

export interface RegisterCredentials {
  email: string
  password: string
  confirmPassword: string
}

export interface AuthResponse {
  user: User
  accessToken?: string
}

export interface AuthError {
  message: string
  statusCode?: number
}

export interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}
