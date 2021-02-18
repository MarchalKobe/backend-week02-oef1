using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AutoMapper;
using backend_week02_oef1.Configuration;
using backend_week02_oef1.DTO;
using backend_week02_oef1.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace backend_week02_oef1.Controllers {
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public class VaccinationController : ControllerBase {
        private CSVSettings _settings;
        private static List<VaccinationType> _vaccinationTypes;
        private static List<VaccinationLocation> _vaccinationLocations;
        private static List<VaccinationRegistration> _vaccinationRegistrations;
        private IMapper _mapper;

        public VaccinationController(IOptions<CSVSettings> settings, IMapper mapper) {
            _settings = settings.Value;
            _mapper = mapper;

            if(_vaccinationTypes == null) {
                _vaccinationTypes = ReadCSVVaccins();
            }

            if(_vaccinationLocations == null) {
                _vaccinationLocations = ReadCSVLocations();
            }

            if(_vaccinationRegistrations == null) {
                _vaccinationRegistrations = ReadCSVRegistrations();
            }
        }

        private List<VaccinationType> ReadCSVVaccins() {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            using(var reader = new StreamReader(_settings.CSVVaccins)) {
                using(var csv = new CsvReader(reader, config)) {
                    var records = csv.GetRecords<VaccinationType>();
                    return records.ToList<VaccinationType>();
                };
            };
        }
        
        private List<VaccinationLocation> ReadCSVLocations() {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            using(var reader = new StreamReader(_settings.CSVLocations)) {
                using(var csv = new CsvReader(reader, config)) {
                    var records = csv.GetRecords<VaccinationLocation>();
                    return records.ToList<VaccinationLocation>();
                };
            };
        }

        private List<VaccinationRegistration> ReadCSVRegistrations() {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            using(var reader = new StreamReader(_settings.CSVRegistrations)) {
                using(var csv = new CsvReader(reader, config)) {
                    var records = csv.GetRecords<VaccinationRegistration>();
                    return records.ToList<VaccinationRegistration>();
                };
            };
        }

        private void SaveRegistrations() {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                HasHeaderRecord = false,
                Delimiter = ";"
            };

            using(var writer = new StreamWriter(_settings.CSVRegistrations)) {
                using(var csv = new CsvWriter(writer, config)) {
                    csv.WriteRecords(_vaccinationRegistrations);
                };
            };
        }

        [HttpGet]
        [Route("/vaccins")]
        public ActionResult<List<VaccinationType>> GetVaccins() {
            return new OkObjectResult(_vaccinationTypes);
        }

        [HttpGet]
        [Route("/locations")]
        public ActionResult<List<VaccinationLocation>> GetLocations() {
            return new OkObjectResult(_vaccinationLocations);
        }

        [HttpGet]
        [Route("/registrations")]
        public ActionResult<List<VaccinationRegistration>> GetRegistrations(string date = "") {
            if(string.IsNullOrEmpty(date)) {
                return new OkObjectResult(_vaccinationRegistrations);
            } else {
                return _vaccinationRegistrations.Where(r => r.VaccinationDate == DateTime.Parse(date)).ToList<VaccinationRegistration>();
            }
        }

        [HttpGet]
        [Route("/registrations")]
        [MapToApiVersion("2.0")]
        public ActionResult<List<VaccinationRegistrationDTO>> GetRegistrationsSmall() {
            return _mapper.Map<List<VaccinationRegistrationDTO>>(_vaccinationRegistrations);
        }

        [HttpPost]
        [Route("registration")]
        public ActionResult<VaccinationRegistration> AddRegistration(VaccinationRegistration newRegistration) {
            if(newRegistration == null) {
                return new BadRequestResult();
            }

            if(_vaccinationTypes.Where(vt => vt.VaccinationTypeId == newRegistration.VaccinationTypeId).Count() == 0) {
                return new BadRequestResult();
            }

            if(_vaccinationLocations.Where(vl => vl.VaccinationLocationId == newRegistration.VaccinationLocationId).Count() == 0) {
                return new BadRequestResult();
            }
            
            newRegistration.VaccinationRegistrationId = Guid.NewGuid();
            _vaccinationRegistrations.Add(newRegistration);
            SaveRegistrations();
            return newRegistration;
        }
    }
}
