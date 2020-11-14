using System;

namespace Crofana.IoC
{
    /// <summary>
    /// 此类将采用指定的实例化策略
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class InstantiationPolicyAttribute : Attribute
    {
        public EInstantiationPolicy Policy { get; }
        public InstantiationPolicyAttribute(EInstantiationPolicy policy) => Policy = policy;
    }
    public enum EInstantiationPolicy
    {
        Singleton = 0,
        Prototype = 1,
        Pooled = 2
    }
}
