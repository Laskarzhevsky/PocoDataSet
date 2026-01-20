namespace PocoDataSet.ObservableExtensionsTests
{
    internal interface IDepartment
    {
        // intentionally weird casing to validate case-insensitive matching
        int id
        {
            get; set;
        }

        string? NAME
        {
            get; set;
        }
    }
}
