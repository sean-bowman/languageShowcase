# Aerospace Dashboard - C# WPF

A Windows desktop application providing aerospace engineering tools with visual interfaces.

## Overview

This WPF application combines several aerospace engineering tools into a unified dashboard:
- **ISA Calculator**: Compute atmospheric properties at any altitude
- **Hohmann Transfer Calculator**: Calculate orbital maneuver delta-v requirements
- **Unit Converter**: Convert between common aerospace units
- **Atmospheric Charts**: Visualize temperature and pressure vs altitude

## Why C#/WPF?

C# with WPF is the standard for aerospace ground control software:
- **Mission Planner** (ArduPilot's GCS) is written in C#
- **Rich UI capabilities** with XAML
- **Strong .NET ecosystem** for data visualization
- **MVVM pattern** for testable, maintainable code

## Prerequisites

- **Visual Studio 2022** with ".NET desktop development" workload
- **.NET 8.0 SDK** (included with Visual Studio)

## Building and Running

### Visual Studio
1. Open `AerospaceDashboard.sln` in Visual Studio
2. Press F5 to build and run

### Command Line
```bash
cd C#/AerospaceDashboard

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

## Features

### ISA Atmosphere Calculator
- Input altitude in meters
- Calculates: Temperature, Pressure, Density, Speed of Sound
- Based on ISO 2533:1975 standard atmosphere

### Hohmann Transfer Calculator
- Input initial and final orbit altitudes
- Calculates: First burn Δv, Second burn Δv, Total Δv, Transfer time
- Uses vis-viva equation for orbital mechanics

### Unit Converter
- Convert between: meters, feet, kilometers, miles, nautical miles
- Instant conversion as you type

### Atmospheric Charts
- Temperature vs Altitude profile
- Pressure vs Altitude profile (logarithmic scale)
- Powered by LiveCharts2

## Project Structure

```
C#/
├── AerospaceDashboard.sln       # Solution file
├── README.md                    # This file
├── AerospaceDashboard/
│   ├── AerospaceDashboard.csproj
│   ├── App.xaml                 # Application entry
│   ├── MainWindow.xaml          # Main UI
│   ├── Models/
│   │   ├── CelestialBody.cs     # Celestial body definition
│   │   ├── Orbit.cs             # Orbit representation
│   │   └── AtmosphericLayer.cs  # Atmospheric data types
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs     # MVVM base class
│   │   └── MainViewModel.cs     # Main window logic
│   ├── Services/
│   │   ├── IsaCalculator.cs     # ISA calculations
│   │   └── HohmannCalculator.cs # Orbital mechanics
│   └── Resources/
│       └── Styles.xaml          # UI styling
└── references/
    └── REFERENCES.md            # Technical references
```

## Key C# Concepts Demonstrated

1. **MVVM Pattern** - Model-View-ViewModel architecture
2. **Data Binding** - Two-way binding with INotifyPropertyChanged
3. **XAML** - Declarative UI definition
4. **Commands** - ICommand for button actions
5. **Records** - Immutable data types (C# 9+)
6. **Nullable Reference Types** - Null safety (C# 8+)

## Dependencies

- **LiveChartsCore.SkiaSharpView.WPF** - Modern charting library

## Screenshots

The dashboard features a dark aerospace-themed UI with:
- Three calculator panels in the top row
- Two atmospheric charts in the bottom row
- Real-time updates as values change

## References

See [references/REFERENCES.md](references/REFERENCES.md) for technical references.

- [WPF Documentation](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)
- [LiveCharts2](https://lvcharts.com/)
- [ArduPilot Mission Planner](https://github.com/ArduPilot/MissionPlanner)
