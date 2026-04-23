using AICRMPro.Application.DTOs;
using FluentValidation;

namespace AICRMPro.Application.Validators;

public class CreateClientValidator : AbstractValidator<CreateClientDto>
{
    public CreateClientValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters");

        RuleFor(x => x.Company)
            .MaximumLength(200).WithMessage("Company cannot exceed 200 characters");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid client status");

        RuleFor(x => x.LeadScore)
            .InclusiveBetween(0, 100).WithMessage("Lead score must be between 0 and 100");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }
}

public class UpdateClientValidator : AbstractValidator<UpdateClientDto>
{
    public UpdateClientValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Phone cannot exceed 20 characters");

        RuleFor(x => x.Company)
            .MaximumLength(200).WithMessage("Company cannot exceed 200 characters");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid client status");

        RuleFor(x => x.LeadScore)
            .InclusiveBetween(0, 100).WithMessage("Lead score must be between 0 and 100");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters");
    }
}

public class ClientFilterValidator : AbstractValidator<ClientFilterDto>
{
    public ClientFilterValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");
    }
}
