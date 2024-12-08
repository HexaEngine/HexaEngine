//#define PROFILE
#nullable disable
#if PROFILE

using MethodDecorator.Fody.Interfaces;
using System.Reflection;

#endif

namespace HexaEngine.Profiling
{
    // Any attribute which provides OnEntry/OnExit/OnException with proper args
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class ProfileAttribute : Attribute
#if PROFILE
        , IMethodDecorator
#endif
    {
#if PROFILE

        private string name;

        // instance, method and args can be captured here and stored in attribute instance fields
        // for future usage in OnEntry/OnExit/OnException
        public void Init(object instance, MethodBase method, object[] args)
        {
            name = $"{method.DeclaringType.Name}.{method.Name}";
        }

        public void OnEntry()
        {
            CPUProfiler.Global.Begin(name);
        }

        public void OnExit()
        {
            CPUProfiler.Global.End(name);
        }

        public void OnException(Exception exception)
        {
        }

#endif
    }
}