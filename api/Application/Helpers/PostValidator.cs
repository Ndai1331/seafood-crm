using Core.Const;
using System.Text.RegularExpressions;

namespace Application.Helpers
{
    public class PostValidator
    {
        public bool ValidateSlug(string input)
        {
            Regex re = new Regex(ContentRegularExpression.SLUG);
            if (input.IsNullOrWhiteSpace()) return false;

            if (input.Length < 1 || !re.IsMatch(input))
            {
                return false;
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

        public bool ValidateGuid(string input)
        {
            if (!input.IsNullOrWhiteSpace())
            {
                int myint;
                if (int.TryParse(input, out myint) && myint != null)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
    }
}
