using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CarService.Entities
{
    public partial class Service
    {
        public string DiscountText
        {
            get
            {
                if (Discount == 0 || Discount == null)
                    return "";
                else return $"* скидка {Discount * 100} %";
            }
        }

        public string TotalCost
        {
            get
            {
                if (Discount  == 0 || Discount == null)
                {
                    return $"{Cost:N2} рублей за {DurationInSeconds / 60} минут";
                }
                else
                {
                    return $"{CostWithDiscount:N2} рублей за {DurationInSeconds / 60} минут";
                }
            }
        }

        public double CostWithDiscount
        {
            get
            {
                if (Discount == 0 || Discount == null)
                {
                    return (double)Cost;
                }
                else
                {
                    var costWithDiscount = (double)Cost * (1.00 - Discount);
                    return costWithDiscount.Value;
                }
            }
        }

        public Visibility DiscountVisibility
        {
            get
            {
                if (Discount == 0 || Discount == null)
                {
                    return Visibility.Collapsed;
                }
                else 
                { 
                    return Visibility.Visible; 
                }
            }
        }

        public string BackColor
        {
            get
            {
                if (Discount == 0 || Discount == null)
                {
                    return "#FFFFE1";
                }
                else
                {
                    return "#D1FFD1";
                }
            }
        }

        public string AdminControlsVisibility
        {
            get
            {
                // 1 - админ, 2 - пользователь.
                if (App.CurrentUser.RoleId == 1)
                {
                    return "Visible";
                }
                else
                {
                    return "Collapsed";
                }
            }
        }
    }
}
