using System;
using System.Windows;

namespace CarService.Entities
{
    // Вычисляемые свойства услуги для привязки в списке услуг (ServicesPage).
    // Сгенерированная часть класса находится в Service.cs и перезаписывается из модели.
    public partial class Service
    {
        // Размер скидки в процентах, 0 если скидки нет.
        public double DiscountPercent => Math.Round((Discount ?? 0) * 100, 2);

        public bool HasDiscount => DiscountPercent > 0;

        public string DiscountText => HasDiscount ? $"* скидка {DiscountPercent:0.##} %" : "";

        public double CostWithDiscount => (double)Cost * (1.0 - (Discount ?? 0));

        public string TotalCost => $"{(HasDiscount ? CostWithDiscount : (double)Cost):N2} рублей за {DurationInSeconds / 60} минут";

        public Visibility DiscountVisibility => HasDiscount ? Visibility.Visible : Visibility.Collapsed;

        // Фон карточки: светло-зеленый для услуг со скидкой.
        public string BackColor => HasDiscount ? "#D1FFD1" : "#FFFFE1";

        // Кнопки "Редактировать" и "Удалить" видит только администратор.
        public Visibility AdminControlsVisibility => App.IsAdmin ? Visibility.Visible : Visibility.Collapsed;
    }
}
