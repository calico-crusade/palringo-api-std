using StructureMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PalApi.Utilities
{
    using Networking;
    using SubProfile.Parsing;

    public interface IReflectionUtility
    {
        IEnumerable<Type> GetTypes(Type implementedInterface);

        object ChangeType(object obj, Type toType);

        T ChangeType<T>(object obj);

        IEnumerable<T> GetAllTypesOf<T>();

        T GetInstance<T>();

        object GetInstance(Type type);
    }

    public class ReflectionUtility : IReflectionUtility
    {
        private Container container;

        public ReflectionUtility(Container container)
        {
            this.container = container;
        }

        public object ChangeType(object obj, Type toType)
        {
            if (obj == null)
                return null;

            var fromType = obj.GetType();

            var to = Nullable.GetUnderlyingType(toType) ?? toType;
            var from = Nullable.GetUnderlyingType(fromType) ?? fromType;

            if (to == from)
                return obj;

            if (to.IsEnum)
            {
                return Enum.ToObject(to, Convert.ChangeType(obj, to.GetEnumUnderlyingType()));
            }

            if (from == typeof(byte[]) && to == typeof(DataMap))
            {
                return new DataMap((byte[])obj);
            }

            if (from == typeof(byte[]))
            {
                obj = PacketSerializer.Outbound.GetString((byte[])obj);

                if (to == typeof(string))
                    return obj;
            }

            if (to == typeof(byte[]) && from == typeof(string))
            {
                return PacketSerializer.Outbound.GetBytes((string)obj);
            }
            
            return Convert.ChangeType(obj, to);
        }

        public T ChangeType<T>(object obj)
        {
            return (T)ChangeType(obj, typeof(T));
        }

        public T GetInstance<T>()
        {
            return container.GetInstance<T>();
        }

        //public async Task<object> ExecuteMethod(MethodInfo info, object instance, params object[] knownParams)
        //{
        //    try
        //    {

        //    }
        //    catch (Exception ex)
        //    {
        //        return ex;
        //    }
        //}

        public object GetInstance(Type type)
        {
            return container.GetInstance(type);
        }

        public IEnumerable<T> GetAllTypesOf<T>()
        {
            return container.GetAllInstances<T>().ToArray();
        }
        
        public IEnumerable<Type> GetTypes(Type implementedInterface)
        {
            var assembly = Assembly.GetEntryAssembly();
            var alreadyLoaded = new List<string>
            {
                assembly.FullName
            };

            foreach (var type in assembly.DefinedTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                    continue;

                if (type.ImplementedInterfaces.Contains(implementedInterface))
                    yield return type;
            }

            var assems = assembly.GetReferencedAssemblies()
                .Select(t => t.FullName)
                .Except(alreadyLoaded)
                .ToArray();
            foreach (var ass in assems)
            {
                foreach (var type in GetTypes(implementedInterface, ass, alreadyLoaded))
                {
                    yield return type;
                }
            }
        }

        private IEnumerable<Type> GetTypes(Type implementedInterface, string assembly, List<string> alreadyLoaded)
        {
            if (alreadyLoaded.Contains(assembly))
                yield break;

            alreadyLoaded.Add(assembly);
            var asml = Assembly.Load(assembly);
            foreach (var type in asml.DefinedTypes)
            {
                if (type.IsInterface || type.IsAbstract)
                    continue;

                if (type.ImplementedInterfaces.Contains(implementedInterface))
                    yield return type;
            }

            var assems = asml.GetReferencedAssemblies()
                .Select(t => t.FullName)
                .Except(alreadyLoaded)
                .ToArray();
            foreach (var ass in assems)
            {
                foreach (var type in GetTypes(implementedInterface, ass, alreadyLoaded))
                {
                    yield return type;
                }
            }

        }
    }
}
