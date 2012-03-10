using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RussianElectionResultScraper.Web.Controllers
{
    public partial class ErrorController : Controller
        {
        //
        // GET: /Error/

        public virtual ActionResult PageNotFound()
            {
            return View();
            }
        }
}
