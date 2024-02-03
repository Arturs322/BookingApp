using BookingApp.Domain.Entities;

namespace BookingApp.Application.Common.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static int VillaRoomsAvailable_Count(
            int villaId, 
            List<VillaNumber> villaNumberList, 
            DateOnly checkInDate, 
            int nights,
            List<Reservation> reservations)
        {
            List<int> bookingInDate = new();
            int finalAvailableRoomForAllNights = int.MaxValue;
            var roomsInVilla = villaNumberList.Where(x => x.VillaId == villaId).Count();

            for(int i = 0; i < nights; i++)
            {
                var villasReserved = reservations.Where(x => x.CheckInDate <= checkInDate.AddDays(i) &&
                x.CheckOutDate > checkInDate.AddDays(i) && x.VillaId == villaId);

                foreach(var reservation in villasReserved)
                {
                    if(!bookingInDate.Contains(reservation.Id))
                    {
                        bookingInDate.Add(reservation.Id);
                    }
                }

                var totalAvailableRooms = roomsInVilla - bookingInDate.Count;
                if(totalAvailableRooms == 0)
                {
                    return 0;
                } else
                {
                    if(finalAvailableRoomForAllNights > totalAvailableRooms)
                    {
                        finalAvailableRoomForAllNights = totalAvailableRooms;
                    }
                }

            }

            return finalAvailableRoomForAllNights;
        }
    }
}
