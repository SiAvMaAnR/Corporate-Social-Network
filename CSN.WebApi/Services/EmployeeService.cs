using CSN.Domain.Entities.Companies;
using CSN.Domain.Entities.Employees;
using CSN.Domain.Interfaces.UnitOfWork;
using CSN.Infrastructure.Helpers;
using CSN.Infrastructure.Interfaces.Services;
using CSN.Infrastructure.Models.Common;
using CSN.Infrastructure.Models.EmployeeDto;
using CSN.Persistence.DBContext;
using CSN.WebApi.Extensions;
using CSN.WebApi.Extensions.CustomExceptions;
using CSN.WebApi.Services.Common;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using System.Text.Json;

namespace CSN.WebApi.Services
{
    public class EmployeeService : BaseService<Employee>, IEmployeeService
    {
        public readonly IConfiguration configuration;
        public readonly IDataProtectionProvider protection;

        public EmployeeService(IUnitOfWork unitOfWork, IHttpContextAccessor context, IConfiguration configuration,
            IDataProtectionProvider protection) : base(unitOfWork, context)
        {
            this.configuration = configuration;
            this.protection = protection;
        }

        public async Task<EmployeeLoginResponse> LoginAsync(EmployeeLoginRequest request)
        {
            Employee? employee = await this.unitOfWork.Employee.GetAsync(employee => employee.Email == request.Email);

            if (employee == null)
            {
                throw new NotFoundException("Account not found");
            }

            bool isVerify = AuthOptions.VerifyPasswordHash(request.Password, employee.PasswordHash, employee.PasswordSalt);

            if (!isVerify)
            {
                throw new BadRequestException("Incorrect email or password");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.Login),
                new Claim(ClaimTypes.Email, employee.Email),
                new Claim(ClaimTypes.Role, employee.Role)
            };

            string token = AuthOptions.CreateToken(claims, new Dictionary<string, string>()
            {
                { "secretKey", this.configuration["Authorization:SecretKey"] },
                { "audience", this.configuration["Authorization:Audience"] },
                { "issuer" , this.configuration["Authorization:Issuer"] },
                { "lifeTime" , this.configuration["Authorization:LifeTime"] },
            });

            return new EmployeeLoginResponse()
            {
                IsSuccess = true,
                TokenType = "Bearer",
                Token = token
            };
        }

        public async Task<EmployeeRegisterResponse> RegisterAsync(EmployeeRegisterRequest request)
        {
            var secretKey = this.configuration["Invite:SecretKey"];

            IDataProtector protector = this.protection.CreateProtector(secretKey);
            string inviteJson = protector.Unprotect(request.Invite);

            Invite? invite = JsonSerializer.Deserialize<Invite>(inviteJson);

            if (invite == null)
            {
                throw new BadRequestException("Incorrect invite");
            }

            if (await this.unitOfWork.Employee.AnyAsync(employee => employee.Email == invite.Email))
            {
                throw new BadRequestException("Account already exists");
            }

            if (await this.unitOfWork.Company.AnyAsync(company => company.Email == invite.Email))
            {
                throw new BadRequestException("Account already exists");
            }

            var invitation = await this.unitOfWork.Invitation.GetAsync(invitation => invitation.Id == invite.Id);

            if (invitation == null || !invitation.IsActive)
            {
                throw new NotFoundException("Invitation not found or inactive");
            }

            if (!AuthOptions.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt))
            {
                throw new BadRequestException("Incorrect password");
            }

            byte[] image = Convert.FromBase64String(request.Image ?? "");

            Company? company = await this.unitOfWork.Company.GetAsync(company => company.Id == invite.CompanyId);

            if (company == null)
            {
                throw new BadRequestException("Incorrect company");
            }

            var employee = new Employee()
            {
                Login = request.Login,
                Email = invite.Email,
                Image = image,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = invite.Role.ToString(),
                CompanyId = invite.CompanyId,
            };

            await Task.WhenAll(
                this.unitOfWork.Employee.AddAsync(employee),
                this.unitOfWork.Invitation.DeleteAsync(invitation)
            );

            await this.unitOfWork.SaveChangesAsync();

            return new EmployeeRegisterResponse()
            {
                IsSuccess = true
            };
        }

        public async Task<EmployeeInfoResponse> GetInfoAsync(EmployeeInfoRequest request)
        {
            Employee? employee = await this.claimsPrincipal!.GetEmployeeAsync(this.unitOfWork, employee => employee.Company);

            if (employee == null)
            {
                throw new NotFoundException("Account is not found");
            }

            return new EmployeeInfoResponse()
            {
                Id = employee.Id,
                Login = employee.Login,
                Email = employee.Email,
                Image = employee.Image,
                Role = employee.Role,
                CompanyId = employee.CompanyId,
                Company = new EmployeeCompany()
                {
                    Id = employee.Company.Id,
                    Login = employee.Company.Login,
                    Email = employee.Company.Email,
                    Role = employee.Company.Role,
                    Image = employee.Company.Image,
                    Description = employee.Company.Description
                }
            };
        }

        public async Task<EmployeeRemoveResponse> RemoveAsync(EmployeeRemoveRequest request)
        {
            Employee? employee = await this.claimsPrincipal!.GetEmployeeAsync(this.unitOfWork);

            if (employee == null)
            {
                throw new NotFoundException("Account is not found");
            }

            await this.unitOfWork.Employee.DeleteAsync(employee);
            await this.unitOfWork.SaveChangesAsync();

            return new EmployeeRemoveResponse()
            {
                IsSuccess = true
            };
        }
    }
}
