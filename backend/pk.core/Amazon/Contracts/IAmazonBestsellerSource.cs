using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace pk.core.Amazon.Contracts;

/// <summary>
/// Amazon 榜单抓取源接口，便于注入不同实现。
/// </summary>
public interface IAmazonBestsellerSource
{
    /// <summary>
    /// 抓取指定 Amazon 类目与榜单类型的条目集合。
    /// </summary>
    /// <param name="amazonCategoryId">Amazon 官方类目编号。</param>
    /// <param name="bestsellerType">榜单类型。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>榜单条目列表。</returns>
    Task<IReadOnlyList<AmazonBestsellerEntry>> FetchAsync(string amazonCategoryId, Amazon.AmazonBestsellerType bestsellerType, CancellationToken cancellationToken);
}
