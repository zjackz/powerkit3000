using System;
using System.Threading;
using System.Threading.Tasks;

namespace pk.core.Amazon.Operations;

/// <summary>
/// 默认的占位数据源，返回空数据，用于尚未接入真实采集时保持流程可用。
/// </summary>
public sealed class NoopAmazonOperationalDataSource : IAmazonOperationalDataSource
{
    /// <inheritdoc />
    public Task<AmazonOperationalDataBatch> FetchAsync(CancellationToken cancellationToken)
    {
        var batch = AmazonOperationalDataBatch.Empty(DateTime.UtcNow);
        return Task.FromResult(batch);
    }
}
