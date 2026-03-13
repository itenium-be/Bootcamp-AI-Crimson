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

// ---- SSO / PKCE helpers ----

function generateCodeVerifier(): string {
  const array = new Uint8Array(32);
  crypto.getRandomValues(array);
  return btoa(String.fromCharCode(...array))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');
}

async function generateCodeChallenge(verifier: string): Promise<string> {
  const data = new TextEncoder().encode(verifier);
  const digest = await crypto.subtle.digest('SHA-256', data);
  return btoa(String.fromCharCode(...new Uint8Array(digest)))
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=/g, '');
}

export async function initiateSsoLogin(): Promise<void> {
  const verifier = generateCodeVerifier();
  const challenge = await generateCodeChallenge(verifier);
  sessionStorage.setItem('pkce_verifier', verifier);

  const params = new URLSearchParams({
    client_id: 'skillforge-spa',
    response_type: 'code',
    scope: 'openid profile email',
    redirect_uri: `${window.location.origin}/callback`,
    code_challenge: challenge,
    code_challenge_method: 'S256',
  });

  window.location.href = `${API_BASE_URL}/connect/authorize?${params.toString()}`;
}

export async function exchangeSsoCode(code: string): Promise<string> {
  const verifier = sessionStorage.getItem('pkce_verifier');
  if (!verifier) throw new Error('No PKCE verifier found');
  sessionStorage.removeItem('pkce_verifier');

  const params = new URLSearchParams({
    grant_type: 'authorization_code',
    code,
    client_id: 'skillforge-spa',
    redirect_uri: `${window.location.origin}/callback`,
    code_verifier: verifier,
  });

  const response = await axios.post<LoginResponse>(`${API_BASE_URL}/connect/token`, params, {
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
  });

  return response.data.access_token;
}

// ---- Password auth ----

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

export async function loginWithEmailApi(email: string, password: string): Promise<LoginResponse> {
  const response = await axios.post<LoginResponse>(`${API_BASE_URL}/auth/login`, { email, password });
  return response.data;
}

interface Team {
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

export type AssigneeType = 'Team' | 'User';
export type AssignmentType = 'Mandatory' | 'Optional';

export interface CourseAssignment {
  id: number;
  courseId: number;
  assigneeType: AssigneeType;
  assigneeId: string;
  assigneeName: string | null;
  type: AssignmentType;
  assignedAt: string;
  assignedBy: string;
}

interface CreateAssignmentData {
  assigneeType: AssigneeType;
  assigneeId: string;
  assigneeName: string | null;
  type: AssignmentType;
  assignedBy: string;
}

export async function fetchCourseAssignments(courseId: number): Promise<CourseAssignment[]> {
  const response = await api.get<CourseAssignment[]>(`/api/courses/${courseId}/assignments`);
  return response.data;
}

export async function createCourseAssignment(courseId: number, data: CreateAssignmentData): Promise<CourseAssignment> {
  const response = await api.post<CourseAssignment>(`/api/courses/${courseId}/assignments`, data);
  return response.data;
}

export async function deleteCourseAssignment(courseId: number, assignmentId: number): Promise<void> {
  await api.delete(`/api/courses/${courseId}/assignments/${assignmentId}`);
}

interface User {
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

interface Enrollment {
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

export interface ResumeInfo {
  lessonId: number | null;
  isComplete: boolean;
}

export async function fetchResumeLesson(courseId: number): Promise<ResumeInfo> {
  const response = await api.get<ResumeInfo>(`/api/courses/${courseId}/resume`);
  return response.data;
}

export async function trackLastVisited(courseId: number, lessonId: number): Promise<void> {
  await api.put(`/api/courses/${courseId}/last-visited/${lessonId}`);
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

export type LessonStatus = 'new' | 'done' | 'later';

export interface Lesson {
  id: number;
  title: string;
  sortOrder: number;
  status: LessonStatus;
}

export async function fetchLessons(courseId: number): Promise<Lesson[]> {
  const response = await api.get<Lesson[]>('/api/lessons', { params: { courseId } });
  return response.data;
}

export async function setLessonStatus(lessonId: number, status: LessonStatus): Promise<void> {
  await api.put(`/api/lessons/${lessonId}/status`, { status });
}

export interface LessonProgressSummary {
  lessonId: number;
  completedCount: number;
}

export async function fetchLessonProgressSummary(lessonId: number): Promise<LessonProgressSummary> {
  const response = await api.get<LessonProgressSummary>(`/api/lessons/${lessonId}/progress-summary`);
  return response.data;
}

export async function resetLessonProgress(lessonId: number): Promise<void> {
  await api.delete(`/api/lessons/${lessonId}/progress`);
}

export interface LessonItem {
  id: number;
  title: string;
  estimatedDuration: number | null;
  sortOrder: number;
}

export interface CreateLessonData {
  title: string;
  estimatedDuration: number | null;
  sortOrder: number;
}

interface UpdateLessonData {
  title: string;
  estimatedDuration: number | null;
  sortOrder: number;
}

export async function fetchCourseLessons(courseId: number): Promise<LessonItem[]> {
  const response = await api.get<LessonItem[]>(`/api/courses/${courseId}/lessons`);
  return response.data;
}

export async function createLesson(courseId: number, data: CreateLessonData): Promise<LessonItem> {
  const response = await api.post<LessonItem>(`/api/courses/${courseId}/lessons`, data);
  return response.data;
}

export async function updateLesson(id: number, data: UpdateLessonData): Promise<void> {
  await api.put(`/api/lessons/${id}`, data);
}

export async function deleteLesson(id: number): Promise<void> {
  await api.delete(`/api/lessons/${id}`);
}

export async function reorderLessons(courseId: number, orderedLessonIds: number[]): Promise<void> {
  await api.put(`/api/courses/${courseId}/lessons/reorder`, { orderedLessonIds });
}

export async function completeLesson(lessonId: number): Promise<void> {
  await api.post(`/api/lessons/${lessonId}/complete`);
}

export interface FeedbackEntry {
  id: number;
  userId: string | null;
  rating: number;
  comment: string | null;
  submittedAt: string;
  isFlagged: boolean;
}

interface CourseFeedbackSummary {
  averageRating: number;
  entries: FeedbackEntry[];
}

export interface CourseFeedbackRanking {
  courseId: number;
  courseName: string;
  averageRating: number;
  count: number;
}

export async function getCourseFeedback(courseId: number, minRating?: number): Promise<CourseFeedbackSummary> {
  const response = await api.get<CourseFeedbackSummary>(`/api/courses/${courseId}/feedback`, {
    params: minRating != null ? { minRating } : undefined,
  });
  return response.data;
}

export async function flagFeedback(id: number): Promise<void> {
  await api.put(`/api/feedback/${id}/flag`);
}

export async function dismissFeedback(id: number): Promise<void> {
  await api.put(`/api/feedback/${id}/dismiss`);
}

export async function getFeedbackSummary(): Promise<CourseFeedbackRanking[]> {
  const response = await api.get<CourseFeedbackRanking[]>('/api/reports/feedback-summary');
  return response.data;
}

export interface ModuleCourse {
  courseId: number;
  courseName: string;
  order: number;
}

export interface Module {
  id: number;
  name: string;
  description: string | null;
  goal: string | null;
  courses: ModuleCourse[];
}

interface ModuleFormData {
  name: string;
  description: string | null;
  goal: string | null;
}

export async function fetchModules(): Promise<Module[]> {
  const response = await api.get<Module[]>('/api/modules');
  return response.data;
}

export async function createModule(data: ModuleFormData): Promise<Module> {
  const response = await api.post<Module>('/api/modules', data);
  return response.data;
}

export async function updateModule(id: number, data: ModuleFormData): Promise<void> {
  await api.put(`/api/modules/${id}`, data);
}

export async function deleteModule(id: number): Promise<void> {
  await api.delete(`/api/modules/${id}`);
}

export async function addCourseToModule(moduleId: number, courseId: number, order: number): Promise<void> {
  await api.post(`/api/modules/${moduleId}/courses`, { courseId, order });
}

export async function removeCourseFromModule(moduleId: number, courseId: number): Promise<void> {
  await api.delete(`/api/modules/${moduleId}/courses/${courseId}`);
}

export async function reorderModuleCourses(moduleId: number, orderedCourseIds: number[]): Promise<void> {
  await api.put(`/api/modules/${moduleId}/courses/reorder`, { orderedCourseIds });
}

export type ContentBlockType = 'text' | 'image' | 'video' | 'pdf' | 'link' | 'youtube';

export interface ContentBlock {
  id: number;
  lessonId: number;
  type: ContentBlockType;
  content: string;
  order: number;
}

interface ContentBlockFormData {
  type: ContentBlockType;
  content: string;
  order: number;
}

export async function fetchContentBlocks(lessonId: number): Promise<ContentBlock[]> {
  const response = await api.get<ContentBlock[]>(`/api/lessons/${lessonId}/content-blocks`);
  return response.data;
}

export async function createContentBlock(lessonId: number, data: ContentBlockFormData): Promise<ContentBlock> {
  const response = await api.post<ContentBlock>(`/api/lessons/${lessonId}/content-blocks`, data);
  return response.data;
}

export async function updateContentBlock(
  lessonId: number,
  blockId: number,
  data: ContentBlockFormData,
): Promise<ContentBlock> {
  const response = await api.put<ContentBlock>(`/api/lessons/${lessonId}/content-blocks/${blockId}`, data);
  return response.data;
}

export async function deleteContentBlock(lessonId: number, blockId: number): Promise<void> {
  await api.delete(`/api/lessons/${lessonId}/content-blocks/${blockId}`);
}

export async function reorderContentBlocks(lessonId: number, orderedIds: number[]): Promise<void> {
  await api.put(`/api/lessons/${lessonId}/content-blocks/reorder`, { orderedIds });
}

export interface ReportSummary {
  activeLearners: number;
  completionsThisMonth: number;
  totalEnrollments: number;
}

export interface CourseUsage {
  courseId: number;
  courseName: string;
  totalEnrollments: number;
  completions: number;
  completionRate: number;
}

export async function fetchReportSummary(): Promise<ReportSummary> {
  const response = await api.get<ReportSummary>('/api/reports/summary');
  return response.data;
}

export async function fetchCourseUsage(params?: {
  from?: string;
  to?: string;
  courseId?: number;
}): Promise<CourseUsage[]> {
  const response = await api.get<CourseUsage[]>('/api/reports/course-usage', { params });
  return response.data;
}
