using CifraShop.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CifraShop.Dto.Mappers
{
    public static class StudentMapper
    {
        public static StudentResponse ToResponse(Student student)
        {
            return new StudentResponse
            {
                Id = student.Id,
                LoginName = student.LoginName,
                DateOfBirth = student.DateOfBirth,
                Balance = student.Balance
            };
        }
    }
}
