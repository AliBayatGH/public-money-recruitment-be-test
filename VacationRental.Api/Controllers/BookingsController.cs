using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VacationRental.Api.Models;

namespace VacationRental.Api.Controllers
{
    [Route("api/v1/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public BookingsController(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        [HttpGet]
        [Route("{bookingId:int}")]
        public BookingViewModel Get(int bookingId)
        {
            if (!_bookings.ContainsKey(bookingId))
                throw new ApplicationException("Booking not found");

            return _bookings[bookingId];
        }

        [HttpPost]
        public ResourceIdViewModel Post(BookingBindingModel model)
        {
            if (model.Nights <= 0)
                throw new ApplicationException("Nigts must be positive");
            if (!_rentals.ContainsKey(model.RentalId))
                throw new ApplicationException("Rental not found");

            int existingUnit = 0;
            for (var i = 0; i < model.Nights; i++)
            {
                List<BookingViewModel> bookingsWithConflict = new List<BookingViewModel>();
                foreach (var booking in _bookings.Values)
                {
                    if (booking.RentalId == model.RentalId
                        && (booking.Start <= model.Start.Date && booking.Start.AddDays(booking.Nights) > model.Start.Date)
                        || (booking.Start < model.Start.AddDays(model.Nights) && booking.Start.AddDays(booking.Nights) >= model.Start.AddDays(model.Nights))
                        || (booking.Start > model.Start && booking.Start.AddDays(booking.Nights) < model.Start.AddDays(model.Nights)))
                    {
                        bookingsWithConflict.Add(booking);
                    }
                }
                if (bookingsWithConflict.Count >= _rentals[model.RentalId].Units)
                    throw new ApplicationException("Not available");

                if (_rentals[model.RentalId].Units > bookingsWithConflict.Count)
                {
                    var bookedRentalUnits = bookingsWithConflict.Select(booking => booking.Unit).Distinct().ToArray();

                    for (var unit = 1; unit <= _rentals[model.RentalId].Units; unit++)
                    {
                        if (!bookedRentalUnits.Contains(unit))
                            existingUnit = unit;
                    }
                }
            }

            var key = new ResourceIdViewModel { Id = _bookings.Keys.Count + 1 };

            _bookings.Add(key.Id, new BookingViewModel
            {
                Id = key.Id,
                Nights = model.Nights,
                RentalId = model.RentalId,
                Start = model.Start.Date,
                Unit = existingUnit
            });

            return key;
        }
    }
}
