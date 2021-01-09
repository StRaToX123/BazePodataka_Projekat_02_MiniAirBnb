using Cassandra;
using CassandraDataLayer.QueryEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CassandraDataLayer
{
    public static class DataProvider
    {
        #region Hotel
        public static Hotel GetHotel(string hotelID)
        {
            ISession session = SessionManager.GetSession();
            Hotel hotel = new Hotel();

            if (session == null)
                return null;

            Row hotelData = session.Execute("select * from \"Hotel\" where \"hotelID\"='1'").FirstOrDefault();

            if(hotelData != null)
            {
                hotel.hotelID = hotelData["hotelID"] != null ? hotelData["hotelID"].ToString() : string.Empty;
                hotel.name = hotelData["name"] != null ? hotelData["name"].ToString() : string.Empty;
                hotel.address = hotelData["address"] != null ? hotelData["address"].ToString() : string.Empty;
                hotel.city = hotelData["city"] != null ? hotelData["city"].ToString() : string.Empty;
                hotel.phone = hotelData["phone"] != null ? hotelData["phone"].ToString() : string.Empty;
                hotel.state = hotelData["state"] != null ? hotelData["state"].ToString() : string.Empty;
                hotel.zip = hotelData["zip"] != null ? hotelData["zip"].ToString() : string.Empty;
            }
            
            return hotel;
        }

        public static List<Hotel> GetHotels()
        {
            ISession session = SessionManager.GetSession();
            List<Hotel> hotels = new List<Hotel>();
            

            if (session == null)
                return null;

            var hotelsData = session.Execute("select * from \"Hotel\"");

            
            foreach(var hotelData in hotelsData)
            {
                Hotel hotel = new Hotel();
                hotel.hotelID = hotelData["hotelID"] != null ? hotelData["hotelID"].ToString() : string.Empty;
                hotel.name = hotelData["name"] != null ? hotelData["name"].ToString() : string.Empty;
                hotel.address = hotelData["address"] != null ? hotelData["address"].ToString() : string.Empty;
                hotel.city = hotelData["city"] != null ? hotelData["city"].ToString() : string.Empty;
                hotel.phone = hotelData["phone"] != null ? hotelData["phone"].ToString() : string.Empty;
                hotel.state = hotelData["state"] != null ? hotelData["state"].ToString() : string.Empty;
                hotel.zip = hotelData["zip"] != null ? hotelData["zip"].ToString() : string.Empty;
                hotels.Add(hotel);
            }
                
            

            return hotels;
        }

        public static void AddHotel(string hotelID)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet hotelData = session.Execute("insert into \"Hotel\" (\"hotelID\", address, city, name, phone, state, zip)  values ('" + hotelID +"', 'Vozda Karadjordja 12', 'Nis', 'Grand', '123', 'Srbija', '18000')");

        }

        public static void DeleteHotel(string hotelID)
        {
            ISession session = SessionManager.GetSession();
            Hotel hotel = new Hotel();

            if (session == null)
                return;

            RowSet hotelData = session.Execute("delete from \"Hotel\" where \"hotelID\" = '" + hotelID + "'");

        }

        #endregion

        #region Room

        public static Room GetRoom(string hotelID, string roomID)
        {
            ISession session = SessionManager.GetSession();
            Room room = new Room();

            if (session == null)
                return null;

            Row roomData = session.Execute("select * from \"Room\" where \"hotelID\"='" + hotelID +"' and \"roomID\"='" + roomID + "'").FirstOrDefault();

            if (roomData != null)
            {
                room.hotelID = roomData["hotelID"] != null ? roomData["hotelID"].ToString() : string.Empty;
                room.roomID = roomData["roomID"] != null ? roomData["roomID"].ToString() : string.Empty;
                room.hottub = roomData["hottub"] != null ? roomData["hottub"].ToString() : string.Empty;
                room.num = roomData["num"] != null ? roomData["num"].ToString() : string.Empty;
                room.rate = roomData["rate"] != null ? roomData["rate"].ToString() : string.Empty;
                room.tv = roomData["tv"] != null ? roomData["tv"].ToString() : string.Empty;
                room.type = roomData["type"] != null ? roomData["type"].ToString() : string.Empty;
            }

            return room;
        }

        public static List<Room> GetRooms()
        {
            ISession session = SessionManager.GetSession();
            List<Room> rooms = new List<Room>();

            if (session == null)
                return null;

            var roomsData = session.Execute("select * from \"Room\"");

            foreach(var row in roomsData)
            {
                Room room = new Room();
                room.hotelID = row["hotelID"] != null ? row["hotelID"].ToString() : string.Empty;
                room.roomID = row["roomID"] != null ? row["roomID"].ToString() : string.Empty;
                room.hottub = row["hottub"] != null ? row["hottub"].ToString() : string.Empty;
                room.num = row["num"] != null ? row["num"].ToString() : string.Empty;
                room.rate = row["rate"] != null ? row["rate"].ToString() : string.Empty;
                room.tv = row["tv"] != null ? row["tv"].ToString() : string.Empty;
                room.type = row["type"] != null ? row["type"].ToString() : string.Empty;

                rooms.Add(room);
            }

            return rooms;
        }

        public static void AddRoom(string hotelID, string roomID)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet roomData = session.Execute("insert into \"Room\"(\"hotelID\",\"roomID\", hottub, num, rate, tv, \"type\") values ('" + hotelID + "', '" + roomID + "', 'yes', '101', '25', 'yes', 'appartment')");

        }

        public static void DeleteRoom(string hotelID, string roomID)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet roomData = session.Execute("delete from \"Room\" where \"hotelID\" = '" + hotelID + "' and \"roomID\" = '" + roomID + "'");

        }

        #endregion

        #region Geust

        public static Guest GetGuest(string phone)
        {
            ISession session = SessionManager.GetSession();
            Guest guest = new Guest();

            if (session == null)
                return null;

            Row guestData = session.Execute("select * from \"Guest\" where phone='" + phone + "'").FirstOrDefault();

            if (guestData != null)
            {
                guest.phone = guestData["phone"] != null ? guestData["phone"].ToString() : string.Empty;
                guest.email = guestData["email"] != null ? guestData["email"].ToString() : string.Empty;
                guest.fname = guestData["fname"] != null ? guestData["fname"].ToString() : string.Empty;
                guest.lname = guestData["lname"] != null ? guestData["lname"].ToString() : string.Empty;
            }

            return guest;
        }

        public static List<Guest> GetGuests()
        {
            ISession session = SessionManager.GetSession();
            List<Guest> guests = new List<Guest>();

            if (session == null)
                return null;

            var guestsData = session.Execute("select * from \"Guest\"");

           
            foreach (var guestData in guestsData)
            {
                Guest guest = new Guest();
                guest.phone = guestData["phone"] != null ? guestData["phone"].ToString() : string.Empty;
                guest.email = guestData["email"] != null ? guestData["email"].ToString() : string.Empty;
                guest.fname = guestData["fname"] != null ? guestData["fname"].ToString() : string.Empty;
                guest.lname = guestData["lname"] != null ? guestData["lname"].ToString() : string.Empty;
                    
                guests.Add(guest);
            }
           

            return guests;
        }

        public static void AddGuest(string phone)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet guestData = session.Execute("insert into \"Guest\"(phone, email, fname, lname) values ('" + phone + "', 'email@email.com', 'test', 'test')");

        }

        public static void DeleteGuest(string phone)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet guestData = session.Execute("delete from \"Guest\" where phone = '" + phone + "'");

        }

        #endregion

        #region Reservation

        public static Reservation GetReservation(string resID)
        {
            ISession session = SessionManager.GetSession();
            Reservation reservation = new Reservation();

            if (session == null)
                return null;

            Row reservationData = session.Execute("select * from \"Reservation\" where \"resID\"='" + resID +"'").FirstOrDefault();

            if (reservationData != null)
            {
                reservation.hotelID = reservationData["hotelID"] != null ? reservationData["hotelID"].ToString() : string.Empty;
                reservation.roomID = reservationData["roomID"] != null ? reservationData["roomID"].ToString() : string.Empty;
                reservation.resID = reservationData["resID"] != null ? reservationData["resID"].ToString() : string.Empty;
                reservation.arrive = reservationData["arrive"] != null ? reservationData["arrive"].ToString() : string.Empty;
                reservation.depart = reservationData["depart"] != null ? reservationData["depart"].ToString() : string.Empty;
                reservation.name = reservationData["name"] != null ? reservationData["name"].ToString() : string.Empty;
                reservation.phone = reservationData["phone"] != null ? reservationData["phone"].ToString() : string.Empty;
                reservation.rate = reservationData["rate"] != null ? reservationData["rate"].ToString() : string.Empty;
            }

            return reservation;
        }

        public static List<Reservation> GetReservations()
        {
            ISession session = SessionManager.GetSession();
            List<Reservation> reservations = new List<Reservation>();

            if (session == null)
                return null;

            var reservationsData = session.Execute("select * from \"Reservation\"");

            
            foreach (Row reservationData in reservationsData)
            {
                Reservation reservation = new Reservation();
                reservation.hotelID = reservationData["hotelID"] != null ? reservationData["hotelID"].ToString() : string.Empty;
                reservation.roomID = reservationData["roomID"] != null ? reservationData["roomID"].ToString() : string.Empty;
                reservation.resID = reservationData["resID"] != null ? reservationData["resID"].ToString() : string.Empty;
                reservation.arrive = reservationData["arrive"] != null ? reservationData["arrive"].ToString() : string.Empty;
                reservation.depart = reservationData["depart"] != null ? reservationData["depart"].ToString() : string.Empty;
                reservation.name = reservationData["name"] != null ? reservationData["name"].ToString() : string.Empty;
                reservation.phone = reservationData["phone"] != null ? reservationData["phone"].ToString() : string.Empty;
                reservation.rate = reservationData["rate"] != null ? reservationData["rate"].ToString() : string.Empty;

                reservations.Add(reservation);
            }
            

            return reservations;
        }

        public static void AddReservation(string resID)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet reservationData = session.Execute("insert into \"Reservation\"(\"resID\", \"hotelID\",\"roomID\", arrive, depart, name, phone, rate) values ('" + resID + "', '1', '101', 'date', 'date', 'test', '018181818', '25')");

        }

        public static void DeleteReservation(string resID)
        {
            ISession session = SessionManager.GetSession();

            if (session == null)
                return;

            RowSet reservationData = session.Execute("delete from \"Reservation\" where \"resID\" = '" + resID + "'");

        }

        #endregion

    }
}
