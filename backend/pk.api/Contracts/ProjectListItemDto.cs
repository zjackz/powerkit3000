namespace pk.api.Contracts;

/// <summary>
/// 项目列表展示 DTO。
/// </summary>
/// <param name="Id">项目 ID。</param>
/// <param name="Name">项目名称。</param>
/// <param name="NameCn">项目中文名称。</param>
/// <param name="Blurb">项目简介。</param>
/// <param name="BlurbCn">项目中文简介。</param>
/// <param name="CategoryName">类别名称。</param>
/// <param name="Country">国家代码。</param>
/// <param name="State">项目状态。</param>
/// <param name="Goal">目标金额。</param>
/// <param name="Pledged">已有筹资。</param>
/// <param name="PercentFunded">达成率。</param>
/// <param name="FundingVelocity">筹资速度。</param>
/// <param name="BackersCount">支持者数量。</param>
/// <param name="Currency">货币代码。</param>
/// <param name="LaunchedAt">上线时间。</param>
/// <param name="Deadline">截止时间。</param>
/// <param name="CreatorName">创建者名称。</param>
/// <param name="LocationName">所在地区名称。</param>
public record ProjectListItemDto(
    long Id,
    string Name,
    string? NameCn,
    string? Blurb,
    string? BlurbCn,
    string CategoryName,
    string Country,
    string State,
    decimal Goal,
    decimal Pledged,
    decimal PercentFunded,
    decimal FundingVelocity,
    int BackersCount,
    string Currency,
    DateTime LaunchedAt,
    DateTime Deadline,
    string CreatorName,
    string? LocationName
);
