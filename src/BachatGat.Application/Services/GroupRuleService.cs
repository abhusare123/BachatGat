using BachatGat.Application.Abstractions;
using BachatGat.Application.DTOs;
using BachatGat.Application.Exceptions;
using BachatGat.Application.Interfaces;
using BachatGat.Core.Entities;
using BachatGat.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BachatGat.Application.Services;

public class GroupRuleService(IAppDbContext db, ILogger<GroupRuleService> logger) : IGroupRuleService
{
    // Valid configurable rule keys with their metadata
    private static readonly IReadOnlyList<RuleDefinition> ConfigurableRuleDefinitions =
    [
        new(
            Key: "PenaltyAmount",
            Label: "Penalty for missed savings",
            LabelMr: "बचत न केल्यास दंड",
            Unit: "Rs",
            Description: "Penalty amount imposed on a member who does not deposit savings on time.",
            DescriptionMr: "वेळेवर बचत न केल्यास आकारला जाणारा दंड."
        ),
        new(
            Key: "MaxLoanAmount",
            Label: "Maximum loan amount (without guarantor)",
            LabelMr: "कमाल कर्ज रक्कम (जामीनशिवाय)",
            Unit: "Rs",
            Description: "Maximum loan amount that can be granted to a member without a guarantor. Loans above this limit require two group member guarantors.",
            DescriptionMr: "जामीनशिवाय सदस्याला देता येणारी कमाल कर्ज रक्कम. या मर्यादेपेक्षा जास्त कर्जासाठी दोन सदस्यांची जामीन आवश्यक आहे."
        ),
        new(
            Key: "ExternalLoanApprovalThresholdPercent",
            Label: "External loan approval threshold",
            LabelMr: "बाह्य कर्जासाठी मान्यता टक्केवारी",
            Unit: "%",
            Description: "Minimum percentage of member approval required before the group can take a loan from an external institution (e.g., a bank).",
            DescriptionMr: "बँक किंवा बाह्य संस्थेकडून कर्ज घेण्यापूर्वी किमान किती टक्के सदस्यांची मंजुरी आवश्यक आहे."
        ),
    ];

    private static readonly IReadOnlySet<string> ValidKeys =
        ConfigurableRuleDefinitions.Select(d => d.Key).ToHashSet();

    public async Task<GroupRulesResponseDto> GetRulesAsync(int groupId, int currentUserId)
    {
        var group = await db.Groups
            .Where(g => g.Id == groupId)
            .Select(g => new { g.InterestRatePercent })
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException();

        if (!await IsMemberAsync(groupId, currentUserId)) throw new NotFoundException();

        // Ensure default configs exist for this group
        await SeedDefaultsIfMissingAsync(groupId, currentUserId);

        var storedConfigs = await db.GroupRuleConfigs
            .Where(r => r.GroupId == groupId)
            .ToDictionaryAsync(r => r.RuleKey, r => r.Value);

        var configurableRules = ConfigurableRuleDefinitions.Select(def => new ConfigurableRuleDto(
            def.Key,
            def.Label,
            def.LabelMr,
            storedConfigs.TryGetValue(def.Key, out var val) ? val : "0",
            def.Unit,
            def.Description,
            def.DescriptionMr
        )).ToList();

        return new GroupRulesResponseDto(configurableRules, group.InterestRatePercent);
    }

    public async Task UpdateRuleAsync(int groupId, string ruleKey, string value, int currentUserId)
    {
        if (!ValidKeys.Contains(ruleKey))
            throw new BadRequestException($"Unknown rule key: {ruleKey}");

        var membership = await db.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == currentUserId && m.IsActive)
            ?? throw new NotFoundException();

        if (membership.Role != GroupMemberRole.Admin)
            throw new ForbiddenException("Only Admin can update group rules");

        ValidateValue(ruleKey, value);

        var config = await db.GroupRuleConfigs
            .FirstOrDefaultAsync(r => r.GroupId == groupId && r.RuleKey == ruleKey);

        if (config is null)
        {
            config = new GroupRuleConfig
            {
                GroupId = groupId,
                RuleKey = ruleKey,
                Value = value,
                UpdatedByUserId = currentUserId,
                UpdatedAt = DateTime.UtcNow
            };
            db.GroupRuleConfigs.Add(config);
        }
        else
        {
            config.Value = value;
            config.UpdatedByUserId = currentUserId;
            config.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        logger.LogInformation(
            "Group rule updated — GroupId {GroupId}, RuleKey {RuleKey}, Value {Value}, by UserId {UserId}",
            groupId, ruleKey, value, currentUserId);
    }

    private async Task SeedDefaultsIfMissingAsync(int groupId, int updatedByUserId)
    {
        var existingKeys = await db.GroupRuleConfigs
            .Where(r => r.GroupId == groupId)
            .Select(r => r.RuleKey)
            .ToListAsync();

        var defaults = new Dictionary<string, string>
        {
            ["PenaltyAmount"] = "0",
            ["MaxLoanAmount"] = "0",
            ["ExternalLoanApprovalThresholdPercent"] = "75"
        };

        bool added = false;
        foreach (var (key, defaultValue) in defaults)
        {
            if (!existingKeys.Contains(key))
            {
                db.GroupRuleConfigs.Add(new GroupRuleConfig
                {
                    GroupId = groupId,
                    RuleKey = key,
                    Value = defaultValue,
                    UpdatedByUserId = updatedByUserId,
                    UpdatedAt = DateTime.UtcNow
                });
                added = true;
            }
        }

        if (added) await db.SaveChangesAsync();
    }

    private static void ValidateValue(string ruleKey, string value)
    {
        if (!decimal.TryParse(value, out var numeric) || numeric < 0)
            throw new BadRequestException("Value must be a non-negative number");

        if (ruleKey == "ExternalLoanApprovalThresholdPercent" && numeric > 100)
            throw new BadRequestException("Threshold cannot exceed 100%");
    }

    private Task<bool> IsMemberAsync(int groupId, int userId) =>
        db.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId && m.IsActive);

    private record RuleDefinition(
        string Key,
        string Label,
        string LabelMr,
        string Unit,
        string Description,
        string DescriptionMr);
}
