using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace TypeHelpers
{
    public static class TypeExtensions
    {
        public static readonly Dictionary<int,Type>  paramNumber2delegateType = new Dictionary<int,Type> ();
        public static readonly Dictionary<string, Type> name2delegateType = new Dictionary<string, Type>();

        
        static TypeExtensions()
        {
            paramNumber2delegateType[0] = typeof(Func<>);
            paramNumber2delegateType[1] = typeof(Func<,>);
            paramNumber2delegateType[2] = typeof(Func<,,>);
            paramNumber2delegateType[3] = typeof(Func<,,,>);
            paramNumber2delegateType[4] = typeof(Func<,,,,>);
            paramNumber2delegateType[5] = typeof(Func<,,,,,>);
            paramNumber2delegateType[6] = typeof(Func<,,,,,,>);
            paramNumber2delegateType[7] = typeof(Func<,,,,,,,>);
            paramNumber2delegateType[8] = typeof(Func<,,,,,,,,>);
            paramNumber2delegateType[9] = typeof(Func<,,,,,,,,,>);
            paramNumber2delegateType[10] =typeof(Func<,,,,,,,,,,>);
            paramNumber2delegateType[11] =typeof(Func<,,,,,,,,,,,>);
            paramNumber2delegateType[12] =typeof(Func<,,,,,,,,,,,,>);
            paramNumber2delegateType[13] =typeof(Func<,,,,,,,,,,,,,>);
            paramNumber2delegateType[14] =typeof(Func<,,,,,,,,,,,,,,>);
            paramNumber2delegateType[15] =typeof(Func<,,,,,,,,,,,,,,,>);
            paramNumber2delegateType[16] =typeof(Func<,,,,,,,,,,,,,,,,>);

            foreach (Type t in paramNumber2delegateType.Values)
            {
                name2delegateType[t.Name] = t;
            }
            
        }

        private class SimpleTypeComparer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y)
            {
                return x.Assembly == y.Assembly &&
                    x.Namespace == y.Namespace &&
                    x.Name == y.Name;
            }

            public int GetHashCode(Type obj)
            {
                throw new NotImplementedException();
            }
        }

        public static MethodInfo GetGenericMethod(this Type type, string name, Type[] parameterTypes)
        {
            var methods = type.GetMethods();
            foreach (var method in methods.Where(m => m.Name == name))
            {
                var methodParameterTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();

                if (methodParameterTypes.SequenceEqual(parameterTypes, new SimpleTypeComparer()))
                {
                    return method;
                }
            }

            return null;
        }

        public static MethodInfo[] GetExtensionMethods(this Type type, Type extendedType)
        {
            MethodInfo[] staticPublicMethods = type.GetMethods(BindingFlags.Static|BindingFlags.Public);
            bool generic = extendedType.IsGenericType;
            var query = from method in staticPublicMethods               
                        where method.IsDefined(typeof(ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType.Name == extendedType.Name
                        select method;

            int qc = query.Count();

        
           
            
            return query.ToArray();
        }

        public static int CountlambdaExpressionParameters(this MethodInfo mi)
        {
            int result = 0;
           
            foreach (ParameterInfo pi in mi.GetParameters())
            {       
                if(pi.ParameterType.IsLambdaExpressionType())
                {
                        result++;
                }             
            }
                return result;
        }
        

        public static bool IsLambdaExpressionType(this Type type)
        {
            Type exprType = typeof(Expression<>);
            if (exprType.Name == type.Name && type.GetGenericArguments()[0].IsLambdaType())            
                return true;
            return false;
        }
        public static bool IsLambdaType(this Type type)
        {
            if (name2delegateType.ContainsKey(type.Name))
                return true;
            return false;
        }
        public static int CountlambdaParameters(this MethodInfo mi)
        {
            int result = 0;
            
            foreach (ParameterInfo pi in mi.GetParameters())
            {
                if (pi.ParameterType.IsLambdaType())
                {                    
                        result++;                  
                }
            }
            return result;
        }

        public static bool IsQueryTypeResult(this Type expr)
        {
            Type enumerable = typeof(IEnumerable<>);
            return enumerable.Name == expr.Name || expr.GetInterface(enumerable.Name) != null;
        }

    }

    public static class LinqRuntimeTypeBuilder
    {
       
        private static AssemblyName assemblyName = new AssemblyName() { Name = "DynamicLinqTypes" };
        private static ModuleBuilder moduleBuilder = null;
        private static Dictionary<string, Type> builtTypes = new Dictionary<string, Type>();

        static LinqRuntimeTypeBuilder()
        {
            moduleBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run).DefineDynamicModule(assemblyName.Name);
        }

        private static string GetTypeKey(Dictionary<string, Type> fields)
        {
            //TODO: optimize the type caching -- if fields are simply reordered, that doesn't mean that they're actually different types, so this needs to be smarter
            string key = string.Empty;
            foreach (var field in fields)
                key += field.Key + ";" + field.Value.Name + ";";

            return key;
        }

        public static Type GetDynamicType(Dictionary<string, Type> fields)
        {
            if (null == fields)
                throw new ArgumentNullException("fields");
            if (0 == fields.Count)
                throw new ArgumentOutOfRangeException("fields", "fields must have at least 1 field definition");

            try
            {
                Monitor.Enter(builtTypes);
                string className = GetTypeKey(fields);

                if (builtTypes.ContainsKey(className))
                    return builtTypes[className];

                TypeBuilder typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Serializable);

                foreach (var field in fields)
                    typeBuilder.DefineField(field.Key, field.Value, FieldAttributes.Public);

                builtTypes[className] = typeBuilder.CreateType();

                return builtTypes[className];
            }
            catch (Exception )
            {
               throw;
               
            }
            finally
            {
                Monitor.Exit(builtTypes);
            }

            
        }


        private static string GetTypeKey(IEnumerable<PropertyInfo> fields)
        {
            return GetTypeKey(fields.ToDictionary(f => f.Name, f => f.PropertyType));
        }

        public static Type GetDynamicType(IEnumerable<PropertyInfo> fields)
        {
            return GetDynamicType(fields.ToDictionary(f => f.Name, f => f.PropertyType));
        }
    }
}
