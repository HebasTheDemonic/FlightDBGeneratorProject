using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlightProject.POCOs;
using FlightProject;
using FlightProject.Facades;
using Prism.Commands;
using System.Threading;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace FlightProjectDBGenerator 
{
    class MainWindowViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private bool _isProcessRunning;
        private decimal _progressPercentage;
        private decimal totalActions;
        private ObservableCollection<string> _logList;
        private Random Random;
        private decimal actionsCompleted;

        private Dictionary<int, int> DictionaryOfFlightListSizeByAirline;
        private Dictionary<int, int> DictionaryOfTicketListSizeByCustomer;

        private static FlyingCenterSystem flyingCenterSystem = FlyingCenterSystem.GetInstance();

        private AutoResetEvent ListBoxSpotReservationEvent = new AutoResetEvent(true);
        private AutoResetEvent UserLoginEvent = new AutoResetEvent(true);
        
        public DelegateCommand ReplaceDBCommand { get; set; }
        public DelegateCommand AddToDBCommand { get; set; }

        public DataSet AirlineDataSet { get; set; }
        public DataSet CustomerDataSet { get; set; }
        public DataSet AdministratorDataSet { get; set; }
        public DataSet CountriesDataSet { get; set; }
        public DataSet FlightsPerCompanyDataSet { get; set; }
        public DataSet TicketsPerCustomerDataSet { get; set; }

        public bool IsProcessRunning
        {
            get
            {
                return _isProcessRunning;
            }
            set
            {
                _isProcessRunning = value;
                OnPropertyChanged("ProcessStarted");
            }
        }
        public decimal ProgressPercentage
        {
            get
            {
                return _progressPercentage;
            }
            set
            {
                _progressPercentage = Decimal.Round(value);
                OnPropertyChanged("ProgressPercentage");
            }
        }
        public ObservableCollection<string> LogList
        {
            get
            {
                return _logList;
            }
            set
            {
                _logList = value;
                OnPropertyChanged("LogList");
            }
        }

        private Dispatcher Dispatcher { get; set; }

      
        Thread AdministratorThread;
        Thread CustomerThread;
        Thread AirlineThread;
        Thread CountriesThread;
        Thread FlightThread;
        Thread TicketThread;

        FlyingCenterSystem system;

        List<AirlineCompany> airlineCompanies;
        List<Customer> customers;


        public MainWindowViewModel()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;

            system = FlyingCenterSystem.GetInstance();

            AdministratorThread = new Thread(AdministratorDBCreator);
            CustomerThread = new Thread(CustomerDBCreator);
            AirlineThread = new Thread(AirlineCompanyDBCreator);
            CountriesThread = new Thread(CountryDBCreator);
            FlightThread = new Thread(FlightDBCreator);
            TicketThread = new Thread(TicketDBCreator);

            AirlineDataSet = new DataSet();
            CustomerDataSet = new DataSet();
            AdministratorDataSet = new DataSet();
            CountriesDataSet = new DataSet();
            FlightsPerCompanyDataSet = new DataSet();
            TicketsPerCustomerDataSet = new DataSet();
            
            ReplaceDBCommand = new DelegateCommand(ExecuteReplaceDBCommand, canExecuteCommand);
            AddToDBCommand = new DelegateCommand(ExecuteAddToDBCommand, canExecuteCommand);

            LogList = new ObservableCollection<string>();
            Random = new Random();
            IsProcessRunning = false;

            AdministratorThread.Start();
            CustomerThread.Start();
            AirlineThread.Start();
            CountriesThread.Start();
            FlightThread.Start();
            TicketThread.Start();

            Task.Run(() =>
            {
                while (true)
                {
                    ReplaceDBCommand.RaiseCanExecuteChanged();
                    AddToDBCommand.RaiseCanExecuteChanged();
                    Thread.Sleep(500);
                }
            });
        }

        public void ExecuteReplaceDBCommand()
        {
            IsProcessRunning = true;
            actionsCompleted = 0;
            TotalActionsCalculator();
            totalActions++; // adds 1 to total actions for clearing the database
            system.ClearDb();
            GetListBoxIndex("Database Erased.");
            GetListBoxIndex("Head Administrator Registration Restored."); //This Action is imbeded inside of the ClearDb command
            actionsCompleted = actionsCompleted + 2;
            ProgressPercentage = (actionsCompleted / totalActions) * 100;
            AdministratorDataSet.listSize--;
            ExecuteAddToDBCommand();
        }

        public async void ExecuteAddToDBCommand()
        {
            if (IsProcessRunning == false)
            {
                IsProcessRunning = true;
                actionsCompleted = 0;
                TotalActionsCalculator();
            }

            AdministratorDataSet.runThread = true;
            CountriesDataSet.runThread = true;
            AirlineDataSet.runThread = true;
            CustomerDataSet.runThread = true;
            FlightsPerCompanyDataSet.runThread = true;
            TicketsPerCustomerDataSet.runThread = true;

            await Task.Run(() => {
                AdministratorDataSet.DBCreated.WaitOne();
                CountriesDataSet.DBCreated.WaitOne();
                AirlineDataSet.DBCreated.WaitOne();
                CustomerDataSet.DBCreated.WaitOne();
                FlightsPerCompanyDataSet.DBCreated.WaitOne();
                TicketsPerCustomerDataSet.DBCreated.WaitOne();
                AdministratorDataSet.DBCreated.Reset();
                CountriesDataSet.DBCreated.Reset();
                AirlineDataSet.DBCreated.Reset();
                CustomerDataSet.DBCreated.Reset();
                FlightsPerCompanyDataSet.DBCreated.Reset();
                TicketsPerCustomerDataSet.DBCreated.Reset();
                IsProcessRunning = false;
            });
        }

        private void AdministratorDBCreator()
        {
            while (true)
            {
                if (AdministratorDataSet.runThread)
                {
                    List<Administrator> administrators = new List<Administrator>();
                    RandomGenerator randomGenerator = new RandomGenerator();

                    if (AdministratorDataSet.listSize > 0)
                    {
                        int ListBoxIndex = GetListBoxIndex($"Creating Administrator Accounts. (0/{AdministratorDataSet.listSize})");
                        for (int index = 1; index <= AdministratorDataSet.listSize; index++)
                        {
                            Administrator administrator = randomGenerator.AdministratorGenerator();
                            if(administrator.UserName == null)
                            {
                                index--;
                                continue;
                            }
                            administrators.Add(administrator);
                            Dispatcher.Invoke(() => LogList[ListBoxIndex] = $"Creating Administrator Accounts. ({index}/{AdministratorDataSet.listSize})");
                            UpdateProgress();
                        }
                    }

                    int facadeIndex = UserLogin("Admin", "9999");
                    LoggedInAdministratorFacade AdminFacade = (LoggedInAdministratorFacade)FlyingCenterSystem.FacadeList[facadeIndex];
                    foreach (Administrator administrator in administrators)
                    {
                        AdminFacade.CreateNewAdministrator(AdminFacade.LoginToken, administrator);
                    }

                    AdministratorDataSet.DBCreated.Set();
                    AdministratorDataSet.runThread = false;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void CountryDBCreator()
        {
            while (true)
            {
                if (CountriesDataSet.runThread)
                {
                    List<Country> countries = new List<Country>();
                    RandomGenerator randomGenerator = new RandomGenerator();

                    int listBoxIndex = GetListBoxIndex($"Creating Country List. (0/{CountriesDataSet.listSize})");
                    for (int index = 1; index <= CountriesDataSet.listSize; index++)
                    {
                        Country country = randomGenerator.CountryGenerator();
                        if (country.CountryName == null)
                        {
                            index--;
                            continue;
                        }
                        countries.Add(country);
                        Dispatcher.Invoke(() => LogList[listBoxIndex] = $"Creating Country List. ({index}/{CountriesDataSet.listSize})");
                        UpdateProgress();
                    }

                    //int facadeIndex = UserLogin("Admin", "9999");
                    //LoggedInAdministratorFacade AdminFacade = (LoggedInAdministratorFacade)FlyingCenterSystem.FacadeList[facadeIndex];
                    //foreach (Country country in countries)
                    //{
                    //    AdminFacade.CreateCountry(AdminFacade.LoginToken, country);
                    //}

                    CountriesDataSet.DBCreated.Set();
                    CountriesDataSet.runThread = false;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void AirlineCompanyDBCreator()
        {
            while (true)
            {
                if (AirlineDataSet.runThread)
                {
                    airlineCompanies = new List<AirlineCompany>();
                    RandomGenerator randomGenerator = new RandomGenerator();

                    int listBoxIndex = GetListBoxIndex($"Creating Airline Company Accounts. (0/{AirlineDataSet.listSize})");
                    for (int index = 1; index <= AirlineDataSet.listSize; index++)
                    {
                        AirlineCompany airlineCompany = randomGenerator.AirlineCompanyGenerator();
                        if (airlineCompany.UserName == null)
                        {
                            index--;
                            continue;
                        }
                        airlineCompany = new AirlineCompany(airlineCompany.AirlineName, airlineCompany.UserName, airlineCompany.Password, Random.Next(1, CountriesDataSet.listSize + 1));
                        airlineCompanies.Add(airlineCompany);
                        Dispatcher.Invoke(() => LogList[listBoxIndex] = $"Creating Airline Company Accounts. ({index}/{AirlineDataSet.listSize})");
                        UpdateProgress();
                    }
                    CountriesDataSet.DBCreated.WaitOne();

                    //int facadeIndex = UserLogin("Admin", "9999");
                    //LoggedInAdministratorFacade AdminFacade = (LoggedInAdministratorFacade)FlyingCenterSystem.FacadeList[facadeIndex];
                    //foreach (AirlineCompany airline in airlineCompanies)
                    //{
                    //    AdminFacade.CreateNewAirline(AdminFacade.LoginToken, airline);
                    //}
                    
                    AirlineDataSet.DBCreated.Set();
                    AirlineDataSet.runThread = false;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void CustomerDBCreator()
        {
            while (true)
            {
                if (CustomerDataSet.runThread)
                {
                    customers = new List<Customer>();
                    RandomGenerator randomGenerator = new RandomGenerator();

                    int listBoxIndex = GetListBoxIndex($"Creating Customer Accounts. (0/{CustomerDataSet.listSize})");
                    for (int index = 1; index <= CustomerDataSet.listSize; index++)
                    {
                        Customer customer = randomGenerator.CustomerGenerator();
                        if(customer.UserName == null)
                        {
                            index--;
                            continue;
                        }
                        customers.Add(customer);
                        Dispatcher.Invoke(() => LogList[listBoxIndex] = $"Creating Customer Accounts. ({index}/{CustomerDataSet.listSize})");
                        UpdateProgress();
                    }

                    //int facadeIndex = UserLogin("Admin", "9999");
                    //LoggedInAdministratorFacade AdminFacade = (LoggedInAdministratorFacade)FlyingCenterSystem.FacadeList[facadeIndex];
                    //foreach (Customer customer in customers)
                    //{
                    //    AdminFacade.CreateNewCustomer(AdminFacade.LoginToken, customer);
                    //}

                    CustomerDataSet.DBCreated.Set();
                    CustomerDataSet.runThread = false;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void FlightDBCreator()
        {
            while (true)
            {
                if (FlightsPerCompanyDataSet.runThread)
                {
                    List<Flight> flights = new List<Flight>();
                    RandomGenerator randomGenerator = new RandomGenerator();
                    List<DateTime> DepartureDates = randomGenerator.DepartureDateGenerator(FlightsPerCompanyDataSet.listSize).ToList<DateTime>();
                    List<DateTime> LandingDates = randomGenerator.LandingDateGenerator(DepartureDates).ToList<DateTime>();

                    int listBoxIndex = GetListBoxIndex($"Creating Flight List. (0/{FlightsPerCompanyDataSet.listSize})");
                    int CompletedItems = 0;
                    for (int keyIndex = 0; keyIndex < AirlineDataSet.listSize; keyIndex++)
                    {
                        DictionaryOfFlightListSizeByAirline.TryGetValue(keyIndex, out int currentFlightListSize);
                        for (int index = 1; index <= currentFlightListSize; index++)
                        {
                            Flight flight = new Flight(keyIndex + 1, Random.Next(1, CountriesDataSet.listSize + 1), Random.Next(1, CountriesDataSet.listSize + 1), DepartureDates[index], LandingDates[index], Random.Next(100, 300));
                            flights.Add(flight);
                            CompletedItems++;
                            Dispatcher.Invoke(() => LogList[listBoxIndex] = $"Creating Flight List. ({CompletedItems}/{FlightsPerCompanyDataSet.listSize})");
                            UpdateProgress();
                        }
                    }
                    AirlineDataSet.DBCreated.WaitOne();

                    //int totalIndex = 0;
                    //for (int keyIndex = 0; keyIndex < AirlineDataSet.listSize; keyIndex++)
                    //{
                    //    int facadeIndex = UserLogin(airlineCompanies[keyIndex].UserName, airlineCompanies[keyIndex].Password);
                    //    LoggedInAirlineFacade airlineFacade = (LoggedInAirlineFacade)FlyingCenterSystem.FacadeList[facadeIndex];
                    //    DictionaryOfFlightListSizeByAirline.TryGetValue(keyIndex, out int CurrentFlightListSize);

                    //    for (int flightListIndex = 1; flightListIndex <= CurrentFlightListSize; totalIndex++, flightListIndex++)
                    //    {
                    //        airlineFacade.CreateFlight(airlineFacade.LoginToken, flights[totalIndex]);   
                    //    }
                    //}

                    FlightsPerCompanyDataSet.DBCreated.Set();
                    FlightsPerCompanyDataSet.runThread = false;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private void TicketDBCreator()
        {
            while (true)
            {
                if (TicketsPerCustomerDataSet.runThread)
                {
                    List<Ticket> tickets = new List<Ticket>();

                    int listBoxIndex = GetListBoxIndex($"Creating Ticket List. (0/{TicketsPerCustomerDataSet.listSize})");
                    int completedItems = 0;
                    for (int keyIndex = 0; keyIndex < CustomerDataSet.listSize; keyIndex++)
                    {
                        DictionaryOfTicketListSizeByCustomer.TryGetValue(keyIndex, out int currentTicketListSize);
                        for (int index = 1; index <= currentTicketListSize; index++)
                        {
                            Ticket ticket = new Ticket(Random.Next(1, FlightsPerCompanyDataSet.listSize + 1), keyIndex + 1);
                            if (tickets.Count != 0)
                            {
                                foreach (Ticket item in tickets)
                                {
                                    if (item.Equals(ticket))
                                    {
                                        index--;
                                        continue;
                                    }
                                }
                            }
                            tickets.Add(ticket);
                            completedItems++;
                            Dispatcher.Invoke(() => LogList[listBoxIndex] = $"Creating Ticket List. ({completedItems}/{TicketsPerCustomerDataSet.listSize})");
                            UpdateProgress();
                        }
                    }
                    FlightsPerCompanyDataSet.DBCreated.WaitOne();
                    CustomerDataSet.DBCreated.WaitOne();

                    //int totalIndex = 0;
                    //for (int keyIndex = 0; keyIndex < CustomerDataSet.listSize; keyIndex++)
                    //{
                    //    int facadeIndex = UserLogin(customers[keyIndex].UserName, customers[keyIndex].Password);
                    //    LoggedInCustomerFacade customerFacade = (LoggedInCustomerFacade)FlyingCenterSystem.FacadeList[facadeIndex];
                    //    DictionaryOfTicketListSizeByCustomer.TryGetValue(keyIndex, out int currentListSize); 
                    //    for (int index = 0; index < currentListSize; index++, totalIndex++)
                    //    {
                    //        customerFacade.PurchaseTicket(customerFacade.LoginToken, tickets[totalIndex].FlightId);
                    //    }
                    //}

                    TicketsPerCustomerDataSet.DBCreated.Set();
                    TicketsPerCustomerDataSet.runThread = false;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        }

        private bool canExecuteCommand()
        {
            return !IsProcessRunning;
        }

        // determines the total number of actions needed to generate the database lists then doubles the amount of total actions to account for registering the new lists into the real database

        public void TotalActionsCalculator()
        {
            DictionaryOfFlightListSizeByAirline = new Dictionary<int, int>();
            DictionaryOfTicketListSizeByCustomer = new Dictionary<int, int>();
            totalActions = 0;
            FlightsPerCompanyDataSet.listSize = 0;
            TicketsPerCustomerDataSet.listSize = 0;

            AirlineDataSet.listSize = GetListSize(AirlineDataSet);
            totalActions = totalActions + AirlineDataSet.listSize;
            CustomerDataSet.listSize = GetListSize(CustomerDataSet);
            totalActions = totalActions + CustomerDataSet.listSize;
            AdministratorDataSet.listSize = GetListSize(AdministratorDataSet);
            totalActions = totalActions + AdministratorDataSet.listSize;
            CountriesDataSet.listSize = GetListSize(CountriesDataSet);
            totalActions = totalActions + CountriesDataSet.listSize;

            for (int index = 0; index < AirlineDataSet.listSize; index++)
            {
                int listSize = GetListSize(FlightsPerCompanyDataSet);
                DictionaryOfFlightListSizeByAirline.Add(index, listSize);
                totalActions = totalActions + listSize;
                FlightsPerCompanyDataSet.listSize = FlightsPerCompanyDataSet.listSize + listSize;
            }

            for (int index = 0; index < CustomerDataSet.listSize; index++)
            {
                int listSize = GetListSize(TicketsPerCustomerDataSet);
                DictionaryOfTicketListSizeByCustomer.Add(index, listSize);
                totalActions = totalActions + listSize;
                TicketsPerCustomerDataSet.listSize = TicketsPerCustomerDataSet.listSize + listSize;
            }

            totalActions = totalActions * 2;
        }

        private void UpdateProgress()
        {
            actionsCompleted++;
            ProgressPercentage = (actionsCompleted / totalActions) * 100;
        }

        private int GetListSize(DataSet dataSet)
        {
            if (dataSet.IsRandomEnabled)
            {
                return Random.Next(dataSet.MinRandomValue, dataSet.MaxRandomValue);
            }
            else
            {
                return dataSet.NonRandomValue;
            }
        }

        // Makes the list box thread safe.

        private int GetListBoxIndex(string initialMessage)
        {
            ListBoxSpotReservationEvent.WaitOne();
            Dispatcher.Invoke(() => LogList.Add(initialMessage));
            int index = LogList.Count - 1;
            ListBoxSpotReservationEvent.Set();
            return index;
        }

        // thread safe user login.

        private int UserLogin(string username, string password)
        {
            UserLoginEvent.WaitOne();
            int index = flyingCenterSystem.UserLogin(username,password);
            UserLoginEvent.Set();
            return index;
        }

        public string Error
        {
            get
            {
                return string.Empty;
            }
        }

        public string this[string propertyName]
        {
            get
            {
                return GetErrorForProperty(propertyName);
            }
        }

        private string GetErrorForProperty(string propertyName)
        {
            switch (propertyName)
            {
                case "TicketsPerCustomerDataSet":
                    if (TicketsPerCustomerDataSet.IsRandomDisabled)
                    {
                        if (FlightsPerCompanyDataSet.IsRandomDisabled)
                        {
                            if(TicketsPerCustomerDataSet.NonRandomValue > FlightsPerCompanyDataSet.NonRandomValue)
                            {
                                return "Ticket number cannot exceed amount of flights.";
                            }
                            else
                            {
                                return string.Empty;
                            }
                        }
                        else
                        {
                            if(TicketsPerCustomerDataSet.NonRandomValue > FlightsPerCompanyDataSet.MinRandomValue)
                            {
                                return "Ticket number cannot exceed the minimum flight range.";
                            }
                            else
                            {
                                return string.Empty;
                            }
                        }
                    }
                    else
                    {
                        if (FlightsPerCompanyDataSet.IsRandomDisabled)
                        {
                            if (TicketsPerCustomerDataSet.MaxRandomValue > FlightsPerCompanyDataSet.NonRandomValue)
                            {
                                return "Maximum range of tickets cannot exceed amount of flights.";
                            }
                            else
                            {
                                return string.Empty;
                            }
                        }
                        else
                        {
                            if (TicketsPerCustomerDataSet.MaxRandomValue > FlightsPerCompanyDataSet.MinRandomValue)
                            {
                                return "Maximum range of tickets cannot exceed the minimum flight range.";
                            }
                            else
                            {
                                return string.Empty;
                            }
                        }
                    }
                default:
                    return string.Empty;
            }
        }


        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
