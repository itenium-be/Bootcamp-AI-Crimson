namespace Itenium.SkillForge.WebApi.Controllers;

public record LessonDto(int Id, string Title, int? EstimatedDuration, int SortOrder);
public record CreateLessonRequest(string Title, int? EstimatedDuration, int SortOrder);
public record UpdateLessonRequest(string Title, int? EstimatedDuration, int SortOrder);
public record ReorderLessonsRequest(int[] OrderedLessonIds);
