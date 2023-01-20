using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
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

        public UsersController(DataContext context, IMapper mapper)
        {
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
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.User.SingleOrDefaultAsync(x => x.UserName == username);

            if (user == null) return NotFound();
            _Mapper.Map(memberUpdateDto, user);

            if (await _context.SaveChangesAsync() > 0) return NoContent();
            return BadRequest("Failed To update Changes");
        }
    }
}