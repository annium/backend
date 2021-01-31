using System;

namespace Annium.Core.Runtime.Types
{
    /// <summary>
    /// Property, marked by this attribute, must contain Type.GetId() int value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ResolutionIdAttribute : Attribute
    {
    }
}