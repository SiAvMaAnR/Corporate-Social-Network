﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSN.Domain.Entities.Common
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
    }
}
