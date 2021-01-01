using System;

namespace DeepDungeonDex
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DoNotShowInHelpAttribute : Attribute
    {
    }
}