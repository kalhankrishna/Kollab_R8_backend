using KollabR8.Application.DTOs;
using KollabR8.Application.Interfaces;
using KollabR8.Domain.Entities;
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
                    return NotFound("The document doesn't exist.");
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
                    return NotFound("The document doesn't exist.");
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
                    return NotFound("Document not found or already deleted.");
                }

                return Ok("Document deleted successfully.");
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

        [HttpPut("modify-access")]
        public async Task<IActionResult> ModifyAccess([FromBody]int documentId, [FromBody]string accessLevel)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
                bool result = await _documentService.ModifyAccessAsync(documentId, userId, accessLevel);

                if (!result)
                {
                    return NotFound("Could not modify the access to the document.");
                }

                return Ok("Document access successfully modified.");
            }
            catch(UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return Forbid("You do not have permission to modify the access of this document.");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-owned-documents")]
        public async Task<IActionResult> GetOwnedDocuments()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
                var ownedDocs = await _documentService.GetOwnedDocumentsbyUser(userId);

                if (ownedDocs != null)
                {
                    return Ok(ownedDocs);
                }

                return NotFound("The user has no owned documents.");
            }

            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-collaborating-documents")]
        public async Task<IActionResult> GetCollaboratingDocuments()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
                var collabDocs = await _documentService.GetCollaboratingDocumentsbyUser(userId);

                if (collabDocs != null)
                {
                    return Ok(collabDocs);
                }

                return NotFound("This user has no documents where they are a collaborator.");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("add-collaborator")]
        public async Task<IActionResult> AddCollaborator([FromQuery]int documentId, [FromQuery]int collaboratorId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
                bool result = await _collaborationService.AddCollaboratorAsync(documentId, userId, collaboratorId);

                if (result)
                {
                    return Ok("Collaborator added successfully.");
                }

                return BadRequest("Collaborator has already been added.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("remove-collaborator")]
        public async Task<IActionResult> RemoveCollaborator([FromQuery] int documentId, [FromQuery] int collaboratorId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub));
                bool result = await _collaborationService.RemoveCollaboratorAsync(documentId, userId, collaboratorId);

                if (result)
                {
                    return Ok("Collaborator has been removed successfully.");
                }

                return NotFound("Collaborator not found in document.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine(ex.Message);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("get-collaborators")]
        public async Task<IActionResult> GetCollaborators(int id)
        {
            try
            {
                List<UserDto> Collaborators = await _collaborationService.GetCollaboratorsAsync(id);

                if (Collaborators != null)
                {
                    return Ok(Collaborators);
                }

                return NotFound("No collaborators found.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
