using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AICRMPro.Application.DTOs;
using AICRMPro.Application.Services;
using AICRMPro.Application.Validators;
using FluentValidation;

namespace AICRMPro.API.Controllers;

[ApiController]
[Route("api/v1/clients")]
[Authorize]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly IValidator<CreateClientDto> _createValidator;
    private readonly IValidator<UpdateClientDto> _updateValidator;
    private readonly IValidator<ClientFilterDto> _filterValidator;

    public ClientController(
        IClientService clientService,
        IValidator<CreateClientDto> createValidator,
        IValidator<UpdateClientDto> updateValidator,
        IValidator<ClientFilterDto> filterValidator)
    {
        _clientService = clientService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _filterValidator = filterValidator;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ClientDto>>> GetAll([FromQuery] ClientFilterDto filters)
    {
        var validationResult = await _filterValidator.ValidateAsync(filters);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var response = await _clientService.GetAllAsync(filters);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetById(Guid id)
    {
        try
        {
            var client = await _clientService.GetByIdAsync(id);
            if (client == null)
            {
                return NotFound(new { error = "Client not found" });
            }
            return Ok(client);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var client = await _clientService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClientDto>> Update(Guid id, [FromBody] UpdateClientDto dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var client = await _clientService.UpdateAsync(id, dto);
            return Ok(client);
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
            await _clientService.DeleteAsync(id);
            return Ok(new { message = "Client deleted successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
