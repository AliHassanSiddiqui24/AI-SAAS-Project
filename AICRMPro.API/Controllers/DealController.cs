using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AICRMPro.Application.DTOs;
using AICRMPro.Application.Services;
using AICRMPro.Application.Validators;
using FluentValidation;

namespace AICRMPro.API.Controllers;

[ApiController]
[Route("api/v1/deals")]
[Authorize]
public class DealController : ControllerBase
{
    private readonly IDealService _dealService;
    private readonly IValidator<CreateDealDto> _createValidator;
    private readonly IValidator<UpdateDealDto> _updateValidator;
    private readonly IValidator<DealFilterDto> _filterValidator;

    public DealController(
        IDealService dealService,
        IValidator<CreateDealDto> createValidator,
        IValidator<UpdateDealDto> updateValidator,
        IValidator<DealFilterDto> filterValidator)
    {
        _dealService = dealService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<DealDto>>> GetAll([FromQuery] DealFilterDto filters)
    {
        var validationResult = await _filterValidator.ValidateAsync(filters);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var response = await _dealService.GetAllAsync(filters);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DealDto>> GetById(Guid id)
    {
        try
        {
            var deal = await _dealService.GetByIdAsync(id);
            if (deal == null)
            {
                return NotFound(new { error = "Deal not found" });
            }
            return Ok(deal);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DealDto>> Create([FromBody] CreateDealDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var deal = await _dealService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = deal.Id }, deal);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DealDto>> Update(Guid id, [FromBody] UpdateDealDto dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var deal = await _dealService.UpdateAsync(id, dto);
            return Ok(deal);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _dealService.DeleteAsync(id);
            return Ok(new { message = "Deal deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
