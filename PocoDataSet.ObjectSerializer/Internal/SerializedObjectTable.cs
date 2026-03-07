using System.Text.Json;

namespace PocoDataSet.ObjectSerializer
{
    internal class SerializedObjectTable
    {
        public string? Name
        {
            get;
            set;
        }

        public string? ItemType
        {
            get;
            set;
        }

        public JsonElement Items
        {
            get;
            set;
        }
    }
}
