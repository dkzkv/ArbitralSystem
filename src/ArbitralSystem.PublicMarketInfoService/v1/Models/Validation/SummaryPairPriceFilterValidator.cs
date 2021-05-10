using System;
using FluentValidation;

namespace ArbitralSystem.PublicMarketInfoService.v1.Models.Validation
{
    /// <summary>
    /// Summary filter validation
    /// </summary>
    public class SummaryPairPriceFilterValidator : AbstractValidator<SummaryPairPriceFilter> 
    {
        /// <inheritdoc />
        public SummaryPairPriceFilterValidator()
        {
            RuleFor(o => o.From).NotNull().When(o => o.To.HasValue)
                .WithMessage("'From' should not be null when 'To' is not!");

            RuleFor(o => o.To).NotNull().When(o => o.From.HasValue)
                .WithMessage("'To' should not be null when 'From' is not!");

            RuleFor(o => o.From).NotEqual(o => o.To).When(o => o.From.HasValue && o.To.HasValue)
                .WithMessage("Dates should not be equal!");

            RuleFor(o => o.To).GreaterThanOrEqualTo(o => o.From).When(o => o.From.HasValue && o.To.HasValue)
                .WithMessage("'From' should not be greater than 'To'!");

            RuleFor(o => new {o.From, o.To}).Must(o=> (o.To-o.From)< TimeSpan.FromDays(31) )
                .When(o=>o.From.HasValue && o.To.HasValue)
                .When(o=>o.From<o.To)
                .WithMessage("Max period is 31 day!");
            
        }
    }
}