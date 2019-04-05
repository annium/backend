using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Annium.Extensions.Conversion
{
    public static class Converter
    {
        private static IList<IConverterInstance> instances = new List<IConverterInstance>();

        private static MethodInfo convertMethod = typeof(Converter).GetMethods()
            .First(e => e.Name == nameof(Convert) && e.IsGenericMethod);

        public static void Register<TSource, TTarget>(Func<TSource, TTarget> converter)
        {
            var instance = FindConverter(typeof(TSource), typeof(TTarget));
            if (instance != null)
                instances.Remove(instance);

            instances.Add(new ConverterInstance<TSource, TTarget>(converter));
        }

        public static bool CanConvert<TSource, TTarget>() =>
            FindConverter(typeof(TSource), typeof(TTarget)) != null;

        public static bool CanConvert(Type sourceType, Type targetType) =>
            FindConverter(sourceType, targetType) != null;

        public static TTarget Convert<TSource, TTarget>(TSource data)
        {
            var source = typeof(TSource);
            var target = typeof(TTarget);

            if (target.IsEnum)
                return (TTarget) Enum.Parse(target, data.ToString(), ignoreCase : true);

            var instance = FindConverter(source, target);
            if (instance == null)
                throw new Exception($"No converter registered for {source.FullName} -> {target.FullName} conversion");

            return (instance as ConverterInstance<TSource, TTarget>).Convert.Invoke(data);
        }

        public static object Convert(object data, Type targetType) =>
            convertMethod.MakeGenericMethod(data.GetType(), targetType).Invoke(null, new [] { data });

        private static IConverterInstance FindConverter(Type sourceType, Type targetType) =>
            instances.FirstOrDefault(e => e.Source == sourceType && e.Target == targetType);

        static Converter()
        {
            DefaultConverters.Register();
        }
    }
}