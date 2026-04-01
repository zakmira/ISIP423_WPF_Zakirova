using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    // Частичный класс для расширения свойств BasePart
    // Это позволяет добавлять свойства, которые не будут перезаписаны при обновлении модели из БД
    public partial class BasePart
    {
        // Свойство для отображения полной информации о компоненте
        public string FullInfo
        {
            get
            {
                var info = new StringBuilder();
                info.Append(Name);

                if (!string.IsNullOrEmpty(SocketName))
                    info.Append($" | Сокет: {SocketName}");

                if (!string.IsNullOrEmpty(FormFactorName))
                    info.Append($" | Форм-фактор: {FormFactorName}");

                if (!string.IsNullOrEmpty(MemoryTypeName))
                    info.Append($" | Память: {MemoryTypeName}");

                if (PowerConsumption.HasValue)
                    info.Append($" | Потребление: {PowerConsumption}W");

                if (RecommendPower.HasValue)
                    info.Append($" | Рекомендованный БП: {RecommendPower}W");

                return info.ToString();
            }
        }

        // Свойство для краткого отображения цены
        public string PriceText
        {
            get { return $"{Price:C0}"; }
        }
    }
}
