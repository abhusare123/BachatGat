using System.ComponentModel.DataAnnotations;

namespace BachatGat.Application.DTOs;

public record ConfigurableRuleDto(
    string Key,
    string Label,
    string LabelMr,
    string Value,
    string Unit,
    string Description,
    string DescriptionMr);

public record GroupRulesResponseDto(
    List<ConfigurableRuleDto> ConfigurableRules,
    decimal InterestRatePercent);

public record UpdateGroupRuleRequest([Required] string Value);
