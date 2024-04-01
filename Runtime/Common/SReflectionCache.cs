using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sapo.DI.Runtime.Attributes;
using Sapo.DI.Runtime.Interfaces;
using UnityEngine;

namespace Sapo.DI.Runtime.Common
{
    internal class SReflectionCache : ISReflectionCache
    {
        public (Type componentType, Type registerType)[] RegistrableComponents { get; private set; }
        
        public Type[] InjectableComponents { get; private set; }

        private readonly Dictionary<Type, FieldInfo[]> _injectFieldsCache = new();

        public void Build()
        {
            var component = typeof(Component);

            var components = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.SafelyGetTypes())
                .Where(t => component.IsAssignableFrom(t));
            
            var registrableComponents = new List<(Type componentType, Type registerType)>();
            var injectableComponents = new List<Type>();
            
            foreach (var type in components)
            {
                if (type.IsDefinedWithAttribute<SRegister>(out var sRegister))
                    registrableComponents.Add((type, sRegister.Type));

                var injectFields = type.GetInjectFields().ToArray();
                if (injectFields.IsEmpty()) continue;

                _injectFieldsCache[type] = injectFields;
                injectableComponents.Add(type);
            }
            
            RegistrableComponents = registrableComponents.ToArray();
            InjectableComponents = injectableComponents.ToArray();
        }
        
        public FieldInfo[] GetInjectFields(Type type)
        {
            if (_injectFieldsCache.TryGetValue(type, out var fields)) return fields;

            fields = type.GetInjectFields().ToArray();
            _injectFieldsCache[type] = fields;
            
            return fields;
        }
    }
}