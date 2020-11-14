using System;
using System.Collections.Generic;
using System.Text;

namespace Crofana.IoC
{
    public interface IMultiInstantiationPolicyCrofanaObjectFactory : ICrofanaObjectFactory
    {
        IInstantiationPolicy GetInstantiationPolicy(Type type);
    }
    public interface IInstantiationPolicy
    {
        object GetObject(Type type);
    }
}
