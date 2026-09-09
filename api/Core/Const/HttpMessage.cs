namespace Core.Const
{
    public static class HttpMessage
    {
        public const string NotFound = "Not Found";
        public const string NoPermission = "No permission";


        public const  string CheckInformation = "Check again information";
        public const  string Ok = "Successful";
        public static DuplicateMessage Duplicate = new DuplicateMessage();
        public const string ServerError = "Server Error";
        public const string InvalidExtension = "Invalid Extention";
        public const string Unauthorized = "Unauthorized";
        public const string Conflict = "The operation was updated by another user";
        public const  string EmptyContent = "Empty Content";
        public const  string TemplateNotFound = "Template not found";
        public static  InvalidBusiness InvalidBusiness = new InvalidBusiness();
        public static SendingFileMessage SendingFile = new SendingFileMessage();
        public static UserReviewMessage UserReview = new UserReviewMessage();

    }

    public  class DuplicateMessage
    {
        public string DuplicateName = "Duplicate Name";
        public string DuplicateEmail = "Duplicate Email";
        public string DuplicatePhoneNumber = "Duplicate PhoneNumber";
        public string DuplicateUserCode = "Duplicate User Code";
        public string DuplicateRoleCode = "Duplicate Code Role";
        public string DuplicateCode = "Duplicate Code";
    }

    public class InvalidBusiness
    {
        public string SharedDocumentFile = "Failed.Document is Shared To Another Person";
        public string FileExistFolder = "Failed.Documents Still Exist In This Folder";
    }

    public class SendingFileMessage
    {
        public string EmailConfigContentNotFound = "Email Config Content Not Found";
        public string EmptyEmail = "Please add your email before performing this function";
    }

    public class UserReviewMessage
    {
        public string  ExistenceOnDay = "Today You've reviewed already";
        public string ReviewYourSelf = "You can't review yourself";
    }
}