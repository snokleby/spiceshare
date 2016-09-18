using System;

namespace SpiceShare.Models.Form
{
    public class FieldValidation
    {
        public string Id { get; set; }
        public bool Valid { get; set; }

        public bool Changed { get; set; }
        public Guid ChangedGUid { get; set; }
    }
}