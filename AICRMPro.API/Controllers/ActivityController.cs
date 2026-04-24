using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AICRMPro.Application.DTOs;
using AICRMPro.Application.Services;
using FluentValidation;

namespace AICRMPro.API.Controllers;

[ApiController]
[Route("api/v1/activities")]
[Authorize]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly IValidator<CreateActivityDto> _createValidator;
    private readonly IValidator<UpdateActivityDto> _updateValidator;
    private readonly IValidator<ActivityFilterDto> _filterValidator;
    private readonly IValidator<CompleteActivityDto> _completeValidator;

    public ActivityController(
        IActivityService activityService,
        IValidator<CreateActivityDto> createValidator,
        IValidator<UpdateActivityDto> updateValidator,
        IValidator<ActivityFilterDto> filterValidator,
        IValidator<CompleteActivityDto> completeValidator)
    {
        _activityService = activityService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
        _completeValidator = completeValidator;
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create([FromBody] CreateActivityDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var activity = await _activityService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetByClientId), new { clientId = activity.ClientId }, activity);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("clients/{clientId}/activities")]
    public async Task<ActionResult<PagedResponse<ActivityDto>>> GetByClientId(Guid clientId, [FromQuery] ActivityFilterDto filters)
    {
        var validationResult = await _filterValidator.ValidateAsync(filters);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var response = await _activityService.GetByClientIdAsync(clientId, filters);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ActivityDto>> Update(Guid id, [FromBody] UpdateActivityDto dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var activity = await _activityService.UpdateAsync(id, dto);
            return Ok(activity);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<ActivityDto>> Complete(Guid id, [FromBody] CompleteActivityDto dto)
    {
        var validationResult = await _completeValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var activity = await _activityService.CompleteAsync(id, dto);
            return Ok(activity);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
