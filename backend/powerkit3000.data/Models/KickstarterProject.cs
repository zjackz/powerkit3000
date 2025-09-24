using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace powerkit3000.data.Models
{
    public class KickstarterProject
    {
        // 项目唯一标识符
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        // 项目名称
        public string? Name { get; set; }
        // 项目中文名称
        public string? NameCn { get; set; }
        // 项目简介
        public string? Blurb { get; set; }
        // 项目中文简介
        public string? BlurbCn { get; set; }
        // 目标金额
        public decimal Goal { get; set; }
        // 已认捐金额
        public decimal Pledged { get; set; }
        // 项目状态 (例如: successful, failed, live)
        public string? State { get; set; }
        // 项目所在国家代码
        public string? Country { get; set; }
        // 货币类型
        public string? Currency { get; set; }
        // 截止日期
        public DateTime Deadline { get; set; }
        // 创建日期
        public DateTime CreatedAt { get; set; }
        // 发布日期
        public DateTime LaunchedAt { get; set; }
        // 支持者数量
        public int BackersCount { get; set; }
        // 以美元计价的已认捐金额
        public decimal UsdPledged { get; set; }
        // 状态变更日期
        public DateTime StateChangedAt { get; set; }
        // 项目的 slug (URL友好名称)
        public string? Slug { get; set; }
        // 国家的可显示名称
        public string? CountryDisplayableName { get; set; }
        // 货币符号
        public string? CurrencySymbol { get; set; }
        // 货币符号是否在金额后面
        public bool? CurrencyTrailingCode { get; set; }
        // 是否处于活动结束后认捐阶段
        public bool? IsInPostCampaignPledgingPhase { get; set; }
        // 是否为员工推荐
        public bool? StaffPick { get; set; }
        // 是否可加星标
        public bool? IsStarrable { get; set; }
        // 是否禁用通信
        public bool? DisableCommunication { get; set; }
        // 静态美元汇率
        public decimal StaticUsdRate { get; set; }
        // 转换后的认捐金额
        public decimal ConvertedPledgedAmount { get; set; }
        // 外汇汇率
        public decimal FxRate { get; set; }
        // 美元兑换汇率
        public decimal UsdExchangeRate { get; set; }
        // 当前货币类型
        public string? CurrentCurrency { get; set; }
        // 美元类型
        public string? UsdType { get; set; }
        // 是否为焦点项目
        public bool? Spotlight { get; set; }
        // 资助百分比
        public decimal PercentFunded { get; set; }
        // 是否被点赞
        public bool? IsLiked { get; set; }
        // 是否被点踩
        public bool? IsDisliked { get; set; }
        // 是否已发布
        public bool? IsLaunched { get; set; }
        // 预发布是否已激活
        public bool? PrelaunchActivated { get; set; }
        // 来源URL
        public string? SourceUrl { get; set; }

        // 创作者ID (外键)
        public long CreatorId { get; set; }
        // 创作者导航属性
        public virtual Creator? Creator { get; set; }

        // 类别ID (外键)
        public long CategoryId { get; set; }
        // 类别导航属性
        public virtual Category? Category { get; set; }

        // 地点ID (外键)
        public long? LocationId { get; set; }
        // 地点导航属性
        public virtual Location? Location { get; set; }

        // 照片信息 (存储为 JSON 字符串)
        [Column(TypeName = "jsonb")]
        public string? Photo { get; set; }

        // URL信息 (存储为 JSON 字符串)
        [Column(TypeName = "jsonb")]
        public string? Urls { get; set; }
    }

    public class Creator
    {
        // 创作者唯一标识符
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        // 创作者名称
        public string? Name { get; set; }
    }

    public class Category
    {
        // 类别唯一标识符
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        // 类别名称
        public string? Name { get; set; }
        // 类别 slug (URL友好名称)
        public string? Slug { get; set; }
        // 父类别ID
        public long? ParentId { get; set; }
        // 父类别名称
        public string? ParentName { get; set; }
    }

    public class Location
    {
        // 地点唯一标识符
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        // 地点名称
        public string? Name { get; set; }
        // 可显示名称
        public string? DisplayableName { get; set; }
        // 国家代码
        public string? Country { get; set; }
        // 州/省
        public string? State { get; set; }
        // 类型
        public string? Type { get; set; }
    }
}
