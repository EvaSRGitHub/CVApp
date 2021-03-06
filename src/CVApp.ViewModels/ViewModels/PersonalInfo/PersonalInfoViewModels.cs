﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;

namespace CVApp.ViewModels.PersonalInfo
{
    public class PersonalInfoViewModels
    {
        public abstract class PersonalInfoBaseViewModel : IValidatableObject
        {
            private readonly string[] AllowedFileExtensions = new[] { ".jpg", ".png" };

            [Required, Display(Name = "First Name")]
            [RegularExpression("^(?=[A-Z][a-z])([A-Za-z]|[A-Za-z][-](?=[A-Za-z])|(?=[A-Za-z]))*$", ErrorMessage = "First name could contains only English letters and hyphen")]
            public string FirstName { get; set; }

            [Required, MaxLength(50), Display(Name = "Last Name")]
            [RegularExpression("^(?=[A-Z][a-z])([A-Za-z]|[A-Za-z]['-](?=[A-Za-z])|(?=[A-Za-z]))*$", ErrorMessage = "Last name could contains only English letters, hyphen and apostrophe")]
            public string LastName { get; set; }

            [Required(ErrorMessage = "Please enter your email")]
            [RegularExpression(@"^[\w!#$%&'*+\-\/=?\^_`{|}~]+(\.[\w!#$%&'*+\-\/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$", ErrorMessage = "Invalid email format.")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Please enter you phone number"), Display(Name = "Phone Number")]
            [RegularExpression(@"^[\+|\-\/\d ]{7,}$", ErrorMessage = "Phone number may contains digits and symbols (+, /, -, ) with minimum length of 7 (ex. +359/288111, +359/888 999 333)")]
            public string PhoneNumber { get; set; }

            public string Summary { get; set; }

            public IFormFile Picture { get; set; }

            [Display(Name = "Portfolio")]
            public string RepoProfile { get; set; }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                if (!(this.Picture == null))
                {
                    var extension = Path.GetExtension(this.Picture.FileName);

                    if (!AllowedFileExtensions.Contains(extension.ToLower()))
                    {
                        yield return new ValidationResult($"The {nameof(this.Picture)} file must be jpg or png format.", new List<string>() { nameof(this.Picture) });
                    }
                }
            }
        }

        public class PersonalInfoInputViewModel : PersonalInfoBaseViewModel { }

        public class PersonalInfoEditViewModel : PersonalInfoBaseViewModel
        {
            [Required]
            public int ResumeId { get; set; }

            [Display(Name = "Current Picture")]
            public string CurrentPicture { get; set; }
        }

        public class PersonalInfoOutViewModel
        {
            public string Id { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Email { get; set; }

            public string PhoneNumber { get; set; }

            public string Summary { get; set; }

            public string Picture { get; set; }

            public string RepoProfile { get; set; }

            public int ResumeId { get; set; }
        }
    }
}
