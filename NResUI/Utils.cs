using System.Numerics;
using System.Reflection;
using ImGuiNET;

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
        // This is a direct port of imgui_demo.cpp HelpMarker function

        // https://github.com/ocornut/imgui/blob/master/imgui_demo.cpp#L190

        public static void ShowHint(string message)
        {
            // ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                // Change background transparency
                ImGui.PushStyleColor(
                    ImGuiCol.PopupBg,
                    new Vector4(
                        1,
                        1,
                        1,
                        0.8f
                    )
                );
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(message);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
                ImGui.PopStyleColor();
            }
        }
    }
}