﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnoCakesMobile.Services
{
    public static class DependencyService
    {
        static readonly object s_dependencyLock = new object();

        static readonly List<Type> DependencyTypes = new List<Type>();
        static readonly Dictionary<Type, DependencyData> DependencyImplementations = new Dictionary<Type, DependencyData>();

        public static T Resolve<T>(DependencyFetchTarget fallbackFetchTarget = DependencyFetchTarget.GlobalInstance) where T : class
        {
            var result = DependencyResolver.Resolve(typeof(T)) as T;

            return result ?? Get<T>(fallbackFetchTarget);
        }

        public static T Get<T>(DependencyFetchTarget fetchTarget = DependencyFetchTarget.GlobalInstance) where T : class
        {
            DependencyData dependencyImplementation;
            lock (s_dependencyLock)
            {
                Type targetType = typeof(T);
                if (!DependencyImplementations.TryGetValue(targetType, out dependencyImplementation))
                {
                    Type implementor = FindImplementor(targetType);
                    DependencyImplementations[targetType] = (dependencyImplementation = implementor != null ? new DependencyData { ImplementorType = implementor } : null);
                }
            }

            if (dependencyImplementation == null)
                return null;

            if (fetchTarget == DependencyFetchTarget.GlobalInstance)
            {
                if (dependencyImplementation.GlobalInstance == null)
                {
                    lock (dependencyImplementation)
                    {
                        if (dependencyImplementation.GlobalInstance == null)
                        {
                            dependencyImplementation.GlobalInstance = Activator.CreateInstance(dependencyImplementation.ImplementorType);
                        }
                    }
                }
                return (T)dependencyImplementation.GlobalInstance;
            }
            return (T)Activator.CreateInstance(dependencyImplementation.ImplementorType);
        }

        public static void Register<T>() where T : class
        {
            Type type = typeof(T);
            if (!DependencyTypes.Contains(type))
                DependencyTypes.Add(type);
        }

        public static void Register<T, TImpl>() where T : class where TImpl : class, T
        {
            Type targetType = typeof(T);
            Type implementorType = typeof(TImpl);
            if (!DependencyTypes.Contains(targetType))
                DependencyTypes.Add(targetType);

            lock (s_dependencyLock)
                DependencyImplementations[targetType] = new DependencyData { ImplementorType = implementorType };
        }

        public static void RegisterSingleton<T>(T instance) where T : class
        {
            Type targetType = typeof(T);
            Type implementorType = typeof(T);
            if (!DependencyTypes.Contains(targetType))
                DependencyTypes.Add(targetType);

            lock (s_dependencyLock)
                DependencyImplementations[targetType] = new DependencyData { ImplementorType = implementorType, GlobalInstance = instance };
        }

        static Type FindImplementor(Type target) =>
            DependencyTypes.FirstOrDefault(t => target.IsAssignableFrom(t));

        

        class DependencyData
        {
            public object GlobalInstance { get; set; }

            public Type ImplementorType { get; set; }
        }

        public enum DependencyFetchTarget
        {
            GlobalInstance,
            NewInstance
        }
    }

    public static class DependencyResolver
    {
        static Func<Type, object[], object> Resolver { get; set; }

        internal static object Resolve(Type type, params object[] args)
        {
            var result = Resolver?.Invoke(type, args);

            if (result != null)
            {
                if (!type.IsInstanceOfType(result))
                {
                    throw new InvalidCastException("Resolved instance is not of the correct type.");
                }
            }

            return result;
        }

        internal static object ResolveOrCreate(Type type)
        {
            var result = Resolve(type);

            if (result != null)
                return result;

            return Activator.CreateInstance(type);
        }
    }
}