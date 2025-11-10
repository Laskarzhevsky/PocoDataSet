using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public static class CompositeValueConverterFactory
    {
        static readonly IValueConverter _default = CreateDefault();

        public static IValueConverter Default
        {
            get
            {
                return _default;
            }
        }

        static IValueConverter CreateDefault()
        {
            IValueConverter[] converters = new IValueConverter[]
            {
                new StringValueConverter(),
                new GuidValueConverter(),
                new EnumValueConverter(),
                new BooleanValueConverter(),
                new DateTimeValueConverter(),
                new ConvertibleValueConverter(),
                new NoOpExactTypeConverter()
            };

            return new CompositeFieldValueConverter(converters);
        }
    }
}
