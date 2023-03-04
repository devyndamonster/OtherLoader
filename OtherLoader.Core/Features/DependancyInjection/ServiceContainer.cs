using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OtherLoader.Core.Features.DependancyInjection
{
    public class ServiceContainer
    {
        Dictionary<Type, object> _dependancies = new Dictionary<Type, object>();

        public void Bind<TInterface, TImplementation>()
        {
            var parameters = ResolveParametersForConstructor<TImplementation>();
            TImplementation instance = (TImplementation)Activator.CreateInstance(typeof(TImplementation), parameters);
            _dependancies.Add(typeof(TInterface), instance);
        }

        public void Bind<TImplementation>() where TImplementation : class
        {
            var parameters = ResolveParametersForConstructor<TImplementation>();
            TImplementation instance = (TImplementation)Activator.CreateInstance(typeof(TImplementation), parameters);
            _dependancies.Add(typeof(TImplementation), instance);
        }

        public void Bind<TInterface, TImplementation>(TImplementation instance)
        {
            _dependancies.Add(typeof(TInterface), instance);
        }

        public void Bind<TImplementation>(TImplementation instance)
        {
            _dependancies.Add(typeof(TImplementation), instance);
        }

        public T Resolve<T>()
        {
            return (T)_dependancies[typeof(T)];
        }

        public IEnumerable<T> CollectImplementationsOfType<T>()
        {
            return _dependancies.Values.OfType<T>();
        }

        private object[] ResolveParametersForConstructor<T>()
        {
            return ResolveParametersForMethod(typeof(T).GetConstructors()[0]);
        }

        private object[] ResolveParametersForMethod(MethodBase method)
        {
            return method
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .Select(parameterType => _dependancies[parameterType])
                .ToArray();
        }
    }
}
