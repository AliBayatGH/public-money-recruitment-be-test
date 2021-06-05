using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;
using VacationRental.Api.Services;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/rentals")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IRentalsService _rentalsService;

        public RentalsController(IRentalsService rentalsService)
        {
            _rentalsService = rentalsService;
        }

        [HttpGet]
        [Route("{rentalId:int}")]
        public RentalViewModel Get(int rentalId)
        {
           return _rentalsService.GetRentalById(rentalId);
        }

        [HttpPost]
        public ResourceIdViewModel Post(RentalBindingModel model)
        {
           return _rentalsService.CreateRental(model);
        }

        [HttpPut]
        [Route("{rentalId:int}")]
        public IActionResult Update(int rentalId, RentalBindingModel model)
        {
            _rentalsService.UpdateRental(rentalId, model);

            return NoContent();
        }
    }
}
