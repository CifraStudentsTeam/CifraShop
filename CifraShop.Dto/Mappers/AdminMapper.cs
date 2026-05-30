using CifraShop.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto.Mappers
{
    public static class AdminMapper
    {
        public static AdminResponse ToResponse(this Admin admin)
        {
            if (admin == null)
                return null;

            return new AdminResponse
            {
                Id = admin.Id,
                Name = admin.Name,
                SurName = admin.SurName,
                EMail = admin.EMail
            };
        }
    }
}
