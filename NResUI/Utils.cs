using System.Reflection;

namespace NResUI
{
    public static class Utils
    {
        public static bool IsDirectory(this FileSystemInfo info)
        {
            // get the file attributes for file or directory
            FileAttributes attr = info.Attributes;

            //detect whether its a directory or file
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return true;
            else
                return false;
        }

        public static bool IsDirectoryPath(this string path)
        {
            return Directory.Exists(path);
        }

        public static void ClearContent(this DirectoryInfo directoryInfo)
        {
            foreach (var directory in directoryInfo.EnumerateDirectories())
            {
                directory.Delete(true);
            }

            foreach (var file in directoryInfo.EnumerateFiles())
            {
                file.Delete();
            }
        }

        public static IEnumerable<Type> GetAssignableTypesFromAssembly<T>(Assembly assembly)
        {
            return assembly.ExportedTypes
                .Where(t => t.IsAssignableTo(typeof(T)) && t is {IsAbstract: false, IsInterface: false});
        }

        public static IList<Type> GetAssignableTypes<T>()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblyNames = executingAssembly.GetReferencedAssemblies();
            var types = referencedAssemblyNames.SelectMany(
                    name =>
                        GetAssignableTypesFromAssembly<T>(Assembly.Load(name))
                )
                .Concat(
                    GetAssignableTypesFromAssembly<T>(executingAssembly)
                )
                .ToList();

            return types;
        }

        public static bool IsNullOrEmpty(this string? str)
        {
            return string.IsNullOrEmpty(str);
        }
    }
}