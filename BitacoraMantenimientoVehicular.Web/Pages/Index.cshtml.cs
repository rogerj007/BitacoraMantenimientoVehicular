using System.Collections.Generic;
using System.Linq;
using BitacoraMantenimientoVehicular.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BitacoraMantenimientoVehicular.Web.Pages {
    public class IndexModel : PageModel {

        public IEnumerable<string> Images { get; set; }

        public void OnGet() {
            Images = Enumerable.Range(1, 3).Select(i => Url.Content($"~/images/{i}.jpeg"));
           
        }
    }
}
