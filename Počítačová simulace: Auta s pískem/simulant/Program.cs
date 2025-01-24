using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;

namespace simulant
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Simulation().Start();
            } catch (Exception ex)
            {
                Console.WriteLine("WOMP WOMP " + ex.Message);
            }
        }
        class Simulation
        {
            private PriorityQueue<Event, int> EventQueue { get; set; }
            private List<Car> Arsenal { get; set; }
            private int Sand { get; set; }
            private bool isLoading { get; set; }
            private Stoplight Stoplight { get; set; }

            public void Start()
            {
                EventQueue = new PriorityQueue<Event, int>();
                Arsenal = new List<Car>();
                isLoading = false;

                GetInput();
                MainLoop();
            }
            private void MainLoop()
            {
                Arsenal = Arsenal.OrderByDescending(c => c.Capacity)
                    .ThenBy(c => c.TravelTime)
                    .ThenBy(c => c.LoadingTime)
                    .ThenBy(c => c.UnloadingTime)
                    .ToList();
                EventQueue.Enqueue(new Event(Arsenal[0], EventType.LOAD, Status.START, 0), 0);
                Arsenal.RemoveAt(0);

                int totalTime = 0;
                while (Sand > 0 || EventQueue.Count > 0)
                {
                    var currentEvent = EventQueue.Dequeue();
                    totalTime = currentEvent.Time;
                    ProcessEvent(currentEvent);
                }

                Console.WriteLine($"\nCELKOVY CAS {totalTime} minut");
            }
            private void ProcessEvent(Event e)
            {
                Console.WriteLine(e);

                switch (e.EventType)
                {
                    case EventType.LOAD when e.Status == Status.START:
                        Sand = Math.Max(0, Sand - e.Car.Capacity);
                        isLoading = true;
                        EventQueue.Enqueue(new Event(e.Car, EventType.LOAD, Status.FINISH, e.Time + e.Car.LoadingTime), e.Time + e.Car.LoadingTime);
                        EventQueue.Enqueue(new Event(e.Car, EventType.TRAVEL, Status.START, e.Time + e.Car.LoadingTime), e.Time + e.Car.LoadingTime);
                        break;

                    case EventType.LOAD when e.Status == Status.FINISH:
                        isLoading = false;
                        if (Arsenal.Count > 0)
                        {
                            var nextCar = Arsenal[0];
                            Arsenal.RemoveAt(0);
                            EventQueue.Enqueue(new Event(nextCar, EventType.LOAD, Status.START, e.Time), e.Time);
                            isLoading = true;
                        }
                        break;

                    case EventType.TRAVEL when e.Status == Status.START:
                        int arriveTime = e.Time + (e.Car.TravelTime / 2);
                        var (isGreenA, timeToChange) = Stoplight.GetState(arriveTime);

                        int waitingTime = 0;
                        if (!isGreenA && Stoplight.GreenDuration != 0)
                        {
                            Console.WriteLine($"[{arriveTime}] - Car type: {e.Car.Type} Car ID: {e.Car.ID} is waiting on a stoplight (B->A), waiting time: {timeToChange}");
                            waitingTime = timeToChange;
                        }

                        int totalTravelTime = e.Car.TravelTime + waitingTime;

                        EventQueue.Enqueue(new Event(e.Car, EventType.TRAVEL, Status.FINISH, e.Time + totalTravelTime), e.Time + totalTravelTime);
                        break;
                    
                    case EventType.TRAVEL when e.Status == Status.FINISH:
                        EventQueue.Enqueue(new Event(e.Car, EventType.UNLOAD, Status.START, e.Time), e.Time);
                        break;

                    case EventType.UNLOAD when e.Status == Status.START:
                        EventQueue.Enqueue(new Event(e.Car, EventType.UNLOAD, Status.FINISH, e.Time + e.Car.UnloadingTime), e.Time + e.Car.UnloadingTime);
                        break;

                    case EventType.UNLOAD when e.Status == Status.FINISH:
                        EventQueue.Enqueue(new Event(e.Car, EventType.TRAVELBACK, Status.START, e.Time), e.Time);
                        break;

                    case EventType.TRAVELBACK when e.Status == Status.START:
                        int arrivalTimeBack = e.Time + (e.Car.TravelTime / 2);
                        var (isGreenABack, timeToChangeBack) = Stoplight.GetState(arrivalTimeBack);
                        
                        int waitingTimeBack = 0;
                        if (isGreenABack && Stoplight.GreenDuration != 0)
                        {
                            Console.WriteLine($"[{arrivalTimeBack}] - Car type: {e.Car.Type} Car ID: {e.Car.ID} is waiting on a stoplight (A->B), waiting time: {timeToChangeBack}");
                            waitingTimeBack = timeToChangeBack;
                        }

                        int totalTravelBackTime = e.Car.TravelTime + waitingTimeBack;
                        
                        EventQueue.Enqueue(new Event(e.Car, EventType.TRAVELBACK, Status.FINISH, e.Time + totalTravelBackTime),e.Time + totalTravelBackTime);
                        break;

                    case EventType.TRAVELBACK when e.Status == Status.FINISH:
                        Arsenal.Add(e.Car);
                        Arsenal = Arsenal.OrderByDescending(c => c.Capacity)
                            .ThenBy(c => c.TravelTime)
                            .ThenBy(c => c.LoadingTime)
                            .ThenBy(c => c.UnloadingTime)
                            .ToList();
                        
                        if (!isLoading && Arsenal.Count > 0 && Sand > 0)
                        {
                            var nextCar = Arsenal[0];
                            Arsenal.RemoveAt(0);
                            EventQueue.Enqueue(new Event(nextCar, EventType.LOAD, Status.START, e.Time), e.Time);
                            isLoading = true;
                        }
                        break;
                }
            }
            private void GetInput()
            {
                Console.Beep(); // hehe heeheehehe
                Console.OutputEncoding = Encoding.UTF8;
                Console.WriteLine("Počet ruzných aut a hmotnost písku:");
                int[] input = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);
                Console.WriteLine("Interval semaforu (0 pro žádný semafor)");
                int stoplightInput = int.Parse(Console.ReadLine());
                Stoplight = new Stoplight(stoplightInput);

                Sand = input[1];
                Console.WriteLine($"Na dalších {input[0]} řádků napiš jaké auta chceš ve formátu: count loadingTime unloadingTime travelTime capacity");

                for (int i = 0; i < input[0]; i++)
                {
                    int[] Car = Array.ConvertAll(Console.ReadLine().Split(' '), int.Parse);

                    for (int j = 0; j < Car[0]; j++)
                    {
                        Car newCar = new Car(j + 1, i + 1, Car[1], Car[2], Car[3], Car[4]);
                        Arsenal.Add(newCar);
                    }
                }
                Console.WriteLine();
            }
        }

        enum Status { START, FINISH }
        enum EventType { LOAD, UNLOAD, TRAVEL, TRAVELBACK }

        class Car
        {
            public int ID { get; private set; }
            public int Type { get; private set; }
            public int LoadingTime { get; private set; }
            public int UnloadingTime { get; private set; }
            public int TravelTime { get; private set; }
            public int Capacity { get; private set; }

            public Car(int iD, int type, int lT, int uT, int tT, int c)
            {
                ID = iD; LoadingTime = lT; UnloadingTime = uT;
                TravelTime = tT; Capacity = c;
                Type = type;
            }
        }
        class Event
        {
            public int Time { get; private set; }
            public Car Car { get; private set; }
            public Status Status { get; private set; }
            public EventType EventType { get; private set; }

            public Event(Car car, EventType eventType, Status status, int time)
            {
                Car = car; Status = status; EventType = eventType;
                Time = time;
            }

            public override string ToString()
            {
                return $"[{Time}] – Car type: {Car.Type} Car ID: {Car.ID} {EventType.ToString()} ({Status.ToString()})";
            }
        }

        class Stoplight
        {
            public int GreenDuration { get; }
            public int Cycle => 2 * GreenDuration;

            public Stoplight(int greenDuration)
            {
                GreenDuration = greenDuration;
            }

            public (bool isGreenA, int nextSwitch) GetState(int currentTime)
            {
                int cycleTime = currentTime % Cycle;
                if (cycleTime < GreenDuration)
                {
                    return (true, GreenDuration - cycleTime); //A->B 
                }
                else
                {
                    return (false, Cycle - cycleTime); // B->A 
                }
            }
        }
    }
}
