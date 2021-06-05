using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/rentals")]
    [ApiController]
    public class RentalsController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public RentalsController(IDictionary<int, RentalViewModel> rentals, IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        [HttpGet]
        [Route("{rentalId:int}")]
        public RentalViewModel Get(int rentalId)
        {
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            return _rentals[rentalId];
        }

        [HttpPost]
        public ResourceIdViewModel Post(RentalBindingModel model)
        {
            var key = new ResourceIdViewModel { Id = _rentals.Keys.Count + 1 };

            _rentals.Add(key.Id, new RentalViewModel
            {
                Id = key.Id,
                Units = model.Units,
                PreparationTimeInDays = model.PreparationTimeInDays
            });

            return key;
        }


        [HttpPut]
        [Route("{rentalId:int}")]
        public IActionResult Update(int rentalId, RentalBindingModel model)
        {
            if (!_rentals.ContainsKey(rentalId))
                return NotFound("Rental not found");

            var rental = _rentals[rentalId];


            if (!(rental.Units != model.Units || rental.PreparationTimeInDays != model.PreparationTimeInDays))
                return NoContent();

            var bookingsWithConflict = _bookings
                .Where(booking =>
                    booking.Value.RentalId == rentalId
                    && ((booking.Value.Start <= DateTime.Now.Date && booking.Value.Start.AddDays(booking.Value.Nights).Date.AddDays(model.PreparationTimeInDays) > DateTime.Now.Date)
                        || (booking.Value.Start < DateTime.MaxValue.AddDays(-model.PreparationTimeInDays).AddDays(model.PreparationTimeInDays) && booking.Value.Start.AddDays(booking.Value.Nights).Date.AddDays(model.PreparationTimeInDays) >= DateTime.MaxValue.AddDays(-model.PreparationTimeInDays).AddDays(model.PreparationTimeInDays))
                        || (booking.Value.Start > DateTime.Now.Date && booking.Value.Start.AddDays(booking.Value.Nights).Date.AddDays(model.PreparationTimeInDays) < DateTime.MaxValue.AddDays(-model.PreparationTimeInDays).AddDays(model.PreparationTimeInDays))));

            var CanRentalBeUpdated = bookingsWithConflict.ToList().Count <= model.Units;
            if (!CanRentalBeUpdated)
                return Conflict("Rental can not be updated");

            rental.Units = model.Units;
            rental.PreparationTimeInDays = model.PreparationTimeInDays;

            return NoContent();
        }
    }
}
