using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using SpiceShare.DataAccess;
using SpiceShare.ImageProcessing;
using SpiceShare.Models;

namespace SpiceShare.Helpers
{
    public class GenerateStockContent
    {
        public static void CreateFresh(SpiceContext db, IHostingEnvironment hostingEnv, IMemoryCache memCache)
        {
            db.Database.EnsureDeleted();
            if (db.Database.EnsureCreated())
            {
                db.ManadagementData.Add(new ManagementData()
                {
                    NextPrune = DateTime.Now.AddMinutes(60)
                });
                db.SaveChanges();
                var generator = new GenerateStockContent();
                generator.Generate(db, hostingEnv);
                var cda = new CachedDataAccess(memCache);
                cda.GetLatestAdded(hostingEnv, true);
            }
        }

        private void Generate(SpiceContext db, IHostingEnvironment hostingEnvironment)
        {
            var uploadFolder = Path.Combine(hostingEnvironment.WebRootPath, ImageFileAccess.GetUploadsFolder(IdentityType.Spice));
            try
            {
                Directory.Delete(uploadFolder, true);
            }
            catch (Exception)
            {
                
            }
            try
            {
                Directory.CreateDirectory(uploadFolder);
            }
            catch (Exception)
            {
                
            }
            var u = new User()
            {
                Name = "Maria Monroe",
                Email = "maria@example.com",
                UserIdentity = Guid.NewGuid(),
                Location = new GeoLocation()
                {
                    Country = "United states (USA)",
                    Lat = 34.1030032,
                    Lon = -118.41046840000001,
                    ZipCode = "90210"
                },
                Spices = new List<Spice>()
                {
                    AddSpice("Cinnamon", "A picture of cinnamon", "I inherited a container of this stuff. Will give it away for free."),
                    AddSpice("Pepper", "A picture of pepper", "I buy this in bulk at a discount, and have a lot more than I can use.")
                },

            };
            db.Users.Add(u);
            var u2 = new User()
            {
                Name = "Wulf Basajaun",
                Email = "ben@example.com",
                UserIdentity = Guid.NewGuid(),
                Location = new GeoLocation()
                {
                    Country = "Cyprus",
                    Lat = 35.007503,
                    Lon = 34.019165,
                    ZipCode = "5330"
                },
                Spices = new List<Spice>()
                {
                    AddSpice("Red curry", "Two glasses of curry, one of them half empty", "I bought this for Taco, but only ate half of it. Will give it away for free."),
                 },

            };
            db.Users.Add(u2);
            var u3 = new User()
            {
                Name = "Selwyn Millard",
                Email = "selwyn.millard@example.com",
                UserIdentity = Guid.NewGuid(),
                Location = new GeoLocation()
                {
                    Country = "United kingdom (UK)",
                    Lat = 51.5073509,
                    Lon = -0.12775829999998223,
                    ZipCode = "WC2"
                },
                Spices = new List<Spice>()
                {
                     AddSpice("Rosemary", "A rosemary", "I grow this in my back-garden, and produce more than I can use. Can share in exhange for some other, more exotic, spices."),
                    AddSpice("Basil", "A jar of pesto", "Ok, so this is not really basil. I made it into pesto, and now I regret it. I can be yours for a fiver."),
                 },

            };
            db.Users.Add(u3);
            var u4 = new User()
            {
                Name = "Albena Cheyenne",
                Email = "albena@example.com",
                UserIdentity = Guid.NewGuid(),
                Location = new GeoLocation()
                {
                    Country = "United states (USA)",
                    Lat = 40.7127837,
                    Lon = -74.00594130000002,
                    ZipCode = "10007"
                },
                Spices = new List<Spice>()
                {
                     AddSpice("Red pepper", "Stock photo of a red pepper", "Interested in swapping with some green peppers."),                
                     AddSpice("Green chili", "Stock photo of a green chili", "I buy this every week, but only use half for pizza."),
                 },

            };
            db.Users.Add(u4);
            db.SaveChanges();
        }

        private static Spice AddSpice(string name, string altText, string info)
        {
            return new Spice()
            {
                Name = name,
                AltText = altText,
                Info = info,
                ImgUploaded = DateTime.Now,
                ImageFileName = "upload",
                ImageFileExtension = "jpg",
                IsStock = true,
                SpiceIdentity = Guid.NewGuid()
            };
        }
    }
}
