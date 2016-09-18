using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using SpiceShare.DataAccess;
using SpiceShare.ImageProcessing;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Net.Http.Headers;
using SpiceShare.Helpers;
using SpiceShare.Models;

namespace SpiceShare.Controllers
{
    public class ApiController : Controller
    {
        private readonly IHostingEnvironment _hostingEvn;
        private readonly IMemoryCache _memoryCache;

        public ApiController(IHostingEnvironment hostingEnvironment, IMemoryCache memCache)
        {
            _hostingEvn = hostingEnvironment;
            _memoryCache = memCache;
        }

        public IActionResult UsersNearMe(double lat, double lon, string search)
        {
            using (var db = new SpiceContext())
            {
                var locationList = SearchHelper.FindUsersByLatLon(lat, lon, db);
                var usersWithSpice = SearchHelper.GetUsersWithSpiceMatch(locationList, search);
                return View("_usersnear", usersWithSpice.OrderBy(sort => sort.Distance).ToList());
            }
        }


        public IActionResult UsersNearZip(string country, string zipcode, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                var locationList = SearchHelper.FindUsersByCountryAndZip(country, zipcode);
                var usersWithSpice = SearchHelper.GetUsersWithSpiceMatch(locationList, search);
                return View("_usersnear", usersWithSpice.OrderBy(sort => sort.Distance).ToList());
            }
            return View("_usersnear", new List<SpiceShare.Models.LocationBasedUser>());
        }


        [HttpGet]
        [Route("api/distanceToUser")]
        public string DistanceToUser(int id, double lat, double lon)
        {
            using (var db = new SpiceContext())
            {
                var theUser = db.Users.Include(m => m.Location).FirstOrDefault(m => m.UserId == id);
                if (theUser == null)
                {
                    throw new ArgumentException("invalid userId");
                }
                //do not give the users exact location away.
                var dist = SearchHelper.GetAproxDistance(lat, lon, theUser);
                return GetRoundedDist(dist);
            }
        }

        public static string GetRoundedDist(double dist)
        {
            if (dist > 1000)
            {
                return Math.Round(dist / 1000, 1) + " km";
            }
            else if (dist > 0)
            {
                return dist + " meters";
            }
            return "unkown";
        }

        [HttpGet]
        [Route("api/prune")]
        public IActionResult Prune()
        {
            try
            {
                using (var db = new SpiceContext())
                {
                    var mgdData = db.ManadagementData.FirstOrDefault();
                    if (mgdData == null) {
                        return View("_resetInfo");
                    }
                    if ((mgdData.NextPrune - DateTime.Now).TotalMinutes > 0)
                    {
                        return View("_resetInfo", (object)(mgdData.NextPrune - DateTime.Now));
                    }
                }
            }

            catch (Exception)
            {
                using (var db = new SpiceContext())
                {
                    GenerateStockContent.CreateFresh(db, _hostingEvn, _memoryCache);
                    return View("_resetInfo", (object)(new TimeSpan(0,0,-1)));
                }
            }

            using (var db = new SpiceContext())
            {
                GenerateStockContent.CreateFresh(db, _hostingEvn, _memoryCache);
            }
            return View("_resetInfo", (object)(new TimeSpan(0, 0, -1)));
        }

        // POST api/values
        [HttpPost]
        [Route("api/newSpiceImage")]
        public async Task<string> NewSpiceImage(Guid identity)
        {
            var hitCounterKey = "uplhit" + HttpContext.Connection.RemoteIpAddress;
            int numHits;
            if (_memoryCache.TryGetValue<int>(hitCounterKey, out numHits))
            {
                _memoryCache.Set( hitCounterKey, ++numHits, DateTimeOffset.Now.AddSeconds(60));
                if (numHits > 3)
                {
                    Response.StatusCode = 400;
                    return CognetiveServices.RateLimit;
                }
            }
            else
            {
                _memoryCache.Set(hitCounterKey, 1, DateTimeOffset.Now.AddSeconds(60));
            }

            var l = await new StreamReader(Request.Body).ReadToEndAsync();
            using (var db = new SpiceContext())
            {
                var theSpice = db.Spices.FirstOrDefault(m => m.SpiceIdentity == identity);
                if (theSpice == null)
                {
                    return "Opps. Seems like this spice has expired. This demo-site is reset regularly. Try creating a new user.";
                }
                var thisImage = new MultiSizeImage("upload", "jpg");
                var subPath = ImageFileAccess.GetPath(theSpice.SpiceId.ToString(), Models.IdentityType.Spice);
                var da = new ImageFileAccess(_hostingEvn);
                Task<string> descTask = null;
                var orignalUrl = GetOriginalUrl(Request,theSpice, thisImage);
             
                try
                {
                    da.SaveImageFromBase64Stream(l, subPath, thisImage, IdentityType.Spice);
                    var safe = await CognetiveServices.IsSafeContent(orignalUrl);
                    if (!safe)
                    {
                        da.TryDeleteImageFolder(subPath);
                        Response.StatusCode = 400;
                        return CognetiveServices.UnwantedContent;
                    }
                    descTask = CognetiveServices.GetDescription(orignalUrl);
                    da.GenerateSizesOnDisk(subPath, thisImage, IdentityType.Spice);
                } catch(Exception ex)
                {
                    System.Threading.Thread.Sleep(new Random().Next(2000));
                    da.SaveImageFromBase64Stream(l, subPath, thisImage, IdentityType.Spice);
                    var safe = await CognetiveServices.IsSafeContent(orignalUrl);
                    if (!safe)
                    {
                        da.TryDeleteImageFolder(subPath);

                        Response.StatusCode = 400;
                        return CognetiveServices.UnwantedContent;
                    }                   
                    if (descTask == null)
                    {
                        descTask = CognetiveServices.GetDescription(orignalUrl);
                    }
                    da.GenerateSizesOnDisk(subPath, thisImage, IdentityType.Spice);
                }
                theSpice.ImageFileName = "upload";
                theSpice.ImageFileExtension = "jpg";
                theSpice.ImgUploaded = DateTime.Now;
                if (descTask != null) {
                    Task.WaitAll(descTask);
                    theSpice.AltText = descTask.Result;
                }
                db.SaveChanges();
            }
            return "ok";
        }

      

        public static string GetOriginalUrl(HttpRequest request, Spice theSpice, MultiSizeImage thisImage)
        {
            var orignalUrl = request.Scheme + "://" + request.Host +
                             ImageFileAccess.GetWebPath(theSpice.SpiceId.ToString(), IdentityType.Spice) + "/" +
                             thisImage.OriginalFileName + "?v=" + new Random().Next(1000000);
            if (request.Host.ToString().Contains("localhost"))
            {
                orignalUrl = "https://spiceshare.azurewebsites.net/Stock/spiceImage/1/upload_Huge.jpg?v=35";
            }
            return orignalUrl;
        }
    }
}
