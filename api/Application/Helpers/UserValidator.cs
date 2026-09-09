using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Core.Const;
using Core.Enum;

namespace Application.Helpers
{
    public class UserValidator
    {

        public bool ValidateName(string input)
        {
            Regex re = new Regex(ContentRegularExpression.NAME);
            if (input.IsNullOrWhiteSpace()) return false;
            
            if (input.Length < 1||!re.IsMatch(input)
            )
            {
                return false;
            }
            return true;

        }
        
        public bool ValidateUserName(string input)
        {
            if (input.IsNullOrWhiteSpace()) return false;

            Regex re = new Regex(ContentRegularExpression.USER_NAME);

            if (input.Length < 2||!re.IsMatch(input))
                return false;
            else
                return true;

        }

        public bool ValidateGender(string input)
        {
            try
            {
                if (!input.IsNullOrWhiteSpace())
                {
                    Gender gender = (Gender) Enum.Parse(typeof(Gender), input);
                    return true;
                }
                return true;
        
            }
            catch (Exception e)
            {
                return false;
            }

            return false;

        }

        public bool ValidatePassword(string input)
        {
            if (input.IsNullOrWhiteSpace()) return false;

            Regex re = new Regex(ContentRegularExpression.PASSWORD);

            if (input.Length < 2||!re.IsMatch(input))
                return false;
            else
                return true;
        }
        
        public bool ValidateEmail(string input)
        {
            if (!input.IsNullOrWhiteSpace())
            {
                Regex re = new Regex(ContentRegularExpression.EMAIL);
            
                if (re.IsMatch(input))
                    return (true);
                else
                    return (false);
            }

            return true;
        }

        public bool ValidateBool(string input)
        {
            if (!input.IsNullOrWhiteSpace())
            {
                bool myBool;
                if (bool.TryParse(input, out myBool))
                {
                    return true;
                }

                return false;
            }


            return false;
        }

        public bool ValidateDate(string input)
        {
            DateTime result = default;

            if (!input.IsNullOrWhiteSpace())
            {
                if (DateTime.TryParseExact(input, "dd/MM/yyyy", 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.None, out result))
                {
                    return true;
                }
            
                return false;
            }

            return true;
        }
        
        
        public bool ValidateUserCode(string input)
        {
            if (input.IsNullOrWhiteSpace()) return false;

            if (input.Length < 2 )
            {
                return false;
            }

            return true;
        }
        
        public bool ValidatePhone(string input)
        {
            if (input.IsNullOrWhiteSpace()) return false;

            Regex re = new Regex(ContentRegularExpression.NUMBER_PHONE);
            
            if (re.IsMatch(input))
                return (true);
            else
                return (false);

        }

        
    }
}