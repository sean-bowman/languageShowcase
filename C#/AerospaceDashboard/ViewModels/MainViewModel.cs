// =============================================================================
// MAIN VIEWMODEL - AEROSPACE DASHBOARD
// =============================================================================
//
// This is the "brain" of the application - it connects the UI (View) to the
// calculation logic (Services) and data (Models).
//
// MVVM ARCHITECTURE RECAP:
// -----------------------
//
//   MainWindow.xaml (View)
//         |
//         | Data Binding (Text="{Binding IsaAltitude}")
//         | Commands (Command="{Binding CalculateIsaCommand}")
//         v
//   MainViewModel (this file)
//         |
//         | Method calls
//         v
//   Services (IsaCalculator, HohmannCalculator)
//         |
//         | Returns
//         v
//   Models (AtmosphericProperties, TransferResult)
//
// WHAT THIS VIEWMODEL PROVIDES:
// ----------------------------
// 1. ISA Calculator: Input altitude, get atmospheric properties
// 2. Hohmann Calculator: Input two altitudes, get transfer parameters
// 3. Unit Converter: Convert between length units
// 4. Charts: Atmospheric property graphs
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. BACKING FIELDS WITH PROPERTIES
//    `private double _isaAltitude = 10000;`
//    `public double IsaAltitude { get => _isaAltitude; set => ... }`
//    Properties wrap fields to add logic (like raising PropertyChanged).
//
// 2. REGIONS
//    `#region ISA Properties` ... `#endregion`
//    Code organization feature. Collapsible in IDE. Purely cosmetic.
//
// 3. SWITCH EXPRESSIONS (C# 8+)
//    `value switch { "meters" => x * 1, "feet" => x * 0.3048, _ => x }`
//    Concise pattern matching syntax. The `_` is the default case.
//
// 4. ARRAY INITIALIZER WITH INFERRED TYPE
//    `new ISeries[] { ... }`
//    Creates an array with the specified elements.
//
// 5. LAMBDA EXPRESSIONS IN OBJECT INITIALIZERS
//    `Mapping = (props, _) => new(props.X, props.Y)`
//    Anonymous function defined inline. The `_` discards unused parameter.
//
// 6. OBJECT INITIALIZER WITH CONSTRUCTOR
//    `new LineSeries<T> { Property = value, ... }`
//    Calls default constructor, then sets properties.
//
// =============================================================================

using System;
using System.Windows.Input;
using AerospaceDashboard.Models;
using AerospaceDashboard.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AerospaceDashboard.ViewModels;

/// <summary>
/// Main ViewModel for the Aerospace Dashboard application.
/// </summary>
/// <remarks>
/// <para>
/// This ViewModel exposes all the data and commands that the MainWindow
/// needs. The View (XAML) binds to properties here, and WPF keeps
/// everything synchronized automatically.
/// </para>
/// <para>
/// C# CONCEPT: Inheritance
/// MainViewModel inherits from ViewModelBase, gaining:
/// - PropertyChanged event
/// - SetProperty helper method
/// - All the MVVM plumbing
/// </para>
/// </remarks>
public class MainViewModel : ViewModelBase
{
    // =========================================================================
    // BACKING FIELDS
    // =========================================================================
    // C# CONCEPT: Private Backing Fields
    // These store the actual data. Properties wrap them to add
    // behavior (like raising PropertyChanged when they change).
    // Convention: prefix with underscore (_) to distinguish from properties.

    // --- ISA Calculator state ---
    private double _isaAltitude = 10000;  // Default: 10 km (in meters)
    private AtmosphericProperties? _isaResult;

    // --- Hohmann Calculator state ---
    private double _initialAltitude = 400000;    // LEO: 400 km
    private double _finalAltitude = 35786000;    // GEO: 35,786 km
    private TransferResult? _transferResult;

    // --- Unit Converter state ---
    private double _inputValue = 1000;
    private string _selectedFromUnit = "meters";
    private string _selectedToUnit = "feet";
    private double _convertedValue;

    // --- Chart data ---
    private ISeries[] _temperatureSeries = Array.Empty<ISeries>();
    private ISeries[] _pressureSeries = Array.Empty<ISeries>();

    // =========================================================================
    // CONSTRUCTOR
    // =========================================================================

    /// <summary>
    /// Initialize the ViewModel with default values and commands.
    /// </summary>
    /// <remarks>
    /// <para>
    /// C# CONCEPT: Command Initialization
    /// Commands are created with RelayCommand, passing the method to execute.
    /// The `_ => Method()` syntax creates a lambda that ignores its parameter
    /// and calls our method.
    /// </para>
    /// <para>
    /// We run initial calculations so the UI shows values immediately,
    /// not blank until the user triggers a calculation.
    /// </para>
    /// </remarks>
    public MainViewModel()
    {
        // Create commands that delegate to our methods
        // The underscore (_) discards the command parameter we don't need
        CalculateIsaCommand = new RelayCommand(_ => CalculateIsa());
        CalculateTransferCommand = new RelayCommand(_ => CalculateTransfer());
        ConvertUnitCommand = new RelayCommand(_ => ConvertUnit());

        // Run initial calculations so UI isn't empty
        CalculateIsa();
        CalculateTransfer();
        ConvertUnit();
        InitializeCharts();
    }

    // =========================================================================
    // ISA CALCULATOR PROPERTIES
    // =========================================================================
    // C# CONCEPT: Regions
    // #region/#endregion create collapsible sections in the IDE.
    // Purely organizational - no effect on compilation.

    #region ISA Properties

    /// <summary>
    /// Altitude for ISA calculations [m].
    /// </summary>
    /// <remarks>
    /// <para>
    /// AEROSPACE: This is the geometric altitude above mean sea level.
    /// Valid range for ISA model: 0 to ~50,000 m.
    /// </para>
    /// <para>
    /// C# CONCEPT: Smart Property Setter
    /// When the value changes:
    /// 1. SetProperty updates the field and raises PropertyChanged
    /// 2. If it actually changed (returns true), we recalculate
    ///
    /// This gives "live update" behavior - change the slider,
    /// calculations update immediately.
    /// </para>
    /// </remarks>
    public double IsaAltitude
    {
        get => _isaAltitude;
        set
        {
            // Only recalculate if value actually changed
            if (SetProperty(ref _isaAltitude, value))
                CalculateIsa();
        }
    }

    /// <summary>
    /// Result of ISA calculation (nullable - may not be calculated yet).
    /// </summary>
    /// <remarks>
    /// C# CONCEPT: Nullable Reference Type (?)
    /// The ? indicates this can be null (no result yet).
    /// The View should handle null gracefully (show "N/A" or blank).
    /// </remarks>
    public AtmosphericProperties? IsaResult
    {
        get => _isaResult;
        set => SetProperty(ref _isaResult, value);
    }

    /// <summary>Command to manually trigger ISA calculation</summary>
    public ICommand CalculateIsaCommand { get; }

    #endregion

    // =========================================================================
    // HOHMANN TRANSFER PROPERTIES
    // =========================================================================

    #region Hohmann Properties

    /// <summary>Starting orbit altitude [m]</summary>
    /// <remarks>
    /// AEROSPACE: Altitude above Earth's surface, not radius from center.
    /// Default is 400 km (typical LEO, like ISS at 420 km).
    /// </remarks>
    public double InitialAltitude
    {
        get => _initialAltitude;
        set => SetProperty(ref _initialAltitude, value);
    }

    /// <summary>Target orbit altitude [m]</summary>
    /// <remarks>
    /// AEROSPACE: Default is 35,786 km (GEO - geostationary orbit).
    /// At this altitude, orbital period = 24 hours.
    /// </remarks>
    public double FinalAltitude
    {
        get => _finalAltitude;
        set => SetProperty(ref _finalAltitude, value);
    }

    /// <summary>Result of Hohmann transfer calculation</summary>
    public TransferResult? TransferResult
    {
        get => _transferResult;
        set => SetProperty(ref _transferResult, value);
    }

    /// <summary>Command to calculate Hohmann transfer</summary>
    public ICommand CalculateTransferCommand { get; }

    #endregion

    // =========================================================================
    // UNIT CONVERTER PROPERTIES
    // =========================================================================

    #region Unit Converter Properties

    /// <summary>Value to convert</summary>
    /// <remarks>
    /// Recalculates automatically when changed (live preview).
    /// </remarks>
    public double InputValue
    {
        get => _inputValue;
        set
        {
            if (SetProperty(ref _inputValue, value))
                ConvertUnit();
        }
    }

    /// <summary>Source unit name</summary>
    public string SelectedFromUnit
    {
        get => _selectedFromUnit;
        set
        {
            if (SetProperty(ref _selectedFromUnit, value))
                ConvertUnit();
        }
    }

    /// <summary>Target unit name</summary>
    public string SelectedToUnit
    {
        get => _selectedToUnit;
        set
        {
            if (SetProperty(ref _selectedToUnit, value))
                ConvertUnit();
        }
    }

    /// <summary>Converted value result</summary>
    public double ConvertedValue
    {
        get => _convertedValue;
        set => SetProperty(ref _convertedValue, value);
    }

    /// <summary>Available length units for selection</summary>
    /// <remarks>
    /// AEROSPACE: These are the most common units:
    /// - meters: SI standard
    /// - feet: US/UK aviation (altitude, runway length)
    /// - kilometers: large distances
    /// - miles: US ground distances
    /// - nautical miles: aviation/maritime (1 nm = 1 arc-minute of latitude)
    /// </remarks>
    public string[] LengthUnits { get; } = { "meters", "feet", "kilometers", "miles", "nautical miles" };

    /// <summary>Command to convert units</summary>
    public ICommand ConvertUnitCommand { get; }

    #endregion

    // =========================================================================
    // CHART PROPERTIES (LiveCharts2)
    // =========================================================================

    #region Chart Properties

    /// <summary>Temperature vs altitude chart data</summary>
    public ISeries[] TemperatureSeries
    {
        get => _temperatureSeries;
        set => SetProperty(ref _temperatureSeries, value);
    }

    /// <summary>Pressure vs altitude chart data</summary>
    public ISeries[] PressureSeries
    {
        get => _pressureSeries;
        set => SetProperty(ref _pressureSeries, value);
    }

    /// <summary>X-axis configuration (altitude)</summary>
    /// <remarks>
    /// C# CONCEPT: Array Initializer with Object Initializer
    /// Creates an array containing one Axis object,
    /// with properties set inline.
    /// </remarks>
    public Axis[] AltitudeAxis { get; } = {
        new Axis
        {
            Name = "Altitude (km)",
            NamePaint = new SolidColorPaint(SKColors.LightGray),
            LabelsPaint = new SolidColorPaint(SKColors.LightGray)
        }
    };

    /// <summary>Y-axis configuration for temperature chart</summary>
    public Axis[] TemperatureAxis { get; } = {
        new Axis
        {
            Name = "Temperature (K)",
            NamePaint = new SolidColorPaint(SKColors.LightGray),
            LabelsPaint = new SolidColorPaint(SKColors.LightGray)
        }
    };

    /// <summary>Y-axis configuration for pressure chart</summary>
    /// <remarks>
    /// C# CONCEPT: Lambda in Property Initializer
    /// `Labeler = value => value.ToString("E2")`
    /// The Labeler is a Func that formats axis labels.
    /// "E2" formats as scientific notation with 2 decimals.
    /// </remarks>
    public Axis[] PressureAxis { get; } = {
        new Axis
        {
            Name = "Pressure (Pa)",
            NamePaint = new SolidColorPaint(SKColors.LightGray),
            LabelsPaint = new SolidColorPaint(SKColors.LightGray),
            Labeler = value => value.ToString("E2")  // Scientific notation
        }
    };

    #endregion

    // =========================================================================
    // CALCULATION METHODS
    // =========================================================================

    #region Methods

    /// <summary>
    /// Calculate atmospheric properties at the current altitude.
    /// </summary>
    private void CalculateIsa()
    {
        IsaResult = IsaCalculator.GetProperties(_isaAltitude);
    }

    /// <summary>
    /// Calculate Hohmann transfer between initial and final altitudes.
    /// </summary>
    private void CalculateTransfer()
    {
        TransferResult = HohmannCalculator.Calculate(_initialAltitude, _finalAltitude);
    }

    /// <summary>
    /// Convert between length units.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ALGORITHM: Convert to meters first (canonical unit), then to target.
    /// This avoids needing N*N conversion factors for N units.
    /// </para>
    /// <para>
    /// C# CONCEPT: Switch Expression (C# 8+)
    /// A concise way to map input to output based on patterns.
    ///
    /// Syntax: `input switch { pattern1 => result1, pattern2 => result2, _ => default }`
    ///
    /// The `_` is the discard pattern - matches anything (default case).
    /// </para>
    /// </remarks>
    private void ConvertUnit()
    {
        // STEP 1: Convert input to meters (canonical unit)
        double meters = _selectedFromUnit switch
        {
            "meters" => _inputValue,
            "feet" => _inputValue * 0.3048,           // 1 ft = 0.3048 m (exact)
            "kilometers" => _inputValue * 1000,
            "miles" => _inputValue * 1609.344,        // 1 mi = 1609.344 m (exact)
            "nautical miles" => _inputValue * 1852,   // 1 nm = 1852 m (exact)
            _ => _inputValue  // Default: assume meters
        };

        // STEP 2: Convert from meters to target unit
        ConvertedValue = _selectedToUnit switch
        {
            "meters" => meters,
            "feet" => meters / 0.3048,
            "kilometers" => meters / 1000,
            "miles" => meters / 1609.344,
            "nautical miles" => meters / 1852,
            _ => meters
        };
    }

    /// <summary>
    /// Initialize chart data with atmospheric profile.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Generates 50 data points from 0 to 50 km altitude,
    /// then creates LiveCharts series for temperature and pressure.
    /// </para>
    /// <para>
    /// C# CONCEPT: Lambda with Tuple Deconstruction
    /// `(props, _) => new(props.X, props.Y)`
    ///
    /// The Mapping property uses a lambda that receives (item, index).
    /// We use _ to discard the index we don't need.
    /// `new(x, y)` creates a point using target-typed new.
    /// </para>
    /// </remarks>
    private void InitializeCharts()
    {
        // Generate atmospheric data from 0 to 50 km
        var profile = IsaCalculator.GenerateProfile(0, 50000, 50);

        // Temperature vs Altitude chart
        TemperatureSeries = new ISeries[]
        {
            new LineSeries<AtmosphericProperties>
            {
                Values = profile,
                // Map each data point to (X=altitude, Y=temperature)
                Mapping = (props, _) => new(props.AltitudeKm, props.Temperature),
                Name = "Temperature",
                Stroke = new SolidColorPaint(SKColors.Orange) { StrokeThickness = 2 },
                Fill = null,        // No fill under the line
                GeometrySize = 0    // No point markers
            }
        };

        // Pressure vs Altitude chart
        PressureSeries = new ISeries[]
        {
            new LineSeries<AtmosphericProperties>
            {
                Values = profile,
                Mapping = (props, _) => new(props.AltitudeKm, props.Pressure),
                Name = "Pressure",
                Stroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 2 },
                Fill = null,
                GeometrySize = 0
            }
        };
    }

    #endregion
}
