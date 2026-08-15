using FluentAssertions;
using UnifiedAccount.Domain.Common;

namespace UnifiedAccount.Domain.Tests.Common;

public class LocalDateTimeProviderTests
{
    [Fact]
    public void Now_ShouldReturnLocalDateTime()
    {
        LocalDateTimeProvider.Now.Kind.Should().Be(DateTimeKind.Local);
    }
}
