using KollabR8.Application.DTOs;
using KollabR8.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KollabR8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly ICollaborationService _collaborationService;

        public DocumentController(IDocumentService documentService, ICollaborationService collaborationService)
        {
            _documentService = documentService;
            _collaborationService = collaborationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentDto documentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
                var docId = await _documentService.CreateDocumentAsync(documentDto.Title, documentDto.Access, userId, documentDto.CollaboratorIds);
                return CreatedAtAction(nameof(GetDocument), new { Id = docId });
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocument(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
                var document = await _documentService.GetDocumentbyIdAsync(id, userId);
                if (document == null)
                {
                    return NotFound();
                }
                return Ok(document);
            }
            catch(UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return Forbid("You do not have permission to view this document.");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDocument(int id, [FromBody]UpdateDocumentDto documentDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
                var document = await _documentService.UpdateDocumentAsync(id, documentDto.Title, documentDto.content, userId);
                if (document == null)
                {
                    return NotFound();
                }
                return Ok(document);
            }
            catch(UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return Forbid("You do not have permission to modify this document.");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
                bool result = await _documentService.DeleteDocumentAsync(id, userId);

                if (!result)
                {
                    return NotFound(new { message = "Document not found or already deleted." });
                }

                return Ok(new { message = "Document deleted successfully." });
            }
            catch(UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return Forbid("You do not have permission to delete this document.");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
