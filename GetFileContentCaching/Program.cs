using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GetFileContentCaching
{
    class Program
    {
        static void Main(string[] args)
        {
            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape))
            {
                Console.WriteLine(GetFileContents());
                Thread.Sleep(1000);
            }
        }

        private static string GetFileContents()
        {
            const string filePath = @"B:\temp\test.txt";
            const string fileKey = "filecontents";
            ObjectCache cache = MemoryCache.Default;
            string fileContents = cache[fileKey] as string;
            if (fileContents == null)
            {
                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(5);
                List<string> filePaths = new List<string>();
                filePaths.Add(filePath);

                policy.ChangeMonitors.Add(new
                    HostFileChangeMonitor(filePaths));

                Console.WriteLine("Fetching file contents");
                fileContents =
                    File.ReadAllText(filePath);

                cache.Set(fileKey, fileContents, policy);
            }

            return fileContents;
        }
    }
}
