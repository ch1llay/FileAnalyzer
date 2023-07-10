using System.IO;

namespace FileAnalyzer;

public class FileRow
{
    public string FileName { get; set; }
    public long Count { get; set; }
    public FileInfo File;
}