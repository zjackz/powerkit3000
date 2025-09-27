namespace pk.api.Contracts;

/// <summary>
/// 收藏项目 DTO。
/// </summary>
public class ProjectFavoriteDto
{
    /// <summary>
    /// 收藏记录 ID。
    /// </summary>
    public required int Id { get; init; }
    /// <summary>
    /// 客户端标识。
    /// </summary>
    public required string ClientId { get; init; }
    /// <summary>
    /// 收藏的项目详情。
    /// </summary>
    public required ProjectListItemDto Project { get; init; }
    /// <summary>
    /// 备注信息。
    /// </summary>
    public string? Note { get; init; }
    /// <summary>
    /// 收藏时间。
    /// </summary>
    public DateTime SavedAt { get; init; }
}
