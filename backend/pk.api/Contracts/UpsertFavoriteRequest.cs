namespace pk.api.Contracts;

/// <summary>
/// 新增或更新收藏请求。
/// </summary>
public class UpsertFavoriteRequest
{
    /// <summary>
    /// 客户端标识。
    /// </summary>
    public required string ClientId { get; init; }
    /// <summary>
    /// 项目 ID。
    /// </summary>
    public required long ProjectId { get; init; }
    /// <summary>
    /// 备注信息。
    /// </summary>
    public string? Note { get; init; }
}
