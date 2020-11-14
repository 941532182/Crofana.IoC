using System;
using System.Reflection;

namespace Crofana.IoC
{
    public abstract class StandardCrofanaObjectFactoryBase : IStandardCrofanaObjectFactory
    {
        public virtual ICrofanaObjectFactory Parent { get; set; }

        public virtual string GetConfigPath(Type type) => type.GetCustomAttribute<ConfigPathAttribute>()?.Path;

        public abstract IInstantiationPolicy GetInstantiationPolicy(Type type);

        public abstract object GetObject(Type type);

        public abstract void ProcessDependencyInjection(object crofanaObject);
    }
}
