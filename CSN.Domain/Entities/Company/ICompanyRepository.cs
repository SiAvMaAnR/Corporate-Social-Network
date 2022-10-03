﻿using CSN.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSN.Domain.Entities.Company
{
    public interface ICompanyRepository : IAsyncRepository<Company>
    {
    }
}
