using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// Логика взаимодействия для BuildPage.xaml
    /// </summary>
    public partial class BuildPage : Page
    {
        public class PartDisplay
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int ManufacturerId { get; set; }
            public string ManufacturerName { get; set; }
            public int PartTypeId { get; set; }
            public string PartTypeName { get; set; }
        }

        private int currentPartTypeId;
        private List<PartDisplay> currentParts;

        private PartDisplay selectedCPU;
        private PartDisplay selectedGPU;
        private PartDisplay selectedRAM;
        private PartDisplay selectedMotherboard;
        private PartDisplay selectedCase;
        private PartDisplay selectedPSU;
        private PartDisplay selectedCooler;
        private PartDisplay selectedStorage;

        public BuildPage()
        {
            InitializeComponent();
            LoadManufacturers();
            LoadParts(1);
        }

        private void LoadManufacturers()
        {
            var manufacturers = Core.Context.manufacturer_.ToList();
            ManufacturerFilter.Items.Add("Все");
            foreach (var m in manufacturers)
            {
                ManufacturerFilter.Items.Add(m.name);
            }
            ManufacturerFilter.SelectedIndex = 0;
        }

        private void LoadParts(int partTypeId)
        {
            currentPartTypeId = partTypeId;

            var query = from bp in Core.Context.basepart_
                        where bp.parttypeid == partTypeId
                        select new PartDisplay
                        {
                            Id = bp.id,
                            Name = bp.name,
                            Price = bp.price,
                            ManufacturerId = bp.manufacturerid,
                            ManufacturerName = bp.manufacturer_.name,
                            PartTypeId = bp.parttypeid,
                            PartTypeName = bp.parttype_.name
                        };

            currentParts = query.ToList();
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (currentParts == null) return;

            var filtered = currentParts.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                filtered = filtered.Where(p => p.Name.ToLower().Contains(SearchBox.Text.ToLower()));
            }

            if (ManufacturerFilter.SelectedItem != null && ManufacturerFilter.SelectedItem.ToString() != "Все")
            {
                string selectedManufacturer = ManufacturerFilter.SelectedItem.ToString();
                filtered = filtered.Where(p => p.ManufacturerName == selectedManufacturer);
            }

            PartsList.ItemsSource = filtered.ToList();
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            switch (btn.Content.ToString())
            {
                case "Процессор": LoadParts(1); break;
                case "Видеокарта": LoadParts(2); break;
                case "Оперативная память": LoadParts(3); break;
                case "Материнская плата": LoadParts(4); break;
                case "Корпус": LoadParts(5); break;
                case "Блок питания": LoadParts(6); break;
                case "Кулер": LoadParts(7); break;
                case "Накопитель": LoadParts(8); break;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ManufacturerFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void PartsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PartsList.SelectedItem == null) return;

            PartDisplay selected = (PartDisplay)PartsList.SelectedItem;

            switch (currentPartTypeId)
            {
                case 1: selectedCPU = selected; break;
                case 2: selectedGPU = selected; break;
                case 3: selectedRAM = selected; break;
                case 4: selectedMotherboard = selected; break;
                case 5: selectedCase = selected; break;
                case 6: selectedPSU = selected; break;
                case 7: selectedCooler = selected; break;
                case 8: selectedStorage = selected; break;
            }

            UpdateSelectedPartsList();
            UpdateTotalPrice();
            CheckCompatibility();
        }

        private void UpdateSelectedPartsList()
        {
            var selectedParts = new List<PartDisplay>();
            if (selectedCPU != null) selectedParts.Add(selectedCPU);
            if (selectedGPU != null) selectedParts.Add(selectedGPU);
            if (selectedRAM != null) selectedParts.Add(selectedRAM);
            if (selectedMotherboard != null) selectedParts.Add(selectedMotherboard);
            if (selectedCase != null) selectedParts.Add(selectedCase);
            if (selectedPSU != null) selectedParts.Add(selectedPSU);
            if (selectedCooler != null) selectedParts.Add(selectedCooler);
            if (selectedStorage != null) selectedParts.Add(selectedStorage);

            SelectedPartsList.ItemsSource = selectedParts;
        }

        private void UpdateTotalPrice()
        {
            decimal total = 0;
            if (selectedCPU != null) total += selectedCPU.Price;
            if (selectedGPU != null) total += selectedGPU.Price;
            if (selectedRAM != null) total += selectedRAM.Price;
            if (selectedMotherboard != null) total += selectedMotherboard.Price;
            if (selectedCase != null) total += selectedCase.Price;
            if (selectedPSU != null) total += selectedPSU.Price;
            if (selectedCooler != null) total += selectedCooler.Price;
            if (selectedStorage != null) total += selectedStorage.Price;

            TotalPriceText.Text = $"Общая стоимость: {total:C}";
        }

        private void CheckCompatibility()
        {
            string error = "";

            if (selectedCPU != null && selectedMotherboard != null)
            {
                var cpu = Core.Context.cpu_.FirstOrDefault(c => c.id == selectedCPU.Id);
                var mobo = Core.Context.motherboard_.FirstOrDefault(m => m.id == selectedMotherboard.Id);

                if (cpu != null && mobo != null && cpu.socketid != mobo.socketid)
                {
                    error += "Сокет процессора и материнской платы не совместимы!\n";
                }
            }

            if (selectedCPU != null && selectedCooler != null)
            {
                var cpu = Core.Context.cpu_.FirstOrDefault(c => c.id == selectedCPU.Id);

                if (cpu != null)
                {
                    bool socketSupported = Core.Context.socketprocessorcooler_
                        .Any(sc => sc.socketid == cpu.socketid && sc.processorcoolerid == selectedCooler.Id);

                    if (!socketSupported)
                    {
                        error += "Кулер не поддерживает сокет процессора!\n";
                    }
                }
            }

            if (selectedMotherboard != null && selectedCase != null)
            {
                var mobo = Core.Context.motherboard_.FirstOrDefault(m => m.id == selectedMotherboard.Id);

                if (mobo != null)
                {
                    bool formFactorSupported = Core.Context.boardformfactorcase_
                        .Any(bf => bf.caseid == selectedCase.Id && bf.formfactorid == mobo.formfactorid);

                    if (!formFactorSupported)
                    {
                        error += "Корпус не поддерживает форм-фактор материнской платы!\n";
                    }
                }
            }

            if (selectedMotherboard != null && selectedRAM != null)
            {
                var mobo = Core.Context.motherboard_.FirstOrDefault(m => m.id == selectedMotherboard.Id);
                var ram = Core.Context.ram_.FirstOrDefault(r => r.id == selectedRAM.Id);

                if (mobo != null && ram != null && mobo.memorytypeid != ram.memorytypeid)
                {
                    error += "Тип памяти материнской платы и оперативной памяти не совместим!\n";
                }
            }

            if (selectedGPU != null && selectedPSU != null)
            {
                var gpu = Core.Context.gpu_.FirstOrDefault(g => g.id == selectedGPU.Id);
                var psu = Core.Context.powersupply_.FirstOrDefault(p => p.id == selectedPSU.Id);

                if (gpu != null && psu != null && gpu.recommendpower > psu.power)
                {
                    error += "Мощность блока питания недостаточна для видеокарты!\n";
                }
            }

            if (string.IsNullOrEmpty(error))
            {
                CompatibilityText.Text = "Совместимость: OK";
                CompatibilityText.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                CompatibilityText.Text = error;
                CompatibilityText.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AssemblyNameBox.Text) || string.IsNullOrWhiteSpace(AuthorNameBox.Text))
            {
                MessageBox.Show("Введите название сборки и имя автора!");
                return;
            }

            if (CompatibilityText.Text != "Совместимость: OK")
            {
                var result = MessageBox.Show("Есть проблемы с совместимостью. Всё равно сохранить?",
                    "Предупреждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No) return;
            }

            var newAssembly = new assembly_
            {
                name = AssemblyNameBox.Text,
                author = AuthorNameBox.Text
            };

            Core.Context.assembly_.Add(newAssembly);
            Core.Context.SaveChanges();

            List<PartDisplay> selectedParts = new List<PartDisplay>();
            if (selectedCPU != null) selectedParts.Add(selectedCPU);
            if (selectedGPU != null) selectedParts.Add(selectedGPU);
            if (selectedRAM != null) selectedParts.Add(selectedRAM);
            if (selectedMotherboard != null) selectedParts.Add(selectedMotherboard);
            if (selectedCase != null) selectedParts.Add(selectedCase);
            if (selectedPSU != null) selectedParts.Add(selectedPSU);
            if (selectedCooler != null) selectedParts.Add(selectedCooler);
            if (selectedStorage != null) selectedParts.Add(selectedStorage);

            foreach (var part in selectedParts)
            {
                var partAssembly = new partassembly_
                {
                    partid = part.Id,
                    assemblyid = newAssembly.id
                };
                Core.Context.partassembly_.Add(partAssembly);
            }

            Core.Context.SaveChanges();

            MessageBox.Show("Сборка сохранена!");
        }

        public void LoadAssembly(assembly_ assembly)
        {
            var parts = Core.Context.partassembly_
                .Where(pa => pa.assemblyid == assembly.id)
                .Select(pa => pa.basepart_)
                .ToList();

            foreach (var part in parts)
            {
                var partDisplay = new PartDisplay
                {
                    Id = part.id,
                    Name = part.name,
                    Price = part.price,
                    ManufacturerId = part.manufacturerid,
                    ManufacturerName = part.manufacturer_.name,
                    PartTypeId = part.parttypeid,
                    PartTypeName = part.parttype_.name
                };

                switch (part.parttypeid)
                {
                    case 1: selectedCPU = partDisplay; break;
                    case 2: selectedGPU = partDisplay; break;
                    case 3: selectedRAM = partDisplay; break;
                    case 4: selectedMotherboard = partDisplay; break;
                    case 5: selectedCase = partDisplay; break;
                    case 6: selectedPSU = partDisplay; break;
                    case 7: selectedCooler = partDisplay; break;
                    case 8: selectedStorage = partDisplay; break;
                }
            }

            UpdateSelectedPartsList();
            UpdateTotalPrice();
            CheckCompatibility();
        }
    }
}
