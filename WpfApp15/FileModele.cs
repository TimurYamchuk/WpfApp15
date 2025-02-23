using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp15
{
    class FileModel
    {
        public string Name { get; set; }
        public string Folder { get; set; }
        public string Size { get; set; }
        public string Date { get; set; }

        public FileModel(string name, string folder, string size, string data)
        {
            Name = name;
            Folder = folder;
            Size = size;
            Date = data;
        }

    }
