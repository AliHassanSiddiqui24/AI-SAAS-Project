using AICRMPro.Application.DTOs;
using FluentValidation;

namespace AICRMPro.Application.Validators;

public class CreateActivityValidator : AbstractValidator<CreateActivityDto>
{
    public CreateActivityValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("Client ID is required");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid activity type");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
    }
}

public class UpdateActivityValidator : AbstractValidator<UpdateActivityDto>
{
    public UpdateActivityValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid activity type");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
    }
}

public class ActivityFilterValidator : AbstractValidator<ActivityFilterDto>
{
    public ActivityFilterValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().When(x => x.Type.HasValue).WithMessage("Invalid activity type");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
    }
}

public class CompleteActivityValidator : AbstractValidator<CompleteActivityDto>
{
    public CompleteActivityValidator()
    {
        RuleFor(x => x.Outcome)
            .MaximumLength(2000).WithMessage("Outcome cannot exceed 2000 characters");
    }
}
