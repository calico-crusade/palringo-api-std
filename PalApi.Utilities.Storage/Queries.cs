using System.Collections.Generic;
using System.IO;

namespace PalApi.Utilities.Storage
{
    public class Queries
    {
        private Dictionary<string, string> queryFiles = null;
        public string Get { get; set; }
        public string GetAll { get; set; }
        public string Create { get; set; }
        public string Delete { get; set; }
        public string Update { get; set; }
        public string Insert { get; set; }
        public string InsertGetId { get; set; }

        public string this[string key] => (queryFiles ?? (queryFiles = QueriesFromFile())).ContainsKey(key) ? queryFiles[key] : null;

        private Dictionary<string, string> QueriesFromFile()
        {
            var rec = new Dictionary<string, string>();
            var dir = Directory.GetCurrentDirectory();
            var files = Directory.GetFiles(dir, "*.sql", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var name = file;
                if (name.StartsWith(dir))
                    name = name.Remove(0, dir.Length).Trim('\\').Trim('/');

                name = name.Replace('\\', ':').Replace('/', ':').Remove(name.Length - 4);

                rec.Add(name, File.ReadAllText(file));
            }

            return rec;
        }
    }
}