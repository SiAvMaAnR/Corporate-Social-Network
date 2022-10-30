﻿using CSN.Domain.Entities.Common;
using CSN.Domain.Entities.Companies;
using CSN.Domain.Interfaces.UnitOfWork;
using CSN.Persistence.DBContext;
using CSN.Persistence.Repositories;
using CSN.Persistence.UnitOfWork;
using System.Security.Claims;

namespace CSN.WebApi.Services.Common
{
    public abstract class BaseService<TEntity> where TEntity : BaseEntity
    {
        protected readonly IUnitOfWork unitOfWork;
        protected readonly EFContext eFContext;
        private readonly ClaimsPrincipal claimsPrincipal;

        public BaseService(EFContext eFContext, IUnitOfWork unitOfWork, ClaimsPrincipal claimsPrincipal)
        {
            this.eFContext = eFContext;
            this.unitOfWork = unitOfWork;
            this.claimsPrincipal = claimsPrincipal;
        }
    }
}
