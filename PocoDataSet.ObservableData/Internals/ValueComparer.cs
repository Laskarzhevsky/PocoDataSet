namespace PocoDataSet.ObservableData
{
    internal static class ValueComparer
    {
        public static bool AreValuesEqual(object? oldValue, object? newValue)
        {
            if (ReferenceEquals(oldValue, newValue))
            {
                return true;
            }

            if (oldValue == null || newValue == null)
            {
                return false;
            }

            byte[]? oldBytes = oldValue as byte[];
            if (oldBytes != null)
            {
                byte[]? newBytes = newValue as byte[];
                if (newBytes == null)
                {
                    return false;
                }

                if (oldBytes.Length != newBytes.Length)
                {
                    return false;
                }

                for (int i = 0; i < oldBytes.Length; i++)
                {
                    if (oldBytes[i] != newBytes[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return object.Equals(oldValue, newValue);
        }
    }
}
