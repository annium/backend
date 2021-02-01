using System;
using Annium.Core.Primitives;

namespace Annium.Core.Mapper.Internal.Profiles
{
    internal class EnumProfile<T> : Profile
        where T : struct, Enum
    {
        public EnumProfile()
        {
            Map<T, string>(x => x.ToString());
            Map<string, Enum>(x => x.ParseEnum<T>());
        }
    }
}