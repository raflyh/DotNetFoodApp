using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Models;
using UserService.Settings;

namespace UserService.GraphQL
{
    public class Mutation
    {
        //Admin Privilege
        public async Task<TransactionStatus> RegisterAdminAsync(
            RegisterUser input,
            [Service] DotNetFoodDbContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Username already exist, Try another username!"));
            }
            var newUser = new User
            {
                Email = input.Email,
                Username = input.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password), // encrypt password
                Created = DateTime.Now,
                Updated = DateTime.Now,
                Latitude = 0,
                Longitude = 0,
                Status = "Free"
            };
            // EF
            var memberRole = context.Roles.Where(m => m.Name == "ADMIN").FirstOrDefault();
            if (memberRole == null)
                throw new Exception("Invalid Role");

            var userRole = new UserRole
            {
                RoleId = memberRole.Id,
                UserId = newUser.Id
            };
            newUser.UserRoles.Add(userRole);
            var ret = context.Users.Add(newUser);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, $"Register Admin {newUser.Username} Success"
            ));
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<TransactionStatus> ChangeRoleAsync(
            int id,
            ChangeRole input,
            [Service] DotNetFoodDbContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User not Found!"));
            }
            var userRole = context.UserRoles.Where(m => m.UserId == user.Id).FirstOrDefault();
            var newUserRole = new UserRole
            {
                Id = userRole.Id,
                UserId = user.Id,
                RoleId = input.RoleId
            };
            context.UserRoles.Add(newUserRole);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Role Added"
            ));
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<TransactionStatus> UpdateUserAsync(
            RegisterUser input,
            [Service] DotNetFoodDbContext context)
        {
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User not Found!"));
            }
            var newUser = new User
            {
                Id = user.Id,
                Updated = DateTime.Now,
                Status = input.Status
            };
            context.Users.Add(newUser);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "User Updated"
            ));
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<TransactionStatus> DeleteUserAsync(
            int Id,
            [Service] DotNetFoodDbContext context)
        {
            var user = context.Users.Where(o => o.Id == Id).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User not Found!"));
            }
            context.Users.Remove(user);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "User Deleted"
            ));
        }

        //User Privilege
        public async Task<TransactionStatus> RegisterUserAsync(
            RegisterUser input,
            ClaimsPrincipal claimsPrincipal,
            [Service] DotNetFoodDbContext context)
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Username already exist, Try another username!"));
            }
            var newUser = new User
            {
                Email = input.Email,
                Username = input.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password), // encrypt password
                Created = DateTime.Now,
                Updated = DateTime.Now,
                Latitude = (float)input.Latitude,
                Longitude = (float)input.Longitude,
                Status = "Free"
            };
            var memberRole = context.Roles.Where(m => m.Name == "BUYER").FirstOrDefault();//Role Auto Buyer
            if (memberRole == null)
                throw new Exception("Invalid Role");

            var userRole = new UserRole
            {
                RoleId = memberRole.Id,
                UserId = newUser.Id
            };
            newUser.UserRoles.Add(userRole);
            context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, $"Register User {newUser.Username} Success"
            ));
           
        }

        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings, // setting token
            [Service] DotNetFoodDbContext context) // EF
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                // generate jwt token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                // jwt payload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.UserId == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(5);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims, // jwt payload
                    signingCredentials: credentials // signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Invalid Username or Password!"));
        }

        [Authorize]
        public async Task<TransactionStatus> EditProfileAsync(
            EditProfile input,
            ClaimsPrincipal claimsPrincipal,
            [Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User not Found!"));
            }
            var newProfile = new Profile
            {
                UserId = user.Id,
                Name = input.Name,
                Address = input.Address,
                Phone = input.Phone
            };
            context.Profiles.Add(newProfile);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Profile Has Been Saved"
            ));
          
        }

        [Authorize]
        public async Task<TransactionStatus> ChangePasswordAsync(
            ChangePasswordInput input,
            ClaimsPrincipal claimsPrincipal,
            [Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User not Found!"));
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password); // encrypt password
            user.Updated = DateTime.Now;

            context.Users.Update(user);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Password is Changed"
            ));
        }
    }
}
