using System;
using System.Collections.Generic;
using System.Linq;
using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public class BookingsService : IBookingsService
    {
        private readonly IDictionary<int, RentalViewModel> _rentals;
        private readonly IDictionary<int, BookingViewModel> _bookings;

        public BookingsService(
            IDictionary<int, RentalViewModel> rentals,
            IDictionary<int, BookingViewModel> bookings)
        {
            _rentals = rentals;
            _bookings = bookings;
        }

        public BookingViewModel GetBookingById(int bookingId)
        {
            if (!_bookings.ContainsKey(bookingId))
                throw new ApplicationException("Booking not found");

            return _bookings[bookingId];
        }

        public ResourceIdViewModel AddBooking(BookingBindingModel model)
        {
            if (model.Nights <= 0)
                throw new ApplicationException("Nigts must be positive");
            if (!_rentals.ContainsKey(model.RentalId))
                throw new ApplicationException("Rental not found");

            var bookingsWithConflict = GetBookingsWithConflict(model.RentalId, model.Start, model.End, _rentals[model.RentalId].PreparationTimeInDays).ToList();

            if (bookingsWithConflict.Count >= _rentals[model.RentalId].Units)
                throw new ApplicationException("Not available");

            var availableUnit = GetAvailableUnit(model.RentalId, bookingsWithConflict);

            var key = new ResourceIdViewModel { Id = _bookings.Keys.Count + 1 };

            _bookings.Add(key.Id, new BookingViewModel
            {
                Id = key.Id,
                Nights = model.Nights,
                RentalId = model.RentalId,
                Start = model.Start.Date,
                Unit = availableUnit
            });

            return key;
        }

        private int GetAvailableUnit(int rentalId, List<BookingViewModel> overlappingBookings)
        {
            int availableUnit = 0;

            if (_rentals[rentalId].Units > overlappingBookings.Count)
            {
                var bookedRentalUnits = overlappingBookings.Select(booking => booking.Unit).Distinct().ToArray();

                for (var unit = 1; unit <= _rentals[rentalId].Units; unit++)
                {
                    if (!bookedRentalUnits.Contains(unit))
                        availableUnit = unit;
                }
            }

            return availableUnit;
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
