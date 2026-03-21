using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO.Ports;
using System.Linq;
using System.Xml.Linq;
using WpfApp1;
using WpfApp1.Models;

namespace WpfApp1
{
    public class DatabaseService
    {
        public List<BasePart> GetPartsByType(int partTypeId)
        {
            return Core.Context.basepart_
                .Where(p => p.parttypeid == partTypeId)
                .Join(Core.Context.manufacturer_,
                      p => p.manufacturerid,
                      m => m.id,
                      (p, m) => new { Part = p, Manufacturer = m })
                .Join(Core.Context.parttype_,
                      pm => pm.Part.parttypeid,
                      pt => pt.id,
                      (pm, pt) => new BasePart
                      {
                          Id = pm.Part.id,
                          Name = pm.Part.name,
                          ManufacturerId = pm.Part.manufacturerid,
                          ManufacturerName = pm.Manufacturer.name,
                          PartTypeId = pm.Part.parttypeid,
                          PartTypeName = pt.name,
                          Image = pm.Part.image,
                          Price = (decimal)pm.Part.price
                      })
                .ToList();
        }

        public List<parttype_> GetPartTypes()
        {
            return Core.Context.parttype_.ToList();
        }

        public List<manufacturer_> GetManufacturers()
        {
            return Core.Context.manufacturer_.ToList();
        }

        public cpu_ GetCPUById(int id)
        {
            return Core.Context.cpu_.FirstOrDefault(c => c.id == id);
        }

        public motherboard_ GetMotherboardById(int id)
        {
            return Core.Context.motherboard_.FirstOrDefault(m => m.id == id);
        }

        public processorcooler_ GetCoolerById(int id)
        {
            return Core.Context.processorcooler_.FirstOrDefault(c => c.id == id);
        }

        public case_ GetCaseById(int id)
        {
            return Core.Context.case_.FirstOrDefault(c => c.id == id);
        }

        public ram_ GetRAMById(int id)
        {
            return Core.Context.ram_.FirstOrDefault(r => r.id == id);
        }

        public powersupply_ GetPowerSupplyById(int id)
        {
            return Core.Context.powersupply_.FirstOrDefault(p => p.id == id);
        }

        public gpu_ GetGPUById(int id)
        {
            return Core.Context.gpu_.FirstOrDefault(g => g.id == id);
        }

        public List<int> GetSocketCoolerCompatibility(int coolerId)
        {
            return Core.Context.socketprocessorcooler_
                .Where(spc => spc.processorcoolerid == coolerId)
                .Select(spc => spc.socketid)
                .ToList();
        }

        public List<int> GetFormFactorCaseCompatibility(int caseId)
        {
            return Core.Context.boardformfactorcase_
                .Where(bfc => bfc.caseid == caseId)
                .Select(bfc => bfc.formfactorid)
                .ToList();
        }

        public void SaveAssembly(Assembly assembly)
        {
            var newAssembly = new assembly_
            {
                name = assembly.Name,
                author = assembly.Author
            }
            ;

            Core.Context.assembly_.Add(newAssembly);
            Core.Context.SaveChanges();

            foreach (var part in assembly.Parts)
            {
                Core.Context.partassembly_.Add(new partassembly_
                {
                    partid = part.Id,
                    assemblyid = newAssembly.id
                });
            }

            Core.Context.SaveChanges();
        }

        public List<Assembly> GetAllAssemblies()
        {
            var assemblies = Core.Context.assembly_.ToList();
            var result = new List<Assembly>();

            foreach (var asm in assemblies)
            {
                var parts = Core.Context.partassembly_
                    .Where(pa => pa.assemblyid == asm.id)
                    .Join(Core.Context.basepart_,
                          pa => pa.partid,
                          bp => bp.id,
                          (pa, bp) => new BasePart
                          {
                              Id = bp.id,
                              Name = bp.name,
                              Price = (decimal)bp.price
                          })
                    .ToList();

                result.Add(new Assembly
                {
                    Id = asm.id,
                    Name = asm.name,
                    Author = asm.author,
                    Parts = parts
                });
            }

            return result;
        }
    }
}

