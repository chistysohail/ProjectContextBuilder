using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectContextBuilder
{
    public sealed class ProjectContextOptions
    {
        public string RootPath { get; set; } = "";
        public string OutputFile { get; set; } = "project-context.txt";

        public List<string> IncludeExtensions { get; set; } = [];
        public List<string> IncludeFileNames { get; set; } = [];

        public List<string> IgnoreFolders { get; set; } = [];
        public List<string> IgnoreFiles { get; set; } = [];

        public int MaxFileSizeKb { get; set; } = 500;
    }
}
