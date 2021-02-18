using System;
using AutoMapper;
using backend_week02_oef1.Models;

namespace backend_week02_oef1.DTO {
    public class AutoMapping : Profile {
        public AutoMapping() {
            CreateMap<VaccinationRegistration, VaccinationRegistrationDTO>();
        }
    }
}
