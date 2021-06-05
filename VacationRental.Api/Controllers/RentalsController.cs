﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            GetRentalById(rentalId);

            return _rentals[rentalId];
        }

        private void GetRentalById(int rentalId)
        {
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");
        }

        [HttpPost]
        public ResourceIdViewModel Post(RentalBindingModel model)
        {
           return CreateRental(model);
        }

        private ResourceIdViewModel CreateRental(RentalBindingModel model)
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
            UpdateRental(rentalId, model);

            return NoContent();
        }

        private void UpdateRental(int rentalId, RentalBindingModel model)
        {
            if (!_rentals.ContainsKey(rentalId))
                throw new ApplicationException("Rental not found");

            var rental = _rentals[rentalId];

            if (rental.Units != model.Units || rental.PreparationTimeInDays != model.PreparationTimeInDays)
            {
                var bookingsWithConflict = GetBookingsWithConflict(rentalId, DateTime.Now.Date, DateTime.MaxValue.AddDays(-model.PreparationTimeInDays), model.PreparationTimeInDays);

                var CanRentalBeUpdated = bookingsWithConflict.ToList().Count <= model.Units;
                if (!CanRentalBeUpdated)
                    throw new ApplicationException("Rental can not be updated");

                rental.Units = model.Units;
                rental.PreparationTimeInDays = model.PreparationTimeInDays;
            }
        }

        private IEnumerable<BookingViewModel> GetBookingsWithConflict(int rentalId, DateTime start, DateTime end, int preparationDays)
        {
            foreach (var booking in _bookings.Values)
            {
                if (booking.RentalId == rentalId
                                        && ((booking.Start <= start && booking.End.AddDays(preparationDays) > start)
                                            || (booking.Start < end.AddDays(preparationDays) && booking.End.AddDays(preparationDays) >= end.AddDays(preparationDays))
                                            || (booking.Start > start && booking.End.AddDays(preparationDays) < end.AddDays(preparationDays))))
                {
                    yield return booking;
                }
            }
        }
    }
}
