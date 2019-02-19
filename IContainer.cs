namespace Bos.Ioc {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    /*
    public interface IContainer {
        void Register<TTypeToResolve, TConcrete>();
        void Register<TTypeToResolve, TConcrete>(LifeCycle lifeCycle);
        void RegisterInstance<ITypeToResolve, TConcrete>(TConcrete instance, LifeCycle lifeCycle);

        TTypeToResolve Resolve<TTypeToResolve>();
        object Resolve(Type typeToResolve);
        void Fill(MonoBehaviour obj);

        void Build();
    }

    public enum LifeCycle {
        Singleton,
        Transient,
        MonoBehaviour
    }

    public class RegisteredObject {
        public Type TypeToResolve { get; private set; }

        public Type ConcreteType { get; private set; }

        public object Instance { get; private set; }

        public LifeCycle LifeCycle { get; private set; }

        public object CachedInstance { get; private set; }

        public RegisteredObject(Type typeToResolve, Type concreteType, LifeCycle lifeCycle, object instance = null) {
            TypeToResolve = typeToResolve;
            ConcreteType = concreteType;
            LifeCycle = lifeCycle;
            CachedInstance = instance;
        }

        public void CreateInstance(params object[] args ) {
            if (LifeCycle != LifeCycle.MonoBehaviour && !ConcreteType.IsSubclassOf(typeof(MonoBehaviour))) {
                Instance = Activator.CreateInstance(ConcreteType, args);
            } else {
                throw new Exception($"Not allow instantiate {ConcreteType.Name} with lifecycle {LifeCycle}");
            }
        }

        public void InvokeConstructMethod(params object[] args ) {
            ConcreteType.GetMethods().FirstOrDefault(m => m.Name == "Construct").Invoke(Instance, args);
        }

        public void SetTempAsPersistent() {
            Instance = CachedInstance;
            CachedInstance = null;
        }
    }

    public class IocContainer : IContainer {

        private readonly Dictionary<Type, RegisteredObject> registeredObjects = new Dictionary<Type, RegisteredObject>();

        public void Register<TTypeToResolve, TConcrete>() {
            Register<TTypeToResolve, TConcrete>(LifeCycle.Singleton);
        }

        public void Register<TTypeToResolve, TConcrete>(LifeCycle lifeCycle) {
            RegisteredObject registeredObject = new RegisteredObject(typeof(TTypeToResolve), typeof(TConcrete), lifeCycle);
            registeredObjects[registeredObject.TypeToResolve] = registeredObject;
        }

        public void RegisterInstance<ITypeToResolve, TConcrete>(TConcrete instance, LifeCycle lifeCycle) {
            RegisteredObject registeredObject = new RegisteredObject(typeof(ITypeToResolve), typeof(TConcrete), lifeCycle, instance);
            registeredObjects[registeredObject.TypeToResolve] = registeredObject;
            if(lifeCycle != LifeCycle.MonoBehaviour) {
                registeredObject.SetTempAsPersistent();
            }
        }

        public TTypeToResolve Resolve<TTypeToResolve>() {
            return (TTypeToResolve)ResolveObject(typeof(TTypeToResolve));
        }

        public void Build() {
            foreach(var pair in registeredObjects) {
                Resolve(pair.Key);
            }
        }


        private object ResolveObject(Type typeToResolve) {
            var registeredObject = registeredObjects.ContainsKey(typeToResolve) ? registeredObjects[typeToResolve] : null;
            if (registeredObject == null) {
                throw new Exception(string.Format(
                    "The type {0} has not been registered", typeToResolve.Name));
            }
            return GetInstance(registeredObject);
        }

        public object Resolve(Type typeToResolve) {
            return ResolveObject(typeToResolve);
        }

        public void Fill(MonoBehaviour obj) {
            RegisteredObject registeredObject = new RegisteredObject(obj.GetType(), obj.GetType(), LifeCycle.MonoBehaviour, obj);
            var dump = GetInstance(registeredObject);
        }

        private object GetInstance(RegisteredObject registeredObject ) {
            if(registeredObject.Instance == null || registeredObject.LifeCycle == LifeCycle.Transient ) {
                if (registeredObject.LifeCycle != LifeCycle.MonoBehaviour) {
                    var parameters = ResolveConstructorParameters(registeredObject);
                    registeredObject.CreateInstance(parameters.ToArray());
                } else {
                    registeredObject.SetTempAsPersistent();
                    var methodInfo = registeredObject.ConcreteType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(m => m.Name == "Construct");
                    if (methodInfo != null) {
                        var parameters = ResolveConstructMethodParameters(registeredObject);
                        registeredObject.InvokeConstructMethod(parameters.ToArray());
                    }
                    
                }
                Inject(registeredObject.Instance);
            }
            return registeredObject.Instance;
        }


        private IEnumerable<object> ResolveConstructorParameters(RegisteredObject registeredObject) {
            var constructorInfo = registeredObject.ConcreteType.GetConstructors().First();
            foreach (var parameter in constructorInfo.GetParameters()) {
                yield return ResolveObject(parameter.ParameterType);
            }
        }

        private IEnumerable<object> ResolveConstructMethodParameters(RegisteredObject registeredObject) {
            var methodInfo = registeredObject.ConcreteType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(m => m.Name == "Construct");
            
            foreach (var parameter in methodInfo.GetParameters()) {
                yield return ResolveObject(parameter.ParameterType);
            }
        }

        public void Inject(object obj) {
            Type objType = obj.GetType();
            FieldInfo[] publicFields = objType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] privateFields = objType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            InjectFields(obj, publicFields);
            InjectFields(obj, privateFields);

            PropertyInfo[] publicProperties = objType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] privateProperties = objType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
            InjectProperties(obj, publicProperties);
            InjectProperties(obj, privateProperties);
        }

        private void InjectFields(object obj, FieldInfo[] fields) {
            foreach (FieldInfo field in fields) {
                var attributes = field.GetCustomAttributes(typeof(InjectAttribute), true);
                if (attributes.Length > 0) {
                    field.SetValue(obj, ResolveObject(field.FieldType));
                }
            }
        }

        private void InjectProperties(object obj, PropertyInfo[] properties) {
            foreach (PropertyInfo property in properties) {
                var attributes = property.GetCustomAttributes(typeof(InjectAttribute), true);
                if (attributes.Length > 0) {
                    property.SetValue(obj, ResolveObject(property.PropertyType));
                }
            }
        }

    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectAttribute : System.Attribute {

        public string Description { get; set; }
    }*/
}