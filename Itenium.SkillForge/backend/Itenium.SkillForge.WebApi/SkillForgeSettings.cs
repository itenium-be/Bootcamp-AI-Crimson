using Itenium.Forge.Core;
using Itenium.Forge.Settings;

namespace Itenium.SkillForge.WebApi;

public class SkillForgeSettings : IForgeSettings
{
    public ForgeSettings Forge { get; set; } = new();
}
