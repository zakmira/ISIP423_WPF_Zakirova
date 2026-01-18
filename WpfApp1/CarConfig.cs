using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class CarConfig
    {
        // Модель
        public string SelectedModel { get; set; } = "Toyota Camry";
        public decimal ModelPrice { get; set; } = 2500000;

        // Двигатель
        public string SelectedEngine { get; set; } = "ДВС";
        public decimal EnginePrice { get; set; } = 250000;

        // Цвет
        public string SelectedColor { get; set; } = "Белый";
        public decimal ColorPrice { get; set; } = 0;

        // Опции
        public string SelectedOption { get; set; } = "Мультимедиа";
        public decimal OptionPrice { get; set; } = 30000;

        // Итоговая цена
        public decimal TotalPrice => ModelPrice + EnginePrice + ColorPrice + OptionPrice;
    }
}
