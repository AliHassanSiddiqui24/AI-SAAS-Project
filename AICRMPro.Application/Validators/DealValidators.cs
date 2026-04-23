using AICRMPro.Application.DTOs;
using FluentValidation;

namespace AICRMPro.Application.Validators;

public class CreateDealValidator : AbstractValidator<CreateDealDto>
{
    public CreateDealValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Value must be greater than 0");

        RuleFor(x => x.Stage)
            .IsInEnum().WithMessage("Stage must be a valid deal stage");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100");

        RuleFor(x => x.ExpectedCloseDate)
            .GreaterThan(DateTime.Today).WithMessage("Expected close date must be in the future");
    }
}

public class UpdateDealValidator : AbstractValidator<UpdateDealDto>
{
    public UpdateDealValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Value must be greater than 0");

        RuleFor(x => x.Stage)
            .IsInEnum().WithMessage("Stage must be a valid deal stage");

        RuleFor(x => x.Probability)
            .InclusiveBetween(0, 100).WithMessage("Probability must be between 0 and 100");

        RuleFor(x => x.ExpectedCloseDate)
            .GreaterThan(DateTime.Today).WithMessage("Expected close date must be in the future");
    }
}

public class DealFilterValidator : AbstractValidator<DealFilterDto>
{
    public DealFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
    }
}
