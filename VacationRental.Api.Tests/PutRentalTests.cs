using System.Net.Http;
using System.Threading.Tasks;
using VacationRental.Api.Models;
using Xunit;

namespace VacationRental.Api.Tests
{
    [Collection("Integration")]
    public class PutRentalTests
    {
        private readonly HttpClient _client;

        public PutRentalTests(IntegrationFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task GivenCompleteRequest_WhenPutRental_ThenAGetReturnsTheUpdatedRental()
        {
            var newRental = new RentalBindingModel
            {
                Units = 25,
                PreparationTimeInDays = 2
            };

            ResourceIdViewModel postResult;
            using (var postResponse = await _client.PostAsJsonAsync($"/api/v1/rentals", newRental))
            {
                Assert.True(postResponse.IsSuccessStatusCode);
                postResult = await postResponse.Content.ReadAsAsync<ResourceIdViewModel>();
            }

            await _client.GetAsync($"/api/v1/rentals/{postResult.Id}");

            var updateRental = new RentalBindingModel
            {
                Units = 24,
                PreparationTimeInDays = 1
            };

            using (var putResponse = await _client.PutAsJsonAsync($"/api/v1/rentals/{postResult.Id}", updateRental))
            {
                Assert.True(putResponse.IsSuccessStatusCode);
            }

            using (var getResponse = await _client.GetAsync($"/api/v1/rentals/{postResult.Id}"))
            {
                Assert.True(getResponse.IsSuccessStatusCode);

                var getResult = await getResponse.Content.ReadAsAsync<RentalViewModel>();
                Assert.Equal(updateRental.Units, getResult.Units);
                Assert.Equal(updateRental.PreparationTimeInDays, getResult.PreparationTimeInDays);
            }
        }
    }
}
