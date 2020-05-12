using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ILReader.Analyzer;
using BF = System.Reflection.BindingFlags;

namespace BoxingDetector {
    class Program {
        static HashSet<string> KnownAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        static Program() {
            KnownAssemblies.Add(typeof(FastAccessors.Field).Assembly.GetName().Name);
            KnownAssemblies.Add(typeof(ILReader.Configuration).Assembly.GetName().Name);
            KnownAssemblies.Add(typeof(ILPattern).Assembly.GetName().Name);
        }
        static void Main() {
            var args = Environment.GetCommandLineArgs();
            string localPath = Path.GetDirectoryName(args[0]);
            if(args.Length > 1) {
                for(int i = 1; i < args.Length; i++)
                    ProcessAssembly(localPath, args[i]);
            }
            else {
                var localFiles = Directory.GetFiles(localPath, "*.dll");
                for(int i = 0; i < localFiles.Length; i++)
                    ProcessAssembly(localPath, localFiles[i], true);
            }
        }
        //
        static ILPattern[] boxingPatterns = new ILPattern[] {
            StringFormatBoxing.Instance,
            StringConcatBoxing.Instance,
            EnumHasFlagBoxing.Instance,
        };
        static void ProcessAssembly(string localPath, string asmPath, bool skipKnownAssemblies = false) {
            if(!File.Exists(asmPath))
                asmPath = Path.Combine(localPath, asmPath);
            if(File.Exists(asmPath)) {
                Assembly assembly = Assembly.LoadFrom(asmPath);
                if(assembly == null)
                    return;
                var asmName = assembly.GetName().Name;
                if(skipKnownAssemblies && KnownAssemblies.Contains(asmName))
                    return;
                var cfg = ILReader.Configuration.Standard;
                ProcessMethods(assembly, x => {
                    var reader = cfg.GetReader(x);
                    for(int i = 0; i < boxingPatterns.Length; i++) {
                        var pattern = boxingPatterns[i];
                        if(pattern.Match(reader))
                            return pattern;
                    }
                    return null;
                }, asmName.ToUpperInvariant());
            }
        }
        static void ProcessMethods(Assembly assembly, Func<MethodInfo, ILPattern> detectBoxing, string asmName) {
            bool asmMatch = false;
            var types = TypeHelper.GetTypes(assembly);
            for(int i = 0; i < types.Length; i++) {
                var type = types[i];
                if(type.IsInterface || type.IsAnonymousType())
                    continue;
                var methods = type.GetMethods(BF.DeclaredOnly | BF.Public | BF.NonPublic | BF.Static | BF.Instance);
                if(methods.Length == 0)
                    continue;
                bool typeMatch = false;
                for(int j = 0; j < methods.Length; j++) {
                    var method = methods[j];
                    var boxingPattern = detectBoxing(method);
                    if(boxingPattern == null)
                        continue;
                    if(!typeMatch) {
                        if(!asmMatch) {
                            Console.WriteLine(asmName);
                            Console.WriteLine(new string('-', asmName.Length + 4));
                            asmMatch = true;
                        }
                        Console.WriteLine();
                        Console.WriteLine(TypeHelper.GetTypeName(type));
                        typeMatch = true;
                    }
                    Console.WriteLine("-> " + method.Name + ", " + boxingPattern.ToString());
                }
            }
            if(asmMatch)
                Console.WriteLine();
        }
    }
}