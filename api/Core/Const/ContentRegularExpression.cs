namespace Core.Const
{
    public  class ContentRegularExpression
    {
        public const string NAME =
                @"^\w[a-zA-ZÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚĂĐĨŨƠàáâãèéêìíòóôõùúăđĩũơƯĂẠẢẤẦẨẪẬẮẰẲẴẶẸẺẼỀỀỂẾưăạảấầẩẫậắằẳẵặẹẻẽềềểếỄỆỈỊỌỎỐỒỔỖỘỚỜỞỠỢỤỦỨỪễệỉịọỏốồổỗộớờởỡợụủứừỬỮỰỲỴÝỶỸửữựỳỵýỷỹ\s\W]*$";

        public const string EMAIL = @"^[\w,.]+@[\w,.]+$";
        public const string NUMBER_PHONE = @"^\d{10}$";
        public const string USER_NAME = @"^\w+$";
        public const string PASSWORD = @"^\w+$";
        public const string SLUG = @"^[a-z\d](?:[a-z\d_-]*[a-z\d])?$";
    }
}