using Microsoft.VisualBasic;

namespace Core.Enum
{
    public enum TaskStatusType
    {
        Open =1,
        Pending = 2,
        InProcess = 3,
        NeedReview = 4,
        Rejected = 5,
        Completed = 6,
        Overdue = 7
    }
    public enum PaymentStatusType
    {
        Unpaid,
        Paid
    }

    public enum BookingAppointmentStatusType
    {
        Booking,
        Confirmed,
        CheckedIn,
        Completed,
        Cancelled
    }

    public enum ProjectStatusType
    {
        Pending = 1,
        InProcess = 2,
        Rejected = 3,
        Completed = 4,
        Overdue = 5,
    }
}