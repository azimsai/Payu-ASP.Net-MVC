using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace PayuTest.Models
{
    public class PaymentDetails
    {

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Enter a valid phone no")]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Entered Phone No format is not valid.")]
        [Display(Name = "Phone Number")]
        public string PhoneNo { get; set; }
        [Required]
        [Display(Name = "Amount")]
        public double Amount { get; set; }
        [Required]
        [Display(Name = "Email address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }        
        public string SuccessUrl { get; set; }
        public string FailUrl { get; set; }
        [Required]
        [Display(Name = "Product Info")]       
        public string ProductInfo { get; set; }


        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public string TxId { get; set; }
        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public string key { get; set; }
        [System.Web.Mvc.HiddenInput(DisplayValue = false)]
        public string Hash { get; set; }


    }
}