using NFluent;
using System.Collections;

namespace BlackFox.Roslyn.Diagnostics.TestHelpers
{
    public static class NFluentExtensions
    {
        public static ICheckLink<ICheck<IEnumerable>> ContainExactlyAnyOrder<T>(this ICheck<IEnumerable> check,
            params T[] expected)
        {
            return check.Not.IsEmpty()
                .And.HasSize(expected.LongLength)
                .And.IsOnlyMadeOf(expected);
        }
    }
}