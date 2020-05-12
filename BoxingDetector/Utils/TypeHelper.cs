namespace BoxingDetector {
    using System;
    using System.Linq;
    using System.Reflection;
    //
    static class TypeHelper {
        public static Type[] GetTypes(Assembly assembly) {
            try {
                return assembly.GetTypes();
            }
            catch(ReflectionTypeLoadException e) {
                return e.Types;
            }
        }
        readonly static Type CompilerGenerated = typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute);
        public static bool IsAnonymousType(this Type type) {
            return type.GetCustomAttributesData().Any(d => d.AttributeType == CompilerGenerated) && type.Name.Contains("AnonymousType");
        }
        public static string GetTypeName(Type type) {
            var typeName = GetTypeNameCore(type);
            if(!type.IsGenericType)
                return typeName;
            var sb = new System.Text.StringBuilder(typeName);
            int argumentsPos = typeName.IndexOf('`');
            sb.Remove(argumentsPos, typeName.Length - argumentsPos);
            sb.Append("<");
            var genericArgs = type.GetGenericArguments();
            for(int i = 0; i < genericArgs.Length; i++) {
                sb.Append(GetTypeName(genericArgs[i]));
                if(i > 0) sb.Append(",");
            }
            sb.Append(">");
            return sb.ToString();
        }
        static string GetTypeNameCore(Type type) {
            if(!string.IsNullOrEmpty(type.Namespace)) {
                var sb = new System.Text.StringBuilder(type.Namespace);
                if(type.IsNested) {
                    Type parent = type.DeclaringType;
                    while(parent != null) {
                        sb.Append(".");
                        sb.Append(parent.Name);
                        if(parent.IsNested)
                            parent = parent.DeclaringType;
                        else parent = null;
                    }
                }
                sb.Append(".");
                sb.Append(type.Name);
                return sb.ToString();
            }
            return type.Name;
        }
    }
}