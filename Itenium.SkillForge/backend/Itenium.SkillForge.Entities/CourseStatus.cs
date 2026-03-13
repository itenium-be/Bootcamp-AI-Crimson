using System.Text.Json.Serialization;

namespace Itenium.SkillForge.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CourseStatus
{
    Draft = 0,
    Published = 1,
    Archived = 2,
}
