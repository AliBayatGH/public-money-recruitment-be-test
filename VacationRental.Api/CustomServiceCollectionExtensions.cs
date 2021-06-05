using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using VacationRental.Api.Models;
using VacationRental.Api.Services;

namespace VacationRental.Api
{
    public static class CustomServiceCollectionExtensions
    {
        public static IServiceCollection AddRentalDictionary(this IServiceCollection services) => services.AddSingleton<IDictionary<int, RentalViewModel>>(new Dictionary<int, RentalViewModel>());
        public static IServiceCollection AddBookingDictionary(this IServiceCollection services) => services.AddSingleton<IDictionary<int, BookingViewModel>>(new Dictionary<int, BookingViewModel>());
        public static IServiceCollection AddBookingsService(this IServiceCollection services) => services.AddScoped<IBookingsService, BookingsService>();
        public static IServiceCollection AddCalendarService(this IServiceCollection services) => services.AddScoped<ICalendarService, CalendarService>();
        public static IServiceCollection AddRentalsService(this IServiceCollection services) => services.AddScoped<IRentalsService, RentalsService>();
    }
}
