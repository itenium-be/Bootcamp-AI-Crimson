import axios from 'axios';
import { useAuthStore } from '../stores';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
});

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle 401 responses
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().logout();
    }
    return Promise.reject(error);
  },
);

interface LoginResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
}

export async function loginApi(username: string, password: string): Promise<LoginResponse> {
  const params = new URLSearchParams();
  params.append('grant_type', 'password');
  params.append('username', username);
  params.append('password', password);
  params.append('client_id', 'skillforge-spa');
  params.append('scope', 'openid profile email');

  const response = await axios.post<LoginResponse>(`${API_BASE_URL}/connect/token`, params, {
    headers: {
      'Content-Type': 'application/x-www-form-urlencoded',
    },
  });

  return response.data;
}

export interface Team {
  id: number;
  name: string;
}

export async function fetchUserTeams(): Promise<Team[]> {
  const response = await api.get<Team[]>('/api/team');
  return response.data;
}

export async function requestPasswordReset(email: string): Promise<void> {
  await axios.post(`${API_BASE_URL}/api/auth/password-reset/request`, { email });
}

export async function confirmPasswordReset(email: string, token: string, newPassword: string): Promise<void> {
  await axios.post(`${API_BASE_URL}/api/auth/password-reset/confirm`, { email, token, newPassword });
}

export type CourseStatus = 'Draft' | 'Published' | 'Archived';

export interface Course {
  id: number;
  name: string;
  description: string | null;
  category: string | null;
  level: string | null;
  estimatedDuration: number | null;
  status: CourseStatus;
  createdAt: string;
}

export interface CourseFormData {
  name: string;
  description: string | null;
  category: string | null;
  level: string | null;
  estimatedDuration: number | null;
}

export async function fetchCourses(): Promise<Course[]> {
  const response = await api.get<Course[]>('/api/course');
  return response.data;
}

export async function fetchCourse(id: number): Promise<Course> {
  const response = await api.get<Course>(`/api/course/${id}`);
  return response.data;
}

export async function createCourse(data: CourseFormData): Promise<Course> {
  const response = await api.post<Course>('/api/course', data);
  return response.data;
}

export async function updateCourse(id: number, data: CourseFormData): Promise<Course> {
  const response = await api.put<Course>(`/api/course/${id}`, data);
  return response.data;
}

export async function publishCourse(id: number): Promise<Course> {
  const response = await api.put<Course>(`/api/course/${id}/publish`);
  return response.data;
}

export async function archiveCourse(id: number): Promise<Course> {
  const response = await api.put<Course>(`/api/course/${id}/archive`);
  return response.data;
}

export async function deleteCourse(id: number): Promise<void> {
  await api.delete(`/api/course/${id}`);
}

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  isActive: boolean;
  lastActiveAt: string | null;
}

export async function fetchUsers(): Promise<User[]> {
  const response = await api.get<User[]>('/api/users');
  return response.data;
}

export async function fetchUser(id: string): Promise<User> {
  const response = await api.get<User>(`/api/users/${id}`);
  return response.data;
}

export async function changeUserRole(id: string, role: string): Promise<void> {
  await api.put(`/api/users/${id}/role`, { role });
}

export async function deactivateUser(id: string): Promise<void> {
  await api.put(`/api/users/${id}/deactivate`);
}

export async function activateUser(id: string): Promise<void> {
  await api.put(`/api/users/${id}/activate`);
}

export async function fetchTeamMembers(teamId: number): Promise<User[]> {
  const response = await api.get<User[]>(`/api/team/${teamId}/members`);
  return response.data;
}

export async function fetchAvailableLearners(teamId: number): Promise<User[]> {
  const response = await api.get<User[]>(`/api/team/${teamId}/available-learners`);
  return response.data;
}

export async function addTeamMember(teamId: number, userId: string): Promise<void> {
  await api.post(`/api/team/${teamId}/members`, { userId });
}

export async function removeTeamMember(teamId: number, userId: string): Promise<void> {
  await api.delete(`/api/team/${teamId}/members/${userId}`);
}

export interface Enrollment {
  id: number;
  courseId: number;
  courseName: string;
  courseCategory: string | null;
  courseLevel: string | null;
  enrolledAt: string;
  status: string;
}

export async function enrollCourse(courseId: number): Promise<Enrollment> {
  const response = await api.post<Enrollment>(`/api/courses/${courseId}/enroll`);
  return response.data;
}

export async function fetchMyEnrollments(): Promise<Enrollment[]> {
  const response = await api.get<Enrollment[]>('/api/enrollments/me');
  return response.data;
}

interface QuestionStat {
  questionId: number;
  questionText: string;
  correctRate: number;
}

interface ScoreDistribution {
  range: string;
  count: number;
}

interface QuizAnalytics {
  averageScore: number;
  passRate: number;
  totalAttempts: number;
  uniqueLearners: number;
  questionStats: QuestionStat[];
  scoreDistribution: ScoreDistribution[];
}

interface QuizLearnerAnalyticsItem {
  userId: string;
  userName: string;
  latestScore: number;
  isPassed: boolean;
  completedAt: string;
}

export async function fetchQuizAnalytics(
  quizId: number,
  params?: { dateFrom?: string; dateTo?: string },
): Promise<QuizAnalytics> {
  const response = await api.get<QuizAnalytics>(`/api/quizzes/${quizId}/analytics`, { params });
  return response.data;
}

export async function fetchQuizLearnerAnalytics(quizId: number, teamId?: number): Promise<QuizLearnerAnalyticsItem[]> {
  const response = await api.get<QuizLearnerAnalyticsItem[]>(`/api/quizzes/${quizId}/analytics/learners`, {
    params: teamId ? { teamId } : undefined,
  });
  return response.data;
}
