using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;

namespace AzureSearch
{
    class Program
    {
        private static IConfigurationRoot _config;
        private static SearchServiceClient searchServiceClient;
        private static ISearchIndexClient searchIndexClient;

        static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            _config = builder.Build();

            string indexName = _config["SearchIndexName"];
            searchServiceClient = CreateSearchServiceClient();

            Console.WriteLine($"Deleting the existing index.. : {indexName}");
            DeleteIndexIfExists(indexName, searchServiceClient);

            Console.WriteLine($"Creating the index : {indexName}");
            CreateIndexs(indexName, searchServiceClient);

            Console.WriteLine($"Uploading the documents ...");
            searchIndexClient = searchServiceClient.Indexes.GetClient(indexName);
            UploadDocuments(searchIndexClient);

            RunQueries(searchIndexClient);

            Console.ReadLine();
        }

        private static SearchServiceClient CreateSearchServiceClient()
        {
            string serviceName = _config["SearchServiceName"];
            string serviceAdminKey = _config["SearchServiceAdminApiKey"];

            SearchServiceClient searchServiceClient = new SearchServiceClient(serviceName, new SearchCredentials(serviceAdminKey));
            return searchServiceClient;
        }

        private static void DeleteIndexIfExists(string indexName, SearchServiceClient searchServiceClient)
        {
            if (searchServiceClient.Indexes.Exists(indexName))
            {
                searchServiceClient.Indexes.Delete(indexName);
            }
        }

        private static void CreateIndexs(string indexName, SearchServiceClient searchServiceClient)
        {
            var definition = new Microsoft.Azure.Search.Models.Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<Hotel>()
            };
            var res = searchServiceClient.Indexes.Create(definition);
        }

        private static void UploadDocuments(ISearchIndexClient searchIndexClient)
        {
            var actions = new IndexAction<Hotel>[]
            {
               IndexAction.Upload(new Hotel
               {
                HotelId = "1",
                HotelName = "Secret Point Motel",
                Description = "The hotel is ideally located on the main commercial artery of the city in the heart of New York. A few minutes away is Time's Square and the historic centre of the city, as well as other places of interest that make New York one of America's most attractive and cosmopolitan cities.",
                DescriptionFr = "L'hôtel est idéalement situé sur la principale artère commerciale de la ville en plein cœur de New York. A quelques minutes se trouve la place du temps et le centre historique de la ville, ainsi que d'autres lieux d'intérêt qui font de New York l'une des villes les plus attractives et cosmopolites de l'Amérique.",
                Category = "Boutique",
                Tags = new[] { "pool", "air conditioning", "concierge" },
                ParkingIncluded = false,
                LastRenovationDate = new DateTimeOffset(1970, 1, 18, 0, 0, 0, TimeSpan.Zero),
                Rating = 3.6,
                Address = new Address()
                {
                    StreetAddress = "677 5th Ave",
                    City = "New York",
                    StateProvince = "NY",
                    PostalCode = "10022",
                    Country = "USA"
                }
               }),
                IndexAction.Upload(
                    new Hotel()
                    {
                        HotelId = "2",
                        HotelName = "Twin Dome Motel",
                        Description = "The hotel is situated in a  nineteenth century plaza, which has been expanded and renovated to the highest architectural standards to create a modern, functional and first-class hotel in which art and unique historical elements coexist with the most modern comforts.",
                        DescriptionFr = "L'hôtel est situé dans une place du XIXe siècle, qui a été agrandie et rénovée aux plus hautes normes architecturales pour créer un hôtel moderne, fonctionnel et de première classe dans lequel l'art et les éléments historiques uniques coexistent avec le confort le plus moderne.",
                        Category = "Boutique",
                        Tags = new[] { "pool", "free wifi", "concierge" },
                        ParkingIncluded = false,
                        LastRenovationDate =  new DateTimeOffset(1979, 2, 18, 0, 0, 0, TimeSpan.Zero),
                        Rating = 3.60,
                        Address = new Address()
                        {
                            StreetAddress = "140 University Town Center Dr",
                            City = "Sarasota",
                            StateProvince = "FL",
                            PostalCode = "34243",
                            Country = "USA"
                        }
                    }
            ),
        IndexAction.Upload(
            new Hotel()
            {
                HotelId = "3",
                HotelName = "Triple Landscape Hotel",
                Description = "The Hotel stands out for its gastronomic excellence under the management of William Dough, who advises on and oversees all of the Hotel’s restaurant services.",
                DescriptionFr = "L'hôtel est situé dans une place du XIXe siècle, qui a été agrandie et rénovée aux plus hautes normes architecturales pour créer un hôtel moderne, fonctionnel et de première classe dans lequel l'art et les éléments historiques uniques coexistent avec le confort le plus moderne.",
                Category = "Resort and Spa",
                Tags = new[] { "air conditioning", "bar", "continental breakfast" },
                ParkingIncluded = true,
                LastRenovationDate = new DateTimeOffset(2015, 9, 20, 0, 0, 0, TimeSpan.Zero),
                Rating = 4.80,
                Address = new Address()
                {
                    StreetAddress = "3393 Peachtree Rd",
                    City = "Atlanta",
                    StateProvince = "GA",
                    PostalCode = "30326",
                    Country = "USA"
                }
            }
        ),
        IndexAction.Upload(
            new Hotel()
            {
                HotelId = "4",
                HotelName = "Sublime Cliff Hotel",
                Description = "Sublime Cliff Hotel is located in the heart of the historic center of Sublime in an extremely vibrant and lively area within short walking distance to the sites and landmarks of the city and is surrounded by the extraordinary beauty of churches, buildings, shops and monuments. Sublime Cliff is part of a lovingly restored 1800 palace.",
                DescriptionFr = "Le sublime Cliff Hotel est situé au coeur du centre historique de sublime dans un quartier extrêmement animé et vivant, à courte distance de marche des sites et monuments de la ville et est entouré par l'extraordinaire beauté des églises, des bâtiments, des commerces et Monuments. Sublime Cliff fait partie d'un Palace 1800 restauré avec amour.",
                Category = "Boutique",
                Tags = new[] { "concierge", "view", "24-hour front desk service" },
                ParkingIncluded = true,
                LastRenovationDate = new DateTimeOffset(1960, 2, 06, 0, 0, 0, TimeSpan.Zero),
                Rating = 4.6,
                Address = new Address()
                {
                    StreetAddress = "7400 San Pedro Ave",
                    City = "San Antonio",
                    StateProvince = "TX",
                    PostalCode = "78216",
                    Country = "USA"
                }
            }
        ),
            };


            var batch = IndexBatch.New(actions);

            try
            {
                searchIndexClient.Documents.Index(batch);
            }
            catch (IndexBatchException e)
            {
                Console.WriteLine("Failed to index some of the documents: {0}", String.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
                throw;
            }

            // Wait 2 seconds before starting queries 
            Console.WriteLine("Waiting for indexing...\n");
            Thread.Sleep(2000);
        }

        private static void WriteDocuments(DocumentSearchResult<Hotel> documentSearchResult)
        {
            foreach (SearchResult<Hotel> item in documentSearchResult.Results)
            {
                Console.WriteLine(JsonConvert.SerializeObject(item.Document));
            }
            Console.WriteLine("---------------------");
        }

        private static void RunQueries(ISearchIndexClient searchIndexClient)
        {
            SearchParameters searchParameters;
            DocumentSearchResult<Hotel> results;

            // Query 1 
            Console.WriteLine("Query 1: Search for term 'Atlanta' with no result trimming");
            searchParameters = new SearchParameters();
            results = searchIndexClient.Documents.Search<Hotel>("Atlanta", searchParameters);

            WriteDocuments(results);


            // Query 2
            Console.WriteLine("Query 2: Search on the term 'Atlanta', with trimming");
            Console.WriteLine("Returning only these fields: HotelName, Tags, Address:\n");
            searchParameters =
                new SearchParameters()
                {
                    Select = new[] { "HotelName", "Tags", "Address" },
                };
            results = searchIndexClient.Documents.Search<Hotel>("Atlanta", searchParameters);
            WriteDocuments(results);

            //Query 3
            Console.WriteLine($"Search for the term 'restaurant' and 'wifi' ");
            Console.WriteLine("Return only these fields: HotelName, Description, and Tags:\n");
            searchParameters = new SearchParameters
            {
                Select = new[] { "HotelName", "Description", "Tags" }
            };
            results = searchIndexClient.Documents.Search<Hotel>("restaurant,wifi", searchParameters);
            WriteDocuments(results);

            // Query 4

            Console.WriteLine($"Filtered rating greater than 4");
            Console.WriteLine("Returning only these fields: HotelName, Rating:\n");

            searchParameters = new SearchParameters()
            {
                Filter = "Rating gt 4",
                Select = new[] { "HotelName", "Rating" }
            };
            results = searchIndexClient.Documents.Search<Hotel>("*", searchParameters);
            WriteDocuments(results);

            // Query 5 - top 2 results
            Console.WriteLine("Query 5: Search on term 'boutique'");
            Console.WriteLine("Sort by rating in descending order, taking the top two results");
            Console.WriteLine("Returning only these fields: HotelId, HotelName, Category, Rating:\n");
            searchParameters = new SearchParameters
            {
                OrderBy = new[] { "Rating desc" },
                Select = new[] { "HotelId", "HotelName", "Category", "Rating" },
                Top = 2
            };
            results = searchIndexClient.Documents.Search<Hotel>("boutique", searchParameters);
            WriteDocuments(results);

        }
    }
}
