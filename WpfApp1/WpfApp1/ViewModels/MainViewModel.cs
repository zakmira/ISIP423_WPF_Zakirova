using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using WpfApp1;
using WpfApp1.Models;

namespace WpfApp1
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private readonly CompatibilityService _compatService;

        private ObservableCollection<BasePart> _availableParts;
        private ObservableCollection<BasePart> _selectedParts;
        private ObservableCollection<parttype_> _partTypes;
        private ObservableCollection<manufacturer_> _manufacturers;
        private int _selectedPartTypeId;
        private int? _selectedManufacturerId;
        private string _searchText;
        private string _assemblyName;
        private string _assemblyAuthor;
        private string _compatibilityStatus;
        private bool _isCompatible;
        private ObservableCollection<Assembly> _savedAssemblies;

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            _compatService = new CompatibilityService(_dbService);

            _availableParts = new ObservableCollection<BasePart>();
            _selectedParts = new ObservableCollection<BasePart>();
            _partTypes = new ObservableCollection<parttype_>();
            _manufacturers = new ObservableCollection<manufacturer_>();
            _savedAssemblies = new ObservableCollection<Assembly>();

            LoadPartTypes();
            LoadManufacturers();
            LoadSavedAssemblies();
            if (_partTypes.Count > 0)
            {
                SelectedPartTypeId = _partTypes[0].id;
            }
        }

        public ObservableCollection<BasePart> AvailableParts
        {
            get => _availableParts;
            set { _availableParts = value; OnPropertyChanged(); }
        }

        public ObservableCollection<BasePart> SelectedParts
        {
            get => _selectedParts;
            set { _selectedParts = value; OnPropertyChanged(); UpdateTotalPrice(); }
        }

        public ObservableCollection<parttype_> PartTypes
        {
            get => _partTypes;
            set { _partTypes = value; OnPropertyChanged(); }
        }

        public ObservableCollection<manufacturer_> Manufacturers
        {
            get => _manufacturers;
            set { _manufacturers = value; OnPropertyChanged(); }
        }

        public int SelectedPartTypeId
        {
            get => _selectedPartTypeId;
            set { _selectedPartTypeId = value; OnPropertyChanged(); LoadParts(); }
        }

        public int? SelectedManufacturerId
        {
            get => _selectedManufacturerId;
            set { _selectedManufacturerId = value; OnPropertyChanged(); FilterParts(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterParts(); }
        }

        public string AssemblyName
        {
            get => _assemblyName;
            set { _assemblyName = value; OnPropertyChanged(); }
        }

        public string AssemblyAuthor
        {
            get => _assemblyAuthor;
            set { _assemblyAuthor = value; OnPropertyChanged(); }
        }

        public string CompatibilityStatus
        {
            get => _compatibilityStatus;
            set { _compatibilityStatus = value; OnPropertyChanged(); }
        }

        public bool IsCompatible
        {
            get => _isCompatible;
            set { _isCompatible = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Assembly> SavedAssemblies
        {
            get => _savedAssemblies;
            set { _savedAssemblies = value; OnPropertyChanged(); }
        }

        public decimal TotalPrice { get; private set; }

        public ICommand SelectPartCommand => new RelayCommand(SelectPart);
        public ICommand RemovePartCommand => new RelayCommand<BasePart>(RemovePart);
        public ICommand SaveAssemblyCommand => new RelayCommand(SaveAssembly);
        public ICommand CheckCompatibilityCommand => new RelayCommand(CheckCompatibility);
        public ICommand LoadAssemblyCommand => new RelayCommand<Assembly>(LoadAssembly);

        private void LoadPartTypes()
        {
            foreach (var type in _dbService.GetPartTypes())
                PartTypes.Add(type);
        }

        private void LoadManufacturers()
        {
            foreach (var m in _dbService.GetManufacturers())
                Manufacturers.Add(m);
        }

        private void LoadParts()
        {
            var parts = _dbService.GetPartsByType(SelectedPartTypeId);
            AvailableParts.Clear();
            foreach (var p in parts)
                AvailableParts.Add(p);
        }

        private void FilterParts()
        {
            var parts = _dbService.GetPartsByType(SelectedPartTypeId);

            if (SelectedManufacturerId.HasValue && SelectedManufacturerId.Value > 0)
                parts = parts.Where(p => p.ManufacturerId == SelectedManufacturerId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(SearchText))
                parts = parts.Where(p => p.Name.Contains(SearchText)).ToList();

            AvailableParts.Clear();
            foreach (var p in parts)
                AvailableParts.Add(p);
        }

        private void SelectPart(object parameter)
        {
            var part = parameter as BasePart;
            if (part != null && !SelectedParts.Any(p => p.PartTypeId == part.PartTypeId))
            {
                // Загружаем дополнительные характеристики для выбранной детали
                LoadPartDetails(part);
                SelectedParts.Add(part);
                CheckCompatibility();
            }
        }

        private void LoadPartDetails(BasePart part)
        {
            // Загружаем характеристики в зависимости от типа детали
            switch (part.PartTypeId)
            {
                case 1: // CPU
                    var cpu = _dbService.GetCPUById(part.Id);
                    if (cpu != null)
                    {
                        var socket = Core.Context.socket_.FirstOrDefault(s => s.id == cpu.socketid);
                        part.SocketName = socket?.name;
                        part.PowerConsumption = cpu.thermalpower;
                    }
                    break;
                case 2: // GPU
                    var gpu = _dbService.GetGPUById(part.Id);
                    if (gpu != null)
                    {
                        part.RecommendPower = gpu.recommendpower;
                    }
                    break;
                case 3: // RAM
                    var ram = _dbService.GetRAMById(part.Id);
                    if (ram != null)
                    {
                        var memType = Core.Context.memorytype_.FirstOrDefault(m => m.id == ram.memorytypeid);
                        part.MemoryTypeName = memType?.name;
                    }
                    break;
                case 4: // Motherboard
                    var mobo = _dbService.GetMotherboardById(part.Id);
                    if (mobo != null)
                    {
                        var socket = Core.Context.socket_.FirstOrDefault(s => s.id == mobo.socketid);
                        var formFactor = Core.Context.formfactor_.FirstOrDefault(f => f.id == mobo.formfactorid);
                        var memType = Core.Context.memorytype_.FirstOrDefault(m => m.id == mobo.memorytypeid);
                        part.SocketName = socket?.name;
                        part.FormFactorName = formFactor?.name;
                        part.MemoryTypeName = memType?.name;
                    }
                    break;
                case 5: // Case
                    var case_ = _dbService.GetCaseById(part.Id);
                    if (case_ != null)
                    {
                        var size = Core.Context.casesize_.FirstOrDefault(s => s.id == case_.sizeid);
                        part.FormFactorName = size?.name;
                    }
                    break;
                case 6: // PowerSupply
                    var psu = _dbService.GetPowerSupplyById(part.Id);
                    if (psu != null)
                    {
                        part.PowerConsumption = psu.power;
                    }
                    break;
            }
        }

        private void RemovePart(BasePart part)
        {
            if (part != null)
            {
                SelectedParts.Remove(part);
                CheckCompatibility();
            }
        }

        private void UpdateTotalPrice()
        {
            TotalPrice = SelectedParts.Sum(p => p.Price);
            OnPropertyChanged(nameof(TotalPrice));
        }

        private void CheckCompatibility()
        {
            var selectedDict = SelectedParts.ToDictionary(p => p.PartTypeId, p => p);
            var result = _compatService.CheckCompatibility(selectedDict);

            IsCompatible = result.IsCompatible;
            CompatibilityStatus = result.IsCompatible
                ? "✅ Все компоненты совместимы!"
                : string.Join("\n", result.Errors);
        }

        private void SaveAssembly()
        {
            if (SelectedParts.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы один компонент!");
                return;
            }

            if (!IsCompatible)
            {
                MessageBox.Show("Есть проблемы с совместимостью!");
                return;
            }

            var assembly = new Assembly
            {
                Name = AssemblyName,
                Author = AssemblyAuthor,
                Parts = SelectedParts.ToList()
            };

            _dbService.SaveAssembly(assembly);
            LoadSavedAssemblies();
            MessageBox.Show("Сборка сохранена!");
        }

        private void LoadSavedAssemblies()
        {
            var assemblies = _dbService.GetAllAssemblies();
            SavedAssemblies.Clear();
            foreach (var a in assemblies)
                SavedAssemblies.Add(a);
        }

        private void LoadAssembly(Assembly assembly)
        {
            if (assembly != null)
            {
                SelectedParts.Clear();
                foreach (var part in assembly.Parts)
                    SelectedParts.Add(part);

                AssemblyName = assembly.Name;
                AssemblyAuthor = assembly.Author;
                CheckCompatibility();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}


