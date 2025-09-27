using System;
using System.Collections.Generic;

namespace pk.core.Amazon.Contracts;

/// <summary>
/// 描述外部系统导入 Amazon 榜单快照所需的数据模型。
/// </summary>
public class AmazonSnapshotImportModel
{
    /// <summary>
    /// 创建导入模型实例。
    /// </summary>
    /// <param name="categoryId">内部类目主键。</param>
    /// <param name="bestsellerType">榜单类型。</param>
    /// <param name="capturedAt">采集时间。</param>
    /// <param name="entries">榜单条目集合。</param>
    public AmazonSnapshotImportModel(
        int categoryId,
        Amazon.AmazonBestsellerType bestsellerType,
        DateTime capturedAt,
        IReadOnlyCollection<AmazonBestsellerEntry> entries)
    {
        if (entries == null)
        {
            throw new ArgumentNullException(nameof(entries));
        }

        if (entries.Count == 0)
        {
            throw new ArgumentException("Import entries must not be empty.", nameof(entries));
        }

        CategoryId = categoryId;
        BestsellerType = bestsellerType;
        CapturedAt = capturedAt;
        Entries = entries;
    }

    /// <summary>
    /// 内部 Amazon 类目主键。
    /// </summary>
    public int CategoryId { get; }

    /// <summary>
    /// 榜单类型。
    /// </summary>
    public Amazon.AmazonBestsellerType BestsellerType { get; }

    /// <summary>
    /// 快照采集时间。
    /// </summary>
    public DateTime CapturedAt { get; }

    /// <summary>
    /// 榜单条目集合。
    /// </summary>
    public IReadOnlyCollection<AmazonBestsellerEntry> Entries { get; }
}
