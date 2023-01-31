using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;

        public readonly IMapper _Mapper;
        public readonly IPhotoService _PhotoService;

        public UsersController(DataContext context, IMapper mapper, IPhotoService photoService)
        {
            _PhotoService = photoService;
            _context = context;
            _Mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await _context.User.ProjectTo<MemberDto>(_Mapper.ConfigurationProvider).ToListAsync();
            return Ok(users);
        }


        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _context.User.Where(x => x.UserName == username).ProjectTo<MemberDto>(_Mapper.ConfigurationProvider).SingleOrDefaultAsync();
        }


        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _context.User.SingleOrDefaultAsync(x => x.UserName == User.GetUserName());

            if (user == null) return NotFound();
            _Mapper.Map(memberUpdateDto, user);

            if (await _context.SaveChangesAsync() > 0) return NoContent();
            return BadRequest("Failed To update Changes");
        }


        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _context.User.Include(x => x.Photos).SingleOrDefaultAsync(x => x.UserName == User.GetUserName());

            if (user == null) return NotFound();

            var result = await _PhotoService.AddPhotoAsync(file);

            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUri.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0) photo.IsMain = true;

            user.Photos.Add(photo);

            if (await _context.SaveChangesAsync() > 0)
            {
                return CreatedAtAction(nameof(GetUser), new { username = user.UserName }, _Mapper.Map<PhotoDto>(photo));
            };

            return BadRequest("Problem Adding Photo");
        }


        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _context.User.Include(x => x.Photos).SingleOrDefaultAsync( x => x.UserName == User.GetUserName());
            
            if(user == null) return NotFound();

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null) return NotFound();

            if(photo.IsMain) return BadRequest("This is already a main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if(currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if(await _context.SaveChangesAsync()>0) return NoContent();

            return BadRequest("problem setting the main photo");
        }

        [HttpDelete("delete-photos/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _context.User.Include(x => x.Photos).SingleOrDefaultAsync(x => x.UserName == User.GetUserName());
           
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if(photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if(photo.PublicId != null)
            {
                var result = await _PhotoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await _context.SaveChangesAsync() > 0) return Ok();

            return BadRequest("Problem deleting photo");


        }

    }
}