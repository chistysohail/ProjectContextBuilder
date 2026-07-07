using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using ProjectContextBuilder;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var options = config
    .GetSection("ProjectContext")
    .Get<ProjectContextOptions>()!;

if (!Directory.Exists(options.RootPath))
{
    Console.WriteLine($"Root path not found: {options.RootPath}");
    return;
}

var sb = new StringBuilder();

sb.AppendLine("# PROJECT CONTEXT FILE");
sb.AppendLine($"Generated: {DateTime.Now}");
sb.AppendLine($"Root: {options.RootPath}");
sb.AppendLine();

sb.AppendLine("# DIRECTORY TREE");
sb.AppendLine(BuildTree(options.RootPath, options));
sb.AppendLine();

sb.AppendLine("# FILE CONTENTS");
sb.AppendLine();

foreach (var file in GetFiles(options.RootPath, options))
{
    var relativePath = Path.GetRelativePath(options.RootPath, file);

    sb.AppendLine("============================================================");
    sb.AppendLine($"FILE: {relativePath}");
    sb.AppendLine("============================================================");

    try
    {
        var text = File.ReadAllText(file);
        text = MaskSensitiveInfo(text);

        sb.AppendLine(text);
    }
    catch (Exception ex)
    {
        sb.AppendLine($"[Could not read file: {ex.Message}]");
    }

    sb.AppendLine();
}

Directory.CreateDirectory(Path.GetDirectoryName(options.OutputFile)!);
File.WriteAllText(options.OutputFile, sb.ToString(), Encoding.UTF8);

Console.WriteLine($"Done. Output saved to: {options.OutputFile}");

static IEnumerable<string> GetFiles(string root, ProjectContextOptions options)
{
    return Directory
        .EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
        .Where(file => !ShouldIgnorePath(file, root, options))
        .Where(file => ShouldIncludeFile(file, options))
        .Where(file => new FileInfo(file).Length <= options.MaxFileSizeKb * 1024);
}

static bool ShouldIncludeFile(string file, ProjectContextOptions options)
{
    var fileName = Path.GetFileName(file);
    var extension = Path.GetExtension(file).ToLowerInvariant();

    if (options.IncludeFileNames.Any(x =>
            string.Equals(x, fileName, StringComparison.OrdinalIgnoreCase)))
        return true;

    if (fileName.Equals("Dockerfile", StringComparison.OrdinalIgnoreCase))
        return true;

    return options.IncludeExtensions
        .Select(x => x.ToLowerInvariant())
        .Contains(extension);
}

static bool ShouldIgnorePath(string path, string root, ProjectContextOptions options)
{
    var relative = Path.GetRelativePath(root, path);
    var parts = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    if (parts.Any(part =>
            options.IgnoreFolders.Any(ignore =>
                string.Equals(part, ignore, StringComparison.OrdinalIgnoreCase))))
        return true;

    var fileName = Path.GetFileName(path);

    return options.IgnoreFiles.Any(pattern => WildcardMatch(fileName, pattern));
}

static bool WildcardMatch(string input, string pattern)
{
    var regex = "^" + Regex.Escape(pattern)
        .Replace("\\*", ".*")
        .Replace("\\?", ".") + "$";

    return Regex.IsMatch(input, regex, RegexOptions.IgnoreCase);
}

static string BuildTree(string root, ProjectContextOptions options)
{
    var sb = new StringBuilder();
    BuildTreeRecursive(root, root, sb, "", options);
    return sb.ToString();
}

static void BuildTreeRecursive(
    string root,
    string current,
    StringBuilder sb,
    string indent,
    ProjectContextOptions options)
{
    var name = current == root ? Path.GetFileName(root.TrimEnd(Path.DirectorySeparatorChar)) : Path.GetFileName(current);
    sb.AppendLine($"{indent}{name}/");

    foreach (var dir in Directory.GetDirectories(current).OrderBy(x => x))
    {
        if (!ShouldIgnorePath(dir, root, options))
            BuildTreeRecursive(root, dir, sb, indent + "  ", options);
    }

    foreach (var file in Directory.GetFiles(current).OrderBy(x => x))
    {
        if (!ShouldIgnorePath(file, root, options) && ShouldIncludeFile(file, options))
            sb.AppendLine($"{indent}  {Path.GetFileName(file)}");
    }
}

static string MaskSensitiveInfo(string text)
{
    var patterns = new[]
    {
        @"(?i)(password\s*[:=]\s*)[""']?[^""'\r\n,}]+",
        @"(?i)(pwd\s*[:=]\s*)[""']?[^""'\r\n,}]+",
        @"(?i)(secret\s*[:=]\s*)[""']?[^""'\r\n,}]+",
        @"(?i)(clientsecret\s*[:=]\s*)[""']?[^""'\r\n,}]+",
        @"(?i)(apikey\s*[:=]\s*)[""']?[^""'\r\n,}]+",
        @"(?i)(api_key\s*[:=]\s*)[""']?[^""'\r\n,}]+",
        @"(?i)(token\s*[:=]\s*)[""']?[^""'\r\n,}]+",
        @"(?i)(connectionstring\s*[:=]\s*)[""']?[^""'\r\n,}]+"
    };

    foreach (var pattern in patterns)
    {
        text = Regex.Replace(text, pattern, "$1***MASKED***");
    }

    text = Regex.Replace(
        text,
        @"(?i)(User ID|Uid|Username|Password|Pwd)=([^;]+)",
        "$1=***MASKED***");

    return text;
}