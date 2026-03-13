using System.Text.Json.Serialization;

namespace Itenium.SkillForge.Entities;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssigneeType
{
    Team,
    User,
}
