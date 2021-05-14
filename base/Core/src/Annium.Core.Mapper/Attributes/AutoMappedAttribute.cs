using System;

namespace Annium.Core.Mapper.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
    public class AutoMappedAttribute : Attribute
    {
    }
}