using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightProjectDBGenerator
{
    class DataSetUserControlViewModel : IDataErrorInfo
    {
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
                            if (TicketsPerCustomerDataSet.NonRandomValue > FlightsPerCompanyDataSet.NonRandomValue)
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
                            if (TicketsPerCustomerDataSet.NonRandomValue > FlightsPerCompanyDataSet.MinRandomValue)
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

        public string Error
        {
            get
            {
                return string.Empty;
            }
        }
    }
}
