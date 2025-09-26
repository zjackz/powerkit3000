using System.Threading;
using System.Threading.Tasks;

namespace pk.core.Amazon.Operations;

/// <summary>
/// 提供运营指标采集数据的接口，便于替换不同抓取实现。
/// </summary>
public interface IAmazonOperationalDataSource
{
    Task<AmazonOperationalDataBatch> FetchAsync(CancellationToken cancellationToken);
}
