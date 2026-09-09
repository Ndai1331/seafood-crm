using System.ComponentModel.DataAnnotations;

namespace Contract.CustomAttribute
{
    public class MyValidationAttribute : ValidationAttribute
    {
        public string ValidSupplierValue { get; set; }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
   
                return ValidationResult.Success;
        }
    }
}