using System;
using System.Collections.Generic;

namespace TestApplication.Helpers
{
    internal static class MockDataFill
    {
        private static readonly Random random = new Random();

        private static readonly string[] mockNames = new string[20]
        {
            "Bjorn",
            "Naeva",
            "Cinema",
            "Leane",
            "Wa",
            "Melia",
            "Nehémie",
            "Mediamas",
            "Celestin",
            "Andre",
            "Marta",
            "Marie-ev",
            "Men",
            "Therese",
            "Oceanne",
            "Rejane",
            "Ake",
            "Maely",
            "Maily",
            "Akes"
        };
        private static string GenerateRandomString(int min, int max)
        {
            int randomNumber = random.Next(min, max);
            string randomString = randomNumber.ToString("D2");
            return randomString;
        }
        private static string GetRandomName(string[] names)
        {
            string name =  names[random.Next(names.Length)];
            return name.Length <= 5 ? name : name.Substring(0, 5);
            
        }
        private static long GenerateRandomLongID()
        {
            byte[] buffer = new byte[8];
            random.NextBytes(buffer);
            long longID = BitConverter.ToInt64(buffer, 0) & 0x7FFFFFFFFFFFFFFF;
            return longID;
        }
        private static (DateTime, DateTime) GenerateRandomDateTimes()
        {
            DateTime firstDateTime = new DateTime(2000, 1, 1).AddDays(random.Next(365 * 20)).AddHours(new Random().Next(24)).AddMinutes(new Random().Next(60)).AddSeconds(new Random().Next(60));

            DateTime secondDateTime = firstDateTime.AddDays(random.Next(1, 10)).AddHours(new Random().Next(24)).AddMinutes(new Random().Next(60)).AddSeconds(new Random().Next(60));

            return (firstDateTime, secondDateTime);
        }
        private static byte[] GenerateTimeStamp()
        {
            byte[] date = BitConverter.GetBytes(DateTime.Now.Ticks);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(date);
            return date;
        }
        internal static List<InventLocation> GetInventLocations(int quantity)
        {
            List<InventLocation> locations = new List<InventLocation>();
            for (int i = 1; i <= quantity; i++)
            {
                var datetime = GenerateRandomDateTimes();
                InventLocation location = new InventLocation
                {
                    InventLocationId = i.ToString("D2"),
                    InventSiteId = GenerateRandomString(50, 126),
                    Name = GetRandomName(mockNames),
                    RecordId = GenerateRandomLongID(),
                    CreateDateTime = datetime.Item1,
                    UpdateDateTime = datetime.Item2,
                    CreatedBy = GetRandomName(mockNames),
                    ModifiedBy = GetRandomName(mockNames),
                    RowVersion = GenerateTimeStamp()
                };
                locations.Add(location);
            }
            return locations;
        }
        internal static List<InventDim> GetInventDims(List<InventLocation> inventLocations)
        {
            List<InventDim> inventDims = new List<InventDim>();
            int fist_index_num = 0;
            foreach (var inventLocation in inventLocations)
            {
                for (int i = 1; i <=1000; i++)
                {
                    InventDim inventDim = new InventDim
                    {
                        InventDimId = $"{fist_index_num}" + i.ToString("D3"),
                        InventSerialId = Guid.NewGuid().ToString().Substring(0, 10),
                        InventLocationId = inventLocation.InventLocationId,
                        WMSLocationId = GenerateRandomString(1, 155),
                        InventBatchId = Guid.NewGuid().ToString().Substring(0, 10),
                        WMSPalletId = Guid.NewGuid().ToString().Substring(0, 18),
                        InventColorId = Guid.NewGuid().ToString().Substring(0, 10),
                        InventSiteId = Guid.NewGuid().ToString().Substring(0, 10),
                        InventSizeId = Guid.NewGuid().ToString().Substring(0, 10),
                        RecordId = GenerateRandomLongID(),
                        RowVersion = GenerateTimeStamp(),
                        CreatedBy = GetRandomName(mockNames),
                        ModifiedBy = GetRandomName(mockNames)

                    };
                    inventDims.Add(inventDim);
                    
                }
                fist_index_num++;
            }
            return inventDims;
        }
         
    }
    
}
