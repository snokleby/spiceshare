using System;
using Microsoft.AspNetCore.Mvc;
using SpiceShare.DataAccess;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SpiceShare.Models;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SpiceShare.Models.Form;
using SpiceShare.Helpers;
using SpiceShare.Model;

namespace SpiceShare.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("", "");
        }

        public IActionResult NearMe()
        {
            return View(new NearMeViewModel());
        }

        public IActionResult NearMeZip(string country, string zipcode, string search)
        {
            if (!string.IsNullOrEmpty(search))
            {
                var res = SearchHelper.FindUsersByCountryAndZip(country, zipcode);
                return View(new NearMeViewModel() { Users = SearchHelper.GetUsersWithSpiceMatch(res, search), Country = country, Zipcode = zipcode, Search = search });

            }
            return View(new NearMeViewModel());
        }

        public IActionResult NearMeGeo(string lat, string lon, string search)
        {                  
            if (!string.IsNullOrEmpty(lat) && !string.IsNullOrEmpty(lon))
            {
                using (var db = new SpiceContext())
                {
                    var res = SearchHelper.FindUsersByLatLon(DoubleFromString(lat).Value, DoubleFromString(lon).Value,
                        db);
                    return View(new NearMeViewModel() { Users = SearchHelper.GetUsersWithSpiceMatch(res, search), Lat = lat, Lon = lon, Search = search });
                }
            }
           // ModelState.AddModelError("generror", "You must specify either a location using latitude/lonitude, or a country and zip-code");
            return View(new NearMeViewModel() );
        }
     
        public IActionResult UserPage(int id, double? lat = null, double? lon = null)
        {
            using (var db = new SpiceContext())
            {
                var theUser = db.Users.Include(users => users.Spices).Include(m => m.Location).FirstOrDefault(m => m.UserId == id);
                if (theUser == null)
                {
                    return View("expired");
                }
                var model = new UserPageViewModel()
                {
                    User = theUser,
                    Id = id
                };
                if (lat != null && lon != null)
                {
                    var dist = SearchHelper.GetAproxDistance(lat.Value, lon.Value, theUser);
                    model.Distance = ApiController.GetRoundedDist(dist);
                    model.Lat = lat.Value;
                    model.Lon = lon.Value;
                }
                return View(model);
            }

        }

        public IActionResult PrivateUserPage(Guid identity)
        {
            using (var db = new SpiceContext())
            {
                var theUser = GetUser(identity, db);
                if (theUser == null)
                {
                    return View("expired");
                }
                SetBaseUrlInViewData();
                var model = new UserViewModel()
                {
                    Lat = theUser.Location?.Lat?.ToString(CultureInfo.InvariantCulture),
                    Lon = theUser.Location?.Lon?.ToString(CultureInfo.InvariantCulture),
                    ZipCode = theUser.Location?.ZipCode,
                    Country = theUser.Location?.Country,
                    Name = theUser.Name,
                    Email = theUser.Email,
                    UserIdentity = theUser.UserIdentity,
                    Spices = new List<SpiceViewModel>()
                };

                SetReadOnlyUserData(model, theUser);
                return View(model);
            }           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PrivateUserPage(UserInputViewModel model)
        {          
            using (var db = new SpiceContext())
            {
               
                SetBaseUrlInViewData();

                var theUser = GetUser(model.UserIdentity, db);
                if (!ModelState.IsValid)
                {
                    var viewModel = new UserViewModel
                    {
                        UserIdentity = model.UserIdentity,
                        Country = model.Country,
                        Email = model.Email,
                        Lat = model.Lat,
                        Lon = model.Lon,
                        Name = model.Name,
                        ZipCode = model.ZipCode,
                        Spices = new List<SpiceViewModel>()
                    };
                    SetReadOnlyUserData(viewModel, theUser);

                    return View(viewModel);
                }
                if (theUser == null)
                {
                    return RedirectToAction("Expired", "Home");
                }
                SaveViewModelToDataModel(model, theUser);
                db.SaveChanges();
                return RedirectToAction("PrivateUserPage", new {identity = theUser.UserIdentity});                
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult _PrivateUserPageAjax(UserInputViewModel model)
        {
            using (var db = new SpiceContext())
            {            
                var theUser = GetUser(model.UserIdentity, db);               
                if (theUser == null)
                {
                    Response.StatusCode = 400;
                    return
                        Content(
                            "Sorry, it seems this site has been reset. This is a demo-site, and will be reset regularly. Try creating a new profile.");
                }
                var res = new FormResponse
                {
                    Fields = new List<FieldValidation>(),
                    Status = FormResponse.FormInvalid
                };
               
                foreach (var field in ModelState)
                {
                    res.Fields.Add(new FieldValidation()
                    {
                        Id = field.Key.ToLower(),
                        Valid = field.Value.ValidationState == ModelValidationState.Valid
                    });
                }

                if (model.Name != theUser.Name)
                {
                    FormHelper.SetChanged(res, "name");
                
                }
                if (model.Country != theUser.Location.Country)
                {
                    FormHelper.SetChanged(res, "country");
                }
                if (model.ZipCode != theUser.Location.ZipCode)
                {
                    FormHelper.SetChanged(res, "zipcode");
                }
                if (model.Email != theUser.Email)
                {
                    FormHelper.SetChanged(res, "email");
                }
                if (DoubleFromString(model.Lat) != theUser.Location.Lat)
                {
                    FormHelper.SetChanged(res, "lat");
                }
                if (DoubleFromString(model.Lon) != theUser.Location.Lon)
                {
                    FormHelper.SetChanged(res, "lon");
                }

                SaveViewModelToDataModel(model, theUser);
                db.SaveChanges();

                if (ModelState.IsValid)
                {
                    res.Status = FormResponse.FormValid;
                    if (theUser.Location.Lat.HasValue && theUser.Location.Lon.HasValue)
                    {
                        res.Status = FormResponse.FormComplete;
                    }
                }

                return Json(res);
            }
        }

        public IActionResult NewUser()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult New()
        {
            using (var db = new SpiceContext())
            {
                db.Database.EnsureCreated();
            }

            using (var db = new SpiceContext())
            {

                var theNewUser = db.Users.Add(new User() { UserIdentity = Guid.NewGuid(), Location = new GeoLocation()});
                db.SaveChanges();
                return RedirectToAction("PrivateUserPage", new {identity = theNewUser.Entity.UserIdentity});
            }
        }

        private void SaveViewModelToDataModel(UserInputViewModel model, User theUser)
        {
            if (!string.IsNullOrEmpty(model.Name))
                theUser.Name = model.Name;
            if (!string.IsNullOrEmpty(model.Email))
                theUser.Email = model.Email;
            theUser.Location.Lat = DoubleFromString(model.Lat);
            theUser.Location.Lon = DoubleFromString(model.Lon);
            if (!string.IsNullOrEmpty(model.Country))
                theUser.Location.Country = model.Country;
            if (!string.IsNullOrEmpty(model.ZipCode))
                theUser.Location.ZipCode = model.ZipCode;
        }

        private double? DoubleFromString(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return null;
            }
            val = val.Replace(",", ".").Replace(" ", "");
            return double.Parse(val, CultureInfo.InvariantCulture);
        }

        private static void SetReadOnlyUserData(UserViewModel model, User theUser)
        {
            model.Id = theUser.UserId;
            foreach (var spice in theUser.Spices)
            {
                model.Spices.Add(SpiceController.ViewModelFromSpiceModel(spice));
            }
        }

        private void SetBaseUrlInViewData()
        {
            var builder = new UriBuilder
            {
                Scheme = Request.Scheme,
                Host = Request.Host.Host
            };

            ViewData["baseurl"] = builder.Uri.ToString();
        }

        private static User GetUser(Guid identity, SpiceContext db)
        {
            return db.Users.Include(users => users.Spices).Include(users => users.Location).FirstOrDefault(m => m.UserIdentity == identity);
        }

    }
}
