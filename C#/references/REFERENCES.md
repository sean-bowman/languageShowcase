# Aerospace Dashboard - References

## C# and WPF Resources

### Official Documentation
- **WPF Overview:** https://docs.microsoft.com/en-us/dotnet/desktop/wpf/
- **.NET 8 Documentation:** https://docs.microsoft.com/en-us/dotnet/core/whats-new/dotnet-8

### MVVM Pattern
Model-View-ViewModel is the standard architecture for WPF applications:

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│      View       │────▶│   ViewModel     │────▶│     Model       │
│    (XAML)       │◀────│   (C# class)    │◀────│   (Data)        │
└─────────────────┘     └─────────────────┘     └─────────────────┘
     Data Binding         INotifyPropertyChanged    Business Logic
```

- **View**: XAML UI (MainWindow.xaml)
- **ViewModel**: Presentation logic (MainViewModel.cs)
- **Model**: Domain objects (Orbit.cs, CelestialBody.cs)

### Data Binding
WPF data binding connects UI elements to ViewModel properties:

```xml
<TextBox Text="{Binding IsaAltitude, UpdateSourceTrigger=PropertyChanged}"/>
```

The `INotifyPropertyChanged` interface enables automatic UI updates:

```csharp
public event PropertyChangedEventHandler? PropertyChanged;

protected void OnPropertyChanged(string propertyName)
{
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
```

## Aerospace Ground Software Examples

### Mission Planner (ArduPilot)
- **GitHub:** https://github.com/ArduPilot/MissionPlanner
- **Language:** C# / WinForms
- **Purpose:** Ground control station for UAVs

### Key Features of GCS Software
- Real-time telemetry display
- Mission planning and waypoint editing
- Parameter configuration
- Flight data logging and analysis

## Charting Library

### LiveCharts2
- **Website:** https://lvcharts.com/
- **GitHub:** https://github.com/beto-rodriguez/LiveCharts2
- **NuGet:** LiveChartsCore.SkiaSharpView.WPF

Features:
- Hardware-accelerated rendering (SkiaSharp)
- Real-time updates
- Cross-platform support
- Extensive chart types

## ISA Calculations Reference

Same formulas as the FORTRAN implementation:

| Layer | Altitude | Lapse Rate | Formula |
|-------|----------|------------|---------|
| Troposphere | 0-11 km | -6.5 K/km | T = T₀ + L×h |
| Tropopause | 11-20 km | 0 K/km | T = 216.65 K |
| Stratosphere | 20-32 km | +1.0 K/km | T = T₂₀ + L×(h-20) |

Pressure formulas:
- Gradient layer: P = P₀ × (T/T₀)^(-g/(L×R))
- Isothermal layer: P = P₁ × exp(-g×Δh/(R×T))

## Hohmann Transfer Reference

Same formulas as the C++ implementation:

```
Δv₁ = √(μ/r₁) × (√(2r₂/(r₁+r₂)) - 1)
Δv₂ = √(μ/r₂) × (1 - √(2r₁/(r₁+r₂)))
t = π × √(a³/μ)
```

## Modern C# Features Used

| Feature | Version | Usage |
|---------|---------|-------|
| Records | C# 9 | Immutable data types |
| Nullable reference types | C# 8 | `string?` null safety |
| Target-typed new | C# 9 | `new()` without type |
| File-scoped namespaces | C# 10 | Reduced nesting |
| Collection expressions | C# 12 | `[1, 2, 3]` syntax |

## Color Palette

The dashboard uses an aerospace-inspired dark theme:

| Name | Hex | Usage |
|------|-----|-------|
| Background | #0D1B2A | Main background |
| Surface | #1B2838 | Panel backgrounds |
| Primary | #1E3A5F | Borders |
| Secondary | #2E5A8F | Buttons |
| Accent | #4A90D9 | Highlights |
| Text | #E0E0E0 | Primary text |
| Text Secondary | #A0A0A0 | Labels |

## Additional Resources

- **XAML Tutorial:** https://docs.microsoft.com/en-us/dotnet/desktop/wpf/xaml/
- **C# Language Reference:** https://docs.microsoft.com/en-us/dotnet/csharp/
- **SkiaSharp:** https://github.com/mono/SkiaSharp
