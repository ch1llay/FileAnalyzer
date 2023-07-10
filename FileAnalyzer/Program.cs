using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileAnalyzer;

long GetRowsCount(string extension, string path)
{
    var filesByExtensions = GetFileExtensions(path);
    List<FileInfo> files;
    filesByExtensions.TryGetValue(extension, out files);
    files = files.Where(f => !f.DirectoryName.Contains("obj")).ToList();
    if (files != null)
    {
        var filesModels = files.Select(f=>new FileRow{FileName = f.FullName, File = f}).ToList();
        Parallel.ForEach(filesModels, f =>
        //foreach (var f in filesModels)
        {
            using (var fs = f.File.OpenText())
            {
                while (fs.ReadLine() != null)
                {
                    f.Count++;
                } 
            }
        }

        );

        return filesModels.Sum(f => f.Count);
    }

    return 0;

}

Dictionary<string, List<FileInfo>> GetFileExtensions(string path)
{
    var allFiles = new List<FileInfo>();
    foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
    {
        try
        {
            allFiles.Add(new FileInfo(file));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{file} {ex}");
        }
    }

    var fileByExtensions = allFiles
        .GroupBy(f => f.Extension)
        .ToDictionary(fg => fg.Key, fg => fg.Select(v => v).ToList());
    
    return fileByExtensions;
}

void GetInfo(Dictionary<string, List<FileInfo>> filesByExtensions)
{
    var filesByExtensionsSize = filesByExtensions
        .ToDictionary(fg => fg.Key, fg => fg.Value.Sum(f => f.Length))
        .OrderByDescending(f => f.Value)
        .ToList();
    var i = 0;
    var totalSize = 0L;
    foreach (var info in filesByExtensionsSize)
    {
        totalSize += info.Value;
        Console.WriteLine($"{++i} {info.Key}, {info.Value / 1024m} KB");
    }
    
    Console.WriteLine($"total size = {totalSize / 1024m} KB");
}


ILogger logger = new ConsoleLogger();

Console.WriteLine("enter dir: ");
var dir = Console.ReadLine()?.Replace('"'.ToString(), "");
var filesExtensions = GetFileExtensions(dir);
Do(()=>GetInfo(filesExtensions));
Console.WriteLine("enter file extension for getting lines count: ");
var extension = Console.ReadLine();

Do(() =>
{
    var rowCount = GetRowsCount(extension, dir);
    Console.WriteLine($"code rows count {rowCount}");
});

Console.ReadKey();

void Do(Action action)
{
    var stopwatch = new Stopwatch();
    stopwatch.Start();
    action.Invoke();
    stopwatch.Stop();
    Console.WriteLine($"lasted {stopwatch.ElapsedMilliseconds} milliseconds");
}


