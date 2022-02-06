﻿using System.IO;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Yandex.Zen.Core.Models
{
    public class TableModel
    {
        public TableModel(IZennoPosterProjectModel zenno, string nameTable, string path)
        {
            Obj = zenno.Tables[nameTable];
            TableName = nameTable;
            File = new FileInfo(zenno.ExecuteMacro(path));
        }

        public TableModel(IZennoPosterProjectModel zenno, string nameTable, ILocalVariable path)
            : this(zenno, nameTable, path.Value) { }

        /// <summary>
        /// Экземпляр таблицы.
        /// </summary>
        public IZennoTable Obj { get; private set; }

        /// <summary>
        /// Файл таблицы.
        /// </summary>
        public FileInfo File { get; private set; }

        /// <summary>
        /// Название таблицы.
        /// </summary>
        public string TableName { get; private set; }

        /// <summary>
        /// Название файла таблицы (должен быть указан File/path).
        /// </summary>
        public string FileName { get => File.Name; }

        /// <summary>
        /// Название файла.
        /// </summary>
        public string Path { get => File.FullName; }
    }
}
