using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using UnifiedAccount.Application.Mappings;

namespace UnifiedAccount.Application.Tests.Mappings;

public class MappingProfileTests
{
    /// <summary>このテストでは マッピング対象を指定したとき、マッピング結果が適用される。</summary>
    [Fact]
    public void MappingProfile_ShouldBeValid()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingProfile>(),
            NullLoggerFactory.Instance);

        configuration.Invoking(config => config.AssertConfigurationIsValid())
            .Should().NotThrow();
    }
}
