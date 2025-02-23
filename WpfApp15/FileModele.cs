using System;

namespace WpfApp15
{
    public class FileModel
    {
        public string Name { get; }
        public string Folder { get; }
        public long Size { get; }
        public DateTime LastModified { get; }

        public FileModel(string name, string folder, long size, DateTime lastModified)
        {
            Name = name;
            Folder = folder;
            Size = size;
            LastModified = lastModified;
        }
    }
}
