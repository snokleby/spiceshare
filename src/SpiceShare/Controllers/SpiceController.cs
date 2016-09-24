using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using SpiceShare.DataAccess;
using SpiceShare.ImageProcessing;
using SpiceShare.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SpiceShare.Models.Form;
using SpiceShare.Helpers;

namespace SpiceShare.Controllers
{
    public class SpiceController : Controller
    {
        private readonly IHostingEnvironment _hostingEvn;
        private readonly IMemoryCache _memoryCache;

        public SpiceController(IHostingEnvironment hostingEnvironment, IMemoryCache memCache)
        {
            _hostingEvn = hostingEnvironment;
            _memoryCache = memCache;
        }

        public IActionResult Index()
        {
            return RedirectToAction("", "");
        }
       
        public IActionResult SpicePage(int id)
        {
            using (var db = new SpiceContext())
            {
                var theSpice = db.Spices.Include(sp => sp.User).FirstOrDefault(m => m.SpiceId == id);
                if (theSpice == null)
                {
                    return View("expired");
                }
                return View(ViewModelFromSpiceModel(theSpice));
            }

        }

        public IActionResult TakeSpicePhoto(Guid spiceIdentity)
        {
            using (var db = new SpiceContext())
            {
                var theSpice = db.Spices.Include(sp => sp.User).FirstOrDefault(m => m.SpiceIdentity == spiceIdentity);
                if (theSpice == null)
                {
                    return View("expired");
                }
                return View(ViewModelFromSpiceModel(theSpice));
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TakeSpicePhoto(IFormFile file, Guid SpiceIdentity)
        {
            if (file == null)
            {
                using (var db = new SpiceContext())
                {
                    var theSpice = db.Spices.Include(spices => spices.User).FirstOrDefault(m => m.SpiceIdentity == SpiceIdentity);
                    if (theSpice == null)
                    {
                        return View("expired");
                    }
                    ModelState.AddModelError("Image", "Please select a photo in step 1 before uploading.");
                    return View("TakeSpicePhoto", ViewModelFromSpiceModel(theSpice));
                }
            }
           
            var stream = file.OpenReadStream();
            using (var db = new SpiceContext())
            {
                var theSpice =
                    db.Spices.Include(spices => spices.User).FirstOrDefault(m => m.SpiceIdentity == SpiceIdentity);
                if (theSpice == null)
                {
                    return View("expired");
                }
                var hitCounterKey = "uplhit" + HttpContext.Connection.RemoteIpAddress;
                int numHits;
                if (_memoryCache.TryGetValue<int>(hitCounterKey, out numHits))
                {
                    _memoryCache.Set(hitCounterKey, ++numHits, DateTimeOffset.Now.AddSeconds(60));
                    if (numHits > 3)
                    {
                        ModelState.AddModelError("Image", CognetiveServices.RateLimit);
                        return View("TakeSpicePhoto", ViewModelFromSpiceModel(theSpice));
                    }
                }
                else
                {
                    _memoryCache.Set(hitCounterKey,1, DateTimeOffset.Now.AddSeconds(60));
                }
                var extension = file.FileName.Split('.').Last();
                var thisImage = new MultiSizeImage("upload", extension);
                var subPath = ImageFileAccess.GetPath(theSpice.SpiceId.ToString(), Models.IdentityType.Spice);
                var da = new ImageFileAccess(_hostingEvn);
                var orignalUrl = ApiController.GetOriginalUrl(Request, theSpice, thisImage);

                try
                {
                    var savedSize = da.SaveImageFromBinaryStream(stream, subPath, thisImage, IdentityType.Spice);
                    if (savedSize > 4000000)
                    {
                        return TooLargeImage(da, subPath, theSpice);
                    }
                }
                catch (Exception) //supert dirty, should refactor.
                {
                    System.Threading.Thread.Sleep(new Random().Next(2000));
                    var savedSize = da.SaveImageFromBinaryStream(stream, subPath, thisImage, IdentityType.Spice);
                    if (savedSize > 4000000)
                    {
                        return TooLargeImage(da, subPath, theSpice);
                    }
                }
                theSpice.ImageFileName = thisImage.BaseFileName;
                theSpice.ImageFileExtension = thisImage.FileExtension;
                theSpice.ImgUploaded = DateTime.Now;
                db.SaveChanges();
            }
            return RedirectToAction("ProcessImage", new { SpiceIdentity = SpiceIdentity });
        }
        public async Task<IActionResult> ProcessImage(Guid SpiceIdentity)
        {
            var model = new ProcessImageViewModel();
            model.SpiceIdentity = SpiceIdentity;
            return View(model);
        }
        public async Task<IActionResult> _ProcessImage(Guid SpiceIdentity)
        {
            Task<string> descTask = null;
            Task<bool> safeTask = null;

            using (var db = new SpiceContext())
            {
                var theSpice =
                    db.Spices.Include(spices => spices.User).FirstOrDefault(m => m.SpiceIdentity == SpiceIdentity);
                if (theSpice == null)
                {
                    return View("expired");
                }
                var thisImage = new MultiSizeImage(theSpice.ImageFileName, theSpice.ImageFileExtension);
                var subPath = ImageFileAccess.GetPath(theSpice.SpiceId.ToString(), Models.IdentityType.Spice);
                var da = new ImageFileAccess(_hostingEvn);
                var orignalUrl = ApiController.GetOriginalUrl(Request, theSpice, thisImage);

                safeTask = CognetiveServices.IsSafeContent(orignalUrl);
                descTask = CognetiveServices.GetDescription(orignalUrl);
       
                try
                {
                    try
                    {
                        da.GenerateSizesOnDisk(subPath, thisImage, IdentityType.Spice);
                    }
                    catch (NotSupportedException ex)
                    {
                        return AddNotsupportedError(theSpice);
                    }
                }

                catch (Exception) //supert dirty, should refactor.
                {
                    try
                    {
                        da.GenerateSizesOnDisk(subPath, thisImage, IdentityType.Spice);
                    }
                    catch (NotSupportedException ex)
                    {
                        return AddNotsupportedError(theSpice);
                    }
                }
                Task.WaitAll(descTask, safeTask);
                if (!safeTask.Result)
                {
                    return DeleteImagesAndAddUnwantedContentError(da, subPath, theSpice);
                }
                theSpice.AltText = descTask.Result;
                db.SaveChanges();
            }
            return RedirectToAction("PrivateSpicePage", new { identity = SpiceIdentity });
        }

        private IActionResult AddNotsupportedError(Spice theSpice)
        {
            ModelState.AddModelError("Image", "Sorry, this image format is not supported. Try jpeg or png");
            return View("TakeSpicePhoto", ViewModelFromSpiceModel(theSpice));
        }

        private IActionResult DeleteImagesAndAddUnwantedContentError(ImageFileAccess da, string subPath, Spice theSpice)
        {
            ModelState.AddModelError("Image", CognetiveServices.UnwantedContent);
            da.TryDeleteImageFolder(subPath);

            return View("TakeSpicePhoto", ViewModelFromSpiceModel(theSpice));
        }

        private IActionResult TooLargeImage(ImageFileAccess da, string subPath, Spice theSpice)
        {
            ModelState.AddModelError("Image", "Sorry, this image was too big. We only support images smaller than 4MB.");
            da.TryDeleteImageFolder(subPath);

            return View("TakeSpicePhoto", ViewModelFromSpiceModel(theSpice));
        }

        public IActionResult PrivateSpicePage(Guid identity)
        {
            using (var db = new SpiceContext())
            {
                var theSpice = db.Spices.Include(sp => sp.User).FirstOrDefault(m => m.SpiceIdentity == identity);
                if (theSpice == null)
                {
                    return View("expired");
                }
                return View(ViewModelFromSpiceModel(theSpice));
            }          
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PrivateSpicePage(SpiceUserInputViewModel model)
        {
            using (var db = new SpiceContext())
            {
                var theSpice = db.Spices.Include(sp => sp.User).FirstOrDefault(m => m.SpiceIdentity == model.SpiceIdentity);
                if (theSpice == null)
                {
                    return RedirectToAction("Expired", "Home");
                }
                theSpice.Name = model.Name;
                theSpice.Info = model.Info;
                theSpice.AltText = model.AltText;
                db.SaveChanges();
                if (!ModelState.IsValid) {
                    return View(ViewModelFromSpiceModel(theSpice));
                }
                return RedirectToAction("PrivateSpicePage", new {identity = model.SpiceIdentity});
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult _PrivateSpicePageAjax(SpiceUserInputViewModel model)
        {
            using (var db = new SpiceContext())
            {
                var theSpice = db.Spices.Include(sp => sp.User).FirstOrDefault(m => m.SpiceIdentity == model.SpiceIdentity);
                if (theSpice == null)
                {
                    Response.StatusCode = 400;
                    return
                        Content(
                            "Sorry, it seems this site has been reset. This is a demo-site, and will be reset regularly. Try creating a new profile.");
                }
                var res = new FormResponse()
                {
                    Fields = new List<FieldValidation>(),
                };
                var tempVm = ViewModelFromSpiceModel(theSpice);

                foreach (var field in ModelState)
                {
                    var valid = field.Value.ValidationState == ModelValidationState.Valid;
                    if (field.Key.ToLower() == "alttext")
                    {
                        valid = tempVm.AltTextOk;
                    }
                    res.Fields.Add(new FieldValidation()
                    {
                        Id = field.Key.ToLower(),
                        Valid = valid
                    });
                }
                if (model.Name != null && model.Name != theSpice.Name)
                {
                    theSpice.Name = model.Name;
                    FormHelper.SetChanged(res, "name");

                }
                if (model.Info != theSpice.Info)
                {
                    theSpice.Info = model.Info;
                    FormHelper.SetChanged(res, "info");

                }
                if (model.AltText != null && model.AltText != theSpice.AltText)
                {
                    theSpice.AltText = model.AltText;                
                    FormHelper.SetChanged(res, "alttext");

                }
                db.SaveChanges();
                var vm = ViewModelFromSpiceModel(theSpice);
              
              
                if (vm.IsComplete)
                {
                    res.Status = FormResponse.FormComplete;
                } else if (vm.IsValid)
                {
                    res.Status = FormResponse.FormValid;
                }
                else
                {
                    res.Status = FormResponse.FormInvalid;
                }
               
                return Json(res);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult New(Guid userIdentity)
        {
            using (var db = new SpiceContext())
            {
                var user = db.Users.First(m => m.UserIdentity == userIdentity);
                var theNewSpice = db.Spices.Add(new Spice() { SpiceIdentity = Guid.NewGuid(), UserId = user.UserId, User = user});
                db.SaveChanges();
                return RedirectToAction("PrivateSpicePage", new {identity = theNewSpice.Entity.SpiceIdentity});
            }
        }


        public static SpiceViewModel ViewModelFromSpiceModel(Spice theSpice)
        {
            return new SpiceViewModel() {
                Image = new MultiSizeImage(theSpice.ImageFileName, theSpice.ImageFileExtension) {
                    DirectoryName = ImageFileAccess.GetWebPath(theSpice.SpiceId.ToString(), 
                    theSpice.IsStock? IdentityType.StockSpice : IdentityType.Spice),
                    AlternativeDescription = theSpice.AltText,
                    CacheBuster = (long)(theSpice.ImgUploaded - new DateTime(2016,09,1)).TotalSeconds },
                Name = theSpice.Name,
                Info = theSpice.Info,
                UserIdentity = theSpice.User.UserIdentity,
                SpiceId = theSpice.SpiceId,
                SpiceIdentity = theSpice.SpiceIdentity,
            UserId = theSpice.UserId,
                AltText = theSpice.AltText,
            UserName = theSpice.User.Name};
        }

    }
}
