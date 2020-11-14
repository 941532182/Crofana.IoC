using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Crofana.Extension.Reflection;

namespace Crofana.IoC
{
    public class SimpleCrofanaObjectFactory : ICrofanaObjectFactory
    {

        private static Type[] EMPTY_TYPE_ARRAY = { };

        private Dictionary<Type, object> objectMap = new Dictionary<Type, object>();

        public object GetObject(Type type)
        {
            if (!type.HasAttributeRecursive<CrofanaObjectAttribute>())
            {
                return null;
            }
            switch (type.GetCustomAttribute<InstantiationPolicyAttribute>()?.Policy)
            {
                case null:
                case EInstantiationPolicy.Singleton:
                    if (!objectMap.ContainsKey(type))
                    {
                        objectMap[type] = NewObject(type);
                    }
                    return objectMap[type];
                case EInstantiationPolicy.Prototype:
                case EInstantiationPolicy.Pooled:
                    return NewObject(type);
                default:
                    return null;
            }

        }

        public T GetObject<T>() where T : class => GetObject(typeof(T)) as T;

        private object NewObject(Type type)
        {
            object obj = type.GetConstructor(EMPTY_TYPE_ARRAY).Invoke(null);
            ProcessDependencyInjection(obj);
            return obj;
        }

        private void ProcessDependencyInjection(object obj)
        {
            BroadcastPreDependencyInjection(obj);

            obj.GetType()
               .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
               .Where(x => (x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property) && x.HasAttributeRecursive<AutowiredAttribute>())
               .ToList()
               .ForEach(x =>
               {
                   if (x.MemberType == MemberTypes.Field)
                   {
                       FieldInfo field = x as FieldInfo;
                       if (field != null && field.FieldType.HasAttribute<CrofanaObjectAttribute>())
                       {
                           field.SetValue(obj, GetObject(field.FieldType));
                       }
                   }
                   else
                   {
                       PropertyInfo prop = x as PropertyInfo;
                       if (prop != null && prop.SetMethod != null && prop.PropertyType.HasAttribute<CrofanaObjectAttribute>())
                       {
                           prop.SetMethod.Invoke(obj, new object[] { GetObject(prop.PropertyType) });
                       }
                   }
               });

            BroadcastPostDependencyInjection(obj);
        }

        private void BroadcastPreDependencyInjection(object obj)
        {

        }

        private void BroadcastPostDependencyInjection(object obj)
        {

        }

    }
}
