using System.Threading;
using System.Threading.Tasks;

namespace pk.core.Amazon.Operations;

/// <summary>
/// 提供运营指标采集数据的接口，便于替换不同抓取实现。
/// </summary>
public interface IAmazonOperationalDataSource
{
    /// <summary>
    /// 拉取最新的运营指标批次数据。
    /// </summary>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>封装运营指标的数据批次。</returns>
    Task<AmazonOperationalDataBatch> FetchAsync(CancellationToken cancellationToken);
}
