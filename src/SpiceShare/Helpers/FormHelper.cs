using SpiceShare.Models.Form;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiceShare.Helpers
{
    public class FormHelper
    {
        public static void SetChanged(FormResponse res, string id)
        {
            var f = res.Fields.FirstOrDefault(m => m.Id == id);
            if (f != null)
            {
                f.Changed = true;
                f.ChangedGUid = Guid.NewGuid();
            }
        }
    }
}
