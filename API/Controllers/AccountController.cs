using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.TokenServices;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenServices;
        private readonly IMapper _mapper;
        public AccountController(DataContext context, ITokenService tokenServices, IMapper mapper)
        {
            _mapper = mapper;
            _tokenServices = tokenServices;
            _context = context;
        }

        //for reister
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.UserName))
            {
                return BadRequest("Username is taken");
            }

            var user = _mapper.Map<AppUser>(registerDto);
            using var hmac = new HMACSHA512();

            user.UserName = registerDto.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenServices.CreateToken(user),
                KnownAs = user.KnownAs
            }; 
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.User.AnyAsync(x => x.UserName == username.ToLower());
        }

        //for login
        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> LogIn(LoginDto loginDto){
            var users = await _context.User.Include(p => p.Photos)
             .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            if(users == null) 
            {
                return Unauthorized("Invalid Username");
            }

            using var hmac = new HMACSHA512(users.PasswordSalt);

            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for(int i =0; i <computeHash.Length; i++)
            {
                if(computeHash[i] != users.PasswordHash[i])
                {
                    return Unauthorized("Invalid Password");
                }
            }
            return new UserDto
            {
                Username = users.UserName,
                Token = _tokenServices.CreateToken(users),
                PhotoUrl = users.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = users.KnownAs

            };
        }
    }
}