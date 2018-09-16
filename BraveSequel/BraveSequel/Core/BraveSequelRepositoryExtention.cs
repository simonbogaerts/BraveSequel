using System.Collections.Generic;
using System.IO;
using BraveSequel.Interfaces;

namespace BraveSequel.Core
{
    public class BraveSequelRepositoryExtention : IBraveSequelRepositoryExtention
    {
        public string Location { get; set; } = "Queries";

        private IDictionary<string, string> Dictionary { get; set; }
            = new Dictionary<string, string>();

        protected string GetQuery(string name)
        {
            if (Dictionary.ContainsKey(name))
            {
                return Dictionary[name];
            }

            Dictionary[name] = File
                .ReadAllText(Path.Combine(
                    Path.GetDirectoryName(GetType().Assembly.Location) + Path.DirectorySeparatorChar + Location,
                    name + ".sql"));

            return Dictionary[name];
        }
    }
}