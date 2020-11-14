using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Crofana.Extension.Reflection;

namespace Crofana.IoC
{
    public class StandardCrofanaObjectFactory : ICrofanaObjectFactory
    {

        private static Type[] EMPTY_TYPE_ARRAY = { };

        private Dictionary<Type, object> objectMap = new Dictionary<Type, object>();
        private List<Action> cachedPreConstructCallbacks = new List<Action>();
        private List<Action<object>> cachedPostConstructCallbacks = new List<Action<object>>();
        private List<Action<object>> cachedPreInjectCallbacks = new List<Action<object>>();
        private List<Action<object>> cachedPostInjectCallbacks = new List<Action<object>>();

        public object GetObject(Type type)
        {
            if (!type.HasAttributeRecursive<CrofanaObjectAttribute>())
            {
                return null;
            }
            return NewObject(type);
        }

        public T GetObject<T>() where T : class => GetObject(typeof(T)) as T;

        private object NewObject(Type type)
        {
            object obj;
            ProcessConstruction(type, out obj);
            if (obj != null)
            {
                TryRegisterObjectMap(obj);
                TryCacheCallbacks(obj);
                ProcessDependencyInjection(obj);
            }
            return obj;
        }

        private void ProcessConstruction(Type type, out object obj)
        {
            BroadcastPreConstruct();

            ConstructorInfo ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, EMPTY_TYPE_ARRAY, null);
            if (ctor == null)
            {
                throw new ConstructorNotFoundException(type);
            }
            obj = ctor.Invoke(null);

            BroadcastPostConstruct(obj);
        }

        private void BroadcastPreConstruct()
        {
            cachedPreConstructCallbacks.ForEach(x => x.Invoke());
        }

        private void BroadcastPostConstruct(object obj)
        {
            cachedPostConstructCallbacks.ForEach(x => x.Invoke(obj));
        }

        private void TryRegisterObjectMap(object obj)
        {
            Type type = obj.GetType();
            ScopeAttribute scope = type.GetCustomAttribute<ScopeAttribute>();
            if (scope == null || scope.Scope == Scope.Singleton)
            {
                objectMap[type] = obj;
            }
        }

        private void TryCacheCallbacks(object obj)
        {
            Type type = obj.GetType();
            bool isCOCListener = type.HasAttributeRecursive<CrofanaObjectConstructionListenerAttribute>();
            bool isDIListener = type.HasAttributeRecursive<DependencyInjectionListenerAttribute>();
            type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .ToList()
                .ForEach(x =>
                {
                    if (isCOCListener)
                    {
                        if (x.HasAttributeRecursive<CrofanaObjectConstructionListenerAttribute.PreConstructAttribute>())
                        {
                            cachedPreConstructCallbacks.Add(x.CreateDelegate(typeof(Action), obj) as Action);
                        }
                        if (x.HasAttributeRecursive<CrofanaObjectConstructionListenerAttribute.PostConstructAttribute>())
                        {
                            cachedPostConstructCallbacks.Add(x.CreateDelegate(typeof(Action<object>), obj) as Action<object>);
                        }
                    }
                    if (isDIListener)
                    {
                        if (x.HasAttributeRecursive<DependencyInjectionListenerAttribute.PreInjectAttribute>())
                        {
                            cachedPreInjectCallbacks.Add(x.CreateDelegate(typeof(Action<object>), obj) as Action<object>);
                        }
                        if (x.HasAttributeRecursive<DependencyInjectionListenerAttribute.PostInjectAttribute>())
                        {
                            cachedPostInjectCallbacks.Add(x.CreateDelegate(typeof(Action<object>), obj) as Action<object>);
                        }
                    }
                });
        }

        private void ProcessDependencyInjection(object obj)
        {
            BroadcastPreInject(obj);

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

            BroadcastPostInject(obj);
        }

        private void BroadcastPreInject(object obj)
        {
            cachedPreInjectCallbacks.ForEach(x => x.Invoke(obj));
        }

        private void BroadcastPostInject(object obj)
        {
            cachedPostInjectCallbacks.ForEach(x => x.Invoke(obj));
        }

    }
}
