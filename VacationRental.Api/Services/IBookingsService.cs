using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public interface IBookingsService
    {
        ResourceIdViewModel AddBooking(BookingBindingModel model);
        BookingViewModel GetBookingById(int bookingId);
    }
}