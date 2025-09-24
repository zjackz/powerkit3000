using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace pk.core.Amazon.Contracts;

/// <summary>
/// Amazon 榜单抓取源接口，便于注入不同实现。
/// </summary>
public interface IAmazonBestsellerSource
{
    Task<IReadOnlyList<AmazonBestsellerEntry>> FetchAsync(string amazonCategoryId, Amazon.AmazonBestsellerType bestsellerType, CancellationToken cancellationToken);
}
