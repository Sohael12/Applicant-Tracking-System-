using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
namespace Stageproject_ATS_AP2025Q2.Utilities;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()?
                        .GetName() ?? enumValue.ToString();
    }
}
