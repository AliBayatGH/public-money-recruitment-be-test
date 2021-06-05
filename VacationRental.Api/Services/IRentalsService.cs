using VacationRental.Api.Models;

namespace VacationRental.Api.Services
{
    public interface IRentalsService
    {
        ResourceIdViewModel CreateRental(RentalBindingModel model);
        RentalViewModel GetRentalById(int rentalId);
        void UpdateRental(int rentalId, RentalBindingModel model);
    }
}