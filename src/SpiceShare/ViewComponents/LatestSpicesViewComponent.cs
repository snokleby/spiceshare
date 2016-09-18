using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SpiceShare.DataAccess;
using SpiceShare.Controllers;
using SpiceShare.Helpers;
using SpiceShare.Models;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace SpiceShare.ViewComponents
{
    public class LatestSpicesViewComponent : ViewComponent
    {
        private readonly IHostingEnvironment _hostingEnv;
        private readonly IMemoryCache _memoryCache;

        public LatestSpicesViewComponent(IHostingEnvironment hostingEnvironment, IMemoryCache memoryCache)
        {
            _hostingEnv = hostingEnvironment;
            _memoryCache = memoryCache;
        }

        public IViewComponentResult Invoke()
        {
            List<Spice> latestSpices  = new CachedDataAccess(_memoryCache).GetLatestAdded(_hostingEnv);           
            return View(new FrontPageViewModel() { LatestSpices = latestSpices });
        }
    }
}
