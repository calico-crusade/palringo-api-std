using StructureMap;
using StructureMap.Graph;
using System;
using System.Collections.Generic;

namespace PalApi.DependencyInjection
{
    public class MapHandler<T>
    {
        private List<Action<ConfigurationExpression>> Mappers;
        private List<Action<ConfigurationExpression, Container>> Containers;
        private List<Action<IAssemblyScanner>> Scanners;

        public MapHandler()
        {
            Mappers = new List<Action<ConfigurationExpression>>();
            Scanners = new List<Action<IAssemblyScanner>>();
            Containers = new List<Action<ConfigurationExpression, Container>>();
        }

        public MapHandler<T> Use<T1, T2>() where T2: T1
        {
            return Config(c =>
            {
                c.For<T1>().Use<T2>();
            });
        }

        public MapHandler<T> Use<T1, T2>(T2 item) where T2: T1
        {
            return Config(c =>
            {
                c.For<T1>().Use(a => item);
            });
        }

        public MapHandler<T> Use<T1, T2>(Func<IContext, T2> item) where T2: T1
        {
            return Config(c =>
            {
                c.For<T1>().Use(a => item(a));
            });
        }

        public MapHandler<T> Use<T1, T2>(Func<Container, IContext, T2> item) where T2 : T1
        {
            return Config((c, c1) =>
            {
                c.For<T1>().Use(a => item(c1, a));
            });
        }

        public MapHandler<T> AllOf<T1>()
        {
            return Scan(c =>
            {
                c.AddAllTypesOf<T1>();
            });
        }

        public MapHandler<T> AllOf(Type t)
        {
            return Scan(c =>
            {
                c.AddAllTypesOf(t);
            });
        }

        public MapHandler<T> Config(Action<ConfigurationExpression> exp)
        {
            Mappers.Add(exp);
            return this;
        }

        public MapHandler<T> Scan(Action<IAssemblyScanner> scan)
        {
            Scanners.Add(scan);
            return this;
        }

        public MapHandler<T> Config(Action<ConfigurationExpression, Container> exp)
        {
            Containers.Add(exp);
            return this;
        }

        public Container Create()
        {
            var cont = new Container();
            cont.Configure(c =>
            {
                c.Scan(s =>
                {
                    s.AssembliesAndExecutablesFromApplicationBaseDirectory();
                    s.TheCallingAssembly();
                    s.WithDefaultConventions();
                    s.SingleImplementationsOfInterface();

                    foreach (var scanner in Scanners)
                        scanner?.Invoke(s);
                });

                foreach (var conf in Mappers)
                    conf?.Invoke(c);

                foreach (var conf in Containers)
                    conf?.Invoke(c, cont);
            });
            return cont;
        }

        public T Build()
        {
            return Create().GetInstance<T>();
        }
    }
}
