namespace PocoDataSet.ObservableExtensionsTests
{
    internal class Department : IDepartment
    {
        // intentionally weird casing to validate case-insensitive matching
        public int id
        {
            get; set;
        }

        public string? NAME
        {
            get; set;
        }
    }
}
