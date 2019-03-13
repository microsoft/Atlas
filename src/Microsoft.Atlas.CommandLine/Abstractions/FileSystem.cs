using System.IO;
using System.Linq;

namespace Microsoft.Atlas.CommandLine.Abstractions
{
    public interface IFileSystem
    {
        string PathCombine(params string[] paths);

        bool DirectoryExists(string path);

        bool FileExists(string path);

        TextReader OpenText(string path);
    }

    public class FileSystem : IFileSystem
    {
        public string PathCombine(params string[] paths)
        {
            var combined = paths.Aggregate(default(string), (acc, path) =>
            {
                if (acc == null)
                {
                    return path;
                }

                var slashIndex = path.IndexOf('/');
                var backslashIndex = path.IndexOf('\\');
                if (slashIndex == 0 || backslashIndex == 0)
                {
                    return path;
                }

                var schemeDelimiterIndex = path.IndexOf("://");
                if (schemeDelimiterIndex > 0 && schemeDelimiterIndex < slashIndex)
                {
                    return path;
                }

                return acc.TrimEnd('/', '\\') + '/' + path.TrimStart('/', '\\');
            });
            return combined;
        }

        public bool DirectoryExists(string path) => Directory.Exists(path);

        public bool FileExists(string path) => File.Exists(path);

        public TextReader OpenText(string path) => File.OpenText(path);
    }
}
