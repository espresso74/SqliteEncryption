using System;

namespace DemoProject.Contract
{
    public interface IFileService
    {
        string DocumentsFolderPath { get; }
        string DefaultNoImageSource { get; }
        Byte[] ReadDBByte(string filePath);
        string ReadTextFile();
        string GetBaseUrl();
        bool Delete(string filePath);
    }
}
