using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RealEstateApi.Application.Dtos;
using RealEstateApi.Application.Interfaces;

namespace RealEstateApi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertyController : ControllerBase
    {
        private readonly IPropertyService _propertyService;

        public PropertyController(IPropertyService propertyService)
        {
            _propertyService = propertyService;
        }

        /// <summary>
        /// Retrieves a paginated, filtered list of properties based on the specified criteria.
        /// </summary>
        /// <param name="filter">Filtering and pagination criteria.</param>
        /// <response code="200">Returns the filtered list of properties.</response>
        /// <response code="500">An internal error occurred while retrieving the properties.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PropertyListDto>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Get([FromQuery] PropertyFilterDto filter)
        {
            try
            {
                var properties = await _propertyService.GetFilteredPropertiesAsync(filter);
                return Ok(properties);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a single property by its identifier.
        /// </summary>
        /// <param name="id">The property identifier.</param>
        /// <response code="200">Returns the details of the specified property.</response>
        /// <response code="404">No property found with the given ID.</response>
        /// <response code="500">An internal error occurred while retrieving the property.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PropertyListDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetPropertyById(string id)
        {
            try
            {
                var propertyListDto = await _propertyService.GetFilteredPropertiesAsync(new PropertyFilterDto
                {
                    Name = null,
                    Address = null
                });

                var property = propertyListDto.FirstOrDefault(p => p.IdProperty == id);
                if (property == null)
                    return NotFound();

                return Ok(property);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new property for the currently authenticated owner.
        /// </summary>
        /// <param name="createDto">The property creation data.</param>
        /// <remarks>
        /// This endpoint requires authentication.
        /// A multipart/form-data request is expected, where <c>createDto</c> fields are sent in form-data.
        /// </remarks>
        /// <response code="201">Returns the newly created property ID.</response>
        /// <response code="400">Invalid input data or an error occurred while creating the property.</response>
        [Authorize]
        [HttpPost]
        [ProducesResponseType(typeof(CreatePropertyResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromForm] PropertyCreateDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var ownerId = getOwnerId();
                createDto.IdOwner = ownerId;
                var newId = await _propertyService.CreatePropertyAsync(createDto);
                return CreatedAtAction(nameof(GetPropertyById), new { id = newId }, new { IdProperty = newId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing property for the currently authenticated owner.
        /// </summary>
        /// <param name="id">The property identifier to update.</param>
        /// <param name="updateDto">The updated property data.</param>
        /// <remarks>This endpoint requires authentication.</remarks>
        /// <response code="204">Property was updated successfully.</response>
        /// <response code="404">No property found, or not owned by the current user.</response>
        /// <response code="400">An error occurred while updating the property.</response>
        [Authorize]
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Update([FromForm] PropertyUpdateDto updateDto)
        {
            try
            {
                var ownerId = getOwnerId();
                updateDto.IdOwner = ownerId;
                var success = await _propertyService.UpdatePropertyAsync(updateDto);
                if (!success)
                    return NotFound(new { Message = "Property not found or not updated." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a property owned by the currently authenticated user.
        /// </summary>
        /// <param name="id">The property identifier to delete.</param>
        /// <remarks>This endpoint requires authentication.</remarks>
        /// <response code="204">Property was deleted successfully.</response>
        /// <response code="404">No property found, or not owned by the current user.</response>
        /// <response code="400">An error occurred while deleting the property.</response>
        [Authorize]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var ownerId = getOwnerId();
                var success = await _propertyService.DeletePropertyAsync(id, ownerId);
                if (!success)
                    return NotFound(new { Message = "Property not found or not deleted." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        private string getOwnerId()
        {
            var ownerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerId))
                throw new UnauthorizedAccessException("Invalid or missing owner information.");
            return ownerId;
        }

        /// <summary>
        /// Represents the response returned when a property is successfully created.
        /// </summary>
        public class CreatePropertyResponse
        {
            /// <summary>
            /// The newly generated property identifier.
            /// </summary>
            public string IdProperty { get; set; } = null!;
        }
    }
}
