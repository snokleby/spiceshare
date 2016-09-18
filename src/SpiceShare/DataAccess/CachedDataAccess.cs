using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using SpiceShare.Controllers;
using SpiceShare.Helpers;

namespace SpiceShare.DataAccess
{
    public class CachedDataAccess
    {
        private readonly IMemoryCache _memoryCache;

        public CachedDataAccess(IMemoryCache memCache)
        {
            _memoryCache = memCache;
        }

        public List<Spice> GetLatestAdded(IHostingEnvironment hostinvEnv, bool forceReload = false)
        {
            List<Spice> latestSpices = null;
           // latestSpices = _memoryCache.Get<List<Spice>>(CacheKeys.LatestSpiceList);
            if (latestSpices != null)
            {
                return latestSpices;                
            }

                try
                {
                    using (var db = new SpiceContext())
                    {
                        latestSpices =
                            db.Spices.OrderByDescending(m => m.SpiceId)
                                .Where(m => !string.IsNullOrEmpty(m.Name)).Take(10)
                                .ToList();
                        if (!latestSpices.Any())
                        {
                            throw new InvalidDataException();
                        }
                    _memoryCache.Set(CacheKeys.LatestSpiceList, latestSpices);
                }
            }
                catch (Exception)
                {

                    using (var db = new SpiceContext())
                    {               
                        GenerateStockContent.CreateFresh(db, hostinvEnv, _memoryCache);
                        latestSpices =
                            db.Spices.OrderByDescending(m => m.SpiceId)
                                .Where(m => !string.IsNullOrEmpty(m.Name)).Take(10)
                                .ToList();
                    }
                }
            return latestSpices;
        } 
    }
}
