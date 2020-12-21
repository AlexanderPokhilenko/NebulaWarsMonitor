using System.Collections.Generic;
using System.Linq;

namespace NebulaWarsMonitor.Models
{
    public class PropertiesDataModel
    {
        public string TableName { get; set; }
        public string EntityName { get; set; }
        public int PrevPage { get; set; }
        public int CurrentPage { get; set; }
        public int NextPage { get; set; }
        public int LastPage { get; set; }
        public bool NeedBack => !(PrevPage == CurrentPage && CurrentPage == 0);
        public bool NeedNext => CurrentPage < LastPage;
        public bool IsReadOnly { get; set; }
        public IEnumerable<string> EntitiesNames { get; set; }
        public Dictionary<string, IEnumerable<object>> Data { get; set; }
        public IEnumerable<Dictionary<string, object>> PrimaryKeys { get; set; }
        public Dictionary<string, Dictionary<string, string>> ForeignKeys { get; set; }
        public IEnumerable<string> Properties => Data?.Keys.AsEnumerable();

        private IEnumerable<IEnumerable<object>> Cells
        {
            get
            {
                var enumerators = Data.Values.Select(x => x.GetEnumerator()).ToArray();
                try
                {
                    while (enumerators.All(x => x.MoveNext()))
                    {
                        yield return enumerators.Select(x => x.Current).ToArray();
                    }
                }
                finally
                {
                    foreach (var enumerator in enumerators)
                        enumerator.Dispose();
                }
            }
        }

        public IEnumerable<(Dictionary<string, object> Keys, IEnumerable<object> Values)> Rows =>
            Cells.Zip(PrimaryKeys, (values, keys) => (keys, values));
    }
}
