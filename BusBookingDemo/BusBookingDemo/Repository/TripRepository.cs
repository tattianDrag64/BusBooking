using BusBookingDemo.Data;
using BusBookingDemo.Entity;
using BusBookingDemo.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace BusBookingDemo.Repository
{
    public class TripRepository : Repository<Trip>, ITripRepository
    {
        //private ApplicationDbContext Context;
        public TripRepository(ApplicationDbContext context) : base(context) { }

        public IQueryable<Trip> Trips => Context.Set<Trip>();

        //public Task DeleteTrip(Trip Trip)
        //{
        //    Items.Set<Trip>.Remove(Trip);
        //    await _context.SaveChangesAsync();
        //}

        //public void editTrip(Trip Trip)
        //{
        //    throw new NotImplementedException();
        //}

        //public void getTrip(Trip Trip)
        //{
        //    throw new NotImplementedException();
        //}

        public void Update(Trip obj)
        {
            Items.Update(obj);
        }

        

        //public IEnumerable<Trip> GetTripsByDirection(int directionId)
        //{
        //    return Items.Where(t => t.Trip.Id == directionId).ToList();
        //}

        //public IEnumerable<Trip> GetTripsByDate(DateTime departureDate)
        //{
        //    return Items.Where(t => t.DepartureDate.Date == departureDate.Date).ToList();
        //}

        //public void Update(Trip trip)
        //{
        //    //var direction = Context.Set<Direction>().FirstOrDefault(d => d.Id == trip.DirectionId);
        //    //trip.DepartureDate = CombineDateAndTime(direction.DepartureDay, direction.DepartureTime);
        //    //trip.ArrivalDate = CombineDateAndTime(direction.DepartureDay, direction.ReturnTime);
        //    //Items.Update(trip);

        //    //------------------------------------------------------------------------------------------------------------------

        //    //var direction = Context.Directions.FirstOrDefault(d => d.Id == trip.DirectionId);
        //    //if (direction == null)
        //    //{
        //    //    throw new InvalidOperationException($"Направление с Id {trip.DirectionId} не найдено.");
        //    //}

        //    //// Обновляем данные поездки на основе данных направления
        //    //trip.DepartureDate = CombineDateAndTime(direction.DepartureDay, direction.DepartureTime);
        //    //trip.ArrivalDate = CombineDateAndTime(direction.DepartureDay, direction.ReturnTime);

        //    //// Обновляем сущность поездки в контексте
        //    //Context.Trips.Update(trip);

        //    //// Сохраняем изменения
        //    //Context.SaveChanges();
        //}

        //public DateTime CombineDateAndTime(DayOfWeek dayOfWeek, TimeSpan time)
        //{
        //    // Определяем текущую дату
        //    DateTime today = DateTime.Today;

        //    // Вычисляем разницу между сегодняшним днем недели и целевым
        //    int daysToAdd = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;

        //    // Находим дату целевого дня недели
        //    DateTime targetDate = today.AddDays(daysToAdd);

        //    // Комбинируем дату и время
        //    return targetDate.Date.Add(time);
        //}
        //public void CompleteTrip(int tripId)
        //{
        //    // Получаем поездку и связанные данные
        //    var trip = Items.FirstOrDefault(t => t.Id == tripId);

        //    if (trip == null)
        //        throw new ArgumentException("Поездка не найдена.");

        //    if (trip.IsCompleted) throw new InvalidOperationException("Поездка уже завершена.");

        //    //    var directionId = Context.Set<Trip>()
        //    //.Where(t => t.Id == tripId) // Фильтрация по TripId
        //    //.Select(t => t.DirectionId)    // Выборка
        //    //                               // Id
        //    //.FirstOrDefault();

        //    //        var busId = Context.Set<Trip>()
        //    //.Include(t => t.Direction)
        //    //.Where(t => t.DirectionId == directionId)
        //    //.Select(t => t.Direction.BusId) // Доступ к BusId через Route
        //    //.FirstOrDefault();

        //    //var busSeats = Context.Set<SeatDetail>()
        //    //               .Where(sd => sd.BusId == trip.Direction.BusId)
        //    //               .ToList();

        //    // Освобождаем все места автобуса
        //    //foreach (var seat in busSeats)
        //    //{
        //    //    seat.IsReserved = false;
        //    //}

        //    // Обновляем статус поездки
        //    trip.IsCompleted = true;

        //    // Сохраняем изменения
        //    Context.SaveChanges();
        //}
    }

}



