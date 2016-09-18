using System.Collections.Generic;

namespace SpiceShare.Models.Form
{
    public class FormResponse
    {

        public const string FormComplete = "full";
        public const string FormValid = "ok";
        public const string FormInvalid = "incomplete";

        public string Status { get; set; }
        public List<FieldValidation> Fields
        {
            get;
            set;
        } 
    }
}