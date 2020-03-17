using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FlightProject.POCOs;

namespace FlightProjectDBGenerator
{
    class RandomGenerator
    {
        private Random Random = new Random();
        
        public Administrator AdministratorGenerator()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RANDOM_USER_URL"]);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Administrator administrator = new Administrator();
              
            HttpResponseMessage response = client.GetAsync("").Result;
            if (response.IsSuccessStatusCode)
            {
                User adminAPI = response.Content.ReadAsAsync<User>().Result;
                administrator = new Administrator(adminAPI.results[0].login.username, adminAPI.results[0].login.password);
            }
            client.Dispose();
            return administrator;
        }

        public AirlineCompany AirlineCompanyGenerator()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RANDOM_USER_URL"]);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            AirlineCompany airlineCompany = new AirlineCompany();

            HttpResponseMessage response = client.GetAsync("").Result;
            if (response.IsSuccessStatusCode)
            {
                User AirlineAPI = response.Content.ReadAsAsync<User>().Result;
                airlineCompany = new AirlineCompany(AirlineAPI.results[0].location.city, AirlineAPI.results[0].login.username, AirlineAPI.results[0].login.password, 0);
            }
            client.Dispose();
            return airlineCompany;
        }

        public Country CountryGenerator()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RANDOM_USER_URL"]);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Country country = new Country();

            HttpResponseMessage response = client.GetAsync("").Result;
            if (response.IsSuccessStatusCode)
            {
                User countryAPI = response.Content.ReadAsAsync<User>().Result;
                country = new Country(countryAPI.results[0].location.country);
            }
            client.Dispose();
            return country;
        }

        public Customer CustomerGenerator()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(ConfigurationManager.AppSettings["RANDOM_USER_URL"]);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Customer customer = new Customer();

            HttpResponseMessage response = client.GetAsync("").Result;
            if (response.IsSuccessStatusCode)
            {
                User CustomerAPI = response.Content.ReadAsAsync<User>().Result;
                customer = new Customer(CustomerAPI.results[0].name.first, CustomerAPI.results[0].name.last, CustomerAPI.results[0].login.username, CustomerAPI.results[0].login.password, $"{CustomerAPI.results[0].location.street.number.ToString()} {CustomerAPI.results[0].location.street.name}, {CustomerAPI.results[0].location.city}", Random.Next(10000, 99999), Random.Next(1000000, 9999999));
            }
            client.Dispose();
            return customer;
        }

        public IEnumerable<DateTime> DepartureDateGenerator(int numberOfDates)
        {
            var Random = new Random(Guid.NewGuid().GetHashCode());
            for (int index = 0; index < numberOfDates; index++)
            {
                var year = Random.Next(Int32.Parse(ConfigurationManager.AppSettings["FLIGHT_YEAR_RANGE_MIN"]), Int32.Parse(ConfigurationManager.AppSettings["FLIGHT_YEAR_RANGE_MAX"]));
                var month = Random.Next(1, 13);
                var days = Random.Next(1, DateTime.DaysInMonth(year, month) + 1);

                yield return new DateTime(year, month, days, Random.Next(0, 24), Random.Next(0, 60), Random.Next(0, 60));
            }
        }

        public IEnumerable<DateTime> LandingDateGenerator (List<DateTime> departureDates)
        {
            var Random = new Random(Guid.NewGuid().GetHashCode());
            for (int index = 0; index < departureDates.Count; index++)
            {
                yield return departureDates[index].AddHours(Random.Next(1, 14));
            }
        }

        public class User
        {
            public APIuser[] results;
        }

        public class APIuser
        {
            public Name name;
            public Login login;
            public Location location;
        }

        public class Name
        {
            public string first;
            public string last;
        }

        public class Login
        {
            public string username;
            public string password;
        }

        public class Location
        {
            public string city;
            public string country;
            public Street street;
        }

        public class Street
        {
            public int number;
            public string name;
        }
    }
}
