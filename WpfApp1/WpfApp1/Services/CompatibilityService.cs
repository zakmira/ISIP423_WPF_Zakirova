using System;
using System.Collections.Generic;
using System.Linq;
using WpfApp1;
using WpfApp1.Models;

namespace WpfApp1
{
    public class CompatibilityService
    {
        private readonly DatabaseService _dbService;

        public CompatibilityService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public CompatibilityResult CheckCompatibility(Dictionary<int, BasePart> selectedParts)
        {
            var result = new CompatibilityResult { IsCompatible = true };

            cpu_ cpu = null;
            motherboard_ motherboard = null;
            processorcooler_ cooler = null;
            case_ case_ = null;
            ram_ ram = null;
            powersupply_ psu = null;
            gpu_ gpu = null;

            foreach (var part in selectedParts.Values)
            {
                switch (part.PartTypeId)
                {
                    case 1: cpu = _dbService.GetCPUById(part.Id); break;
                    case 2: gpu = _dbService.GetGPUById(part.Id); break;
                    case 3: ram = _dbService.GetRAMById(part.Id); break;
                    case 4: motherboard = _dbService.GetMotherboardById(part.Id); break;
                    case 5: case_ = _dbService.GetCaseById(part.Id); break;
                    case 6: psu = _dbService.GetPowerSupplyById(part.Id); break;
                    case 7: cooler = _dbService.GetCoolerById(part.Id); break;
                }
            }

            // 1. Проверка сокета CPU и Motherboard
            if (cpu != null && motherboard != null && cpu.socketid != motherboard.socketid)
            {
                result.IsCompatible = false;
                result.Errors.Add("❌ Процессор и материнская плата имеют разные сокеты!");
            }

            // 2. Проверка сокета CPU и Cooler
            if (cpu != null && cooler != null)
            {
                var compatibleSockets = _dbService.GetSocketCoolerCompatibility(cooler.id);
                if (!compatibleSockets.Contains(cpu.socketid))
                {
                    result.IsCompatible = false;
                    result.Errors.Add("❌ Кулер не совместим с сокетом процессора!");
                }
            }

            // 3. Проверка форм-фактора Motherboard и Case
            if (motherboard != null && case_ != null)
            {
                var compatibleFormFactors = _dbService.GetFormFactorCaseCompatibility(case_.id);
                if (!compatibleFormFactors.Contains(motherboard.formfactorid))
                {
                    result.IsCompatible = false;
                    result.Errors.Add("❌ Материнская плата не совместима с корпусом!");
                }
            }

            // 4. Проверка типа памяти
            if (motherboard != null && ram != null && motherboard.memorytypeid != ram.memorytypeid)
            {
                result.IsCompatible = false;
                result.Errors.Add("❌ Тип памяти материнской платы и ОЗУ не совпадают!");
            }

            // 5. Проверка мощности БП
            if (psu != null && gpu != null && psu.power < gpu.recommendpower)
            {
                result.IsCompatible = false;
                result.Errors.Add($"❌ Мощность БП ({psu.power}W) меньше рекомендуемой ({gpu.recommendpower}W)!");
            }

            return result;
        }
    }
}

