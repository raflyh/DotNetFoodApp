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
           // var userRole = context.UserRoles.Where(m => m.UserId == user.Id).FirstOrDefault();
            var newUserRole = new UserRole
            {
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
            int id,
            UpdateUser input,
            [Service] DotNetFoodDbContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User not Found!"));
            }
            user.Status = input.Status;
            user.Updated = DateTime.Now;

            context.Users.Update(user);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "User Status Updated"
            ));
        }

        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<TransactionStatus> DeleteUserAsync(
            int id,
            [Service] DotNetFoodDbContext context)
        {
            var user = context.Users.Where(o => o.Id == id).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User not Found!"));
            }
            if (user.Status == "BLOCKED")
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return await Task.FromResult(new TransactionStatus
            (
                true, "User Deleted"
            ));
            }
            user.Status = "BLOCKED";
            user.Updated = DateTime.Now;

            context.Users.Update(user);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "User Blocked"
            ));
        }
        //Manager Privilege
        [Authorize(Roles = new [] {"MANAGER"})]
        public async Task<TransactionStatus> RegisterCourierAsync(
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
                Latitude = (float)input.Latitude,
                Longitude = (float)input.Longitude,
                Status = "Free"
            };
            var memberRole = context.Roles.Where(m => m.Name == "COURIER").FirstOrDefault();//Role Auto Courier
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
                true, $"Register Courier {newUser.Username} Success"
            ));
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<TransactionStatus> UpdateCourierAsync(
            int id,
            UpdateUser input,
            [Service] DotNetFoodDbContext context)
        {
            var courierRole = context.Roles.Where(a => a.Name == "COURIER").FirstOrDefault();
            var user = context.Users.Where(o => o.Id == id).Where(a => a.UserRoles.Any(o => o.RoleId == courierRole.Id)).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Courier not Found!"));
            }
            user.Status = input.Status;
            user.Updated = DateTime.Now;

            context.Users.Update(user);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Courier Status Updated"
            ));
        }

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<TransactionStatus> DeleteCourierAsync(
            int id,
            [Service] DotNetFoodDbContext context)
        {
            var courierRole = context.Roles.Where(a => a.Name == "COURIER").FirstOrDefault();
            var user = context.Users.Where(o => o.Id == id).Where(a => a.UserRoles.Any(o => o.RoleId == courierRole.Id)).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "Courier not Found!"));
            }
            var courier = context.UserRoles.Where(o => o.UserId ==user.Id && o.RoleId == 4).FirstOrDefault();
            if (user.Status == "BLOCKED")
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
                return await Task.FromResult(new TransactionStatus
            (
                true, "Courier Deleted"
            ));
            }
            user.Status = "BLOCKED";
            user.Updated = DateTime.Now;

            context.Users.Update(user);

            await context.SaveChangesAsync();

            return await Task.FromResult(new TransactionStatus
            (
                true, "Courier Blocked"
            ));
        }
        //User Privilege
        public async Task<TransactionStatus> RegisterUserAsync(
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
            if (user.Status == "BLOCKED")
            {
                return await Task.FromResult(new UserToken(null, null, "Your Account is Blocked, Please contact the Admin"));
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
            var profile = context.Profiles.Where(o => o.UserId == user.Id).FirstOrDefault();
            if(profile== null)
            {
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
                true, "Profile Has Been Added"
            ));
            }
            profile.Name = input.Name;
            profile.Address = input.Address;
            profile.Phone = input.Phone;

            context.Profiles.Update(profile);
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

        [Authorize]
        public async Task<TransactionStatus> TopUpAsync(
            TopUpBalance input,
            ClaimsPrincipal claimsPrincipal,
            [Service] DotNetFoodDbContext context)
        {
            var userName = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new TransactionStatus(false, "User not Found!"));
            }
            var userBalance = context.Balances.Where(o => o.UserId == user.Id).OrderBy(p => p.Id).LastOrDefault();
            if (userBalance == null)
            {
                var newBalance = new Balance
                {
                    UserId = user.Id,
                    BalanceTotal = input.TopUp,
                    BalanceMutation = input.TopUp,
                    Date = DateTime.Now
                };
                context.Balances.Add(newBalance);

                await context.SaveChangesAsync();
                return await Task.FromResult(new TransactionStatus
                (
                    true, $"Top Up Success! {input.TopUp} Has Been Added to Your Balance!"
                ));
            }
            var addBalance = new Balance
            {
                UserId = user.Id,
                BalanceTotal = userBalance.BalanceTotal+input.TopUp,
                BalanceMutation = input.TopUp,
                Date = DateTime.Now
            };
            context.Balances.Add(addBalance);

            await context.SaveChangesAsync();
            return await Task.FromResult(new TransactionStatus
            (
                true, $"Top Up Success! {input.TopUp} Has Been Added to Your Balance!"
            ));

        }
    }
}
