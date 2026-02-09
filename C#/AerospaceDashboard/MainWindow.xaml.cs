// =============================================================================
// MAIN WINDOW CODE-BEHIND
// =============================================================================
//
// C# / WPF CONCEPT: Code-Behind Files
// ====================================
//
// Every XAML file can have a corresponding .xaml.cs "code-behind" file.
// The two files together define one partial class:
//
//   MainWindow.xaml     <- UI layout (what it looks like)
//   MainWindow.xaml.cs  <- C# logic (how it behaves) - THIS FILE
//
// WHAT GOES WHERE:
// ----------------
//
//   MainWindow.xaml:
//   - Window size, title, icon
//   - Layout (Grid, StackPanel, etc.)
//   - Controls (Button, TextBox, etc.)
//   - Data bindings (Text="{Binding PropertyName}")
//   - Command bindings (Command="{Binding CommandName}")
//   - Styles and visual properties
//
//   MainWindow.xaml.cs (this file):
//   - Constructor with InitializeComponent()
//   - Event handlers (if not using MVVM commands)
//   - Code that requires C# (can't be done in XAML)
//
// MVVM BEST PRACTICE:
// ------------------
// With MVVM, the code-behind is often nearly empty!
// - UI logic goes in ViewModel (data binding)
// - User actions go through Commands (ICommand)
// - View only handles pure UI concerns
//
// This makes the View (XAML + code-behind) "dumb" - it just displays
// what the ViewModel tells it. All the "thinking" happens in ViewModel.
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. PARTIAL CLASS (paired with XAML)
//    `public partial class MainWindow`
//    This file + MainWindow.xaml = one complete class.
//
// 2. INHERITANCE FROM WINDOW
//    `MainWindow : Window`
//    Window is the base class for all WPF windows.
//    Provides title bar, resize, minimize, maximize, close, etc.
//
// 3. INITIALIZECOMPONENT()
//    This auto-generated method parses the XAML and creates all controls.
//    MUST be called in constructor, before accessing any XAML-defined controls.
//
// =============================================================================

using System.Windows;

namespace AerospaceDashboard;

/// <summary>
/// Main application window.
/// </summary>
/// <remarks>
/// <para>
/// C# CONCEPT: XAML Code-Behind
/// This class is the "code-behind" for MainWindow.xaml.
/// Together they form one partial class representing the window.
/// </para>
/// <para>
/// MVVM PATTERN:
/// In MVVM, the code-behind is minimal. All logic lives in the ViewModel.
/// The View (this window) just displays data and forwards user actions.
///
/// The connection is made via DataContext (set in XAML):
///   &lt;Window.DataContext&gt;
///       &lt;vm:MainViewModel /&gt;
///   &lt;/Window.DataContext&gt;
///
/// After that, all {Binding ...} expressions look up properties
/// on the MainViewModel.
/// </para>
/// </remarks>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initialize the window and load XAML content.
    /// </summary>
    /// <remarks>
    /// <para>
    /// C# CONCEPT: InitializeComponent()
    /// This method is auto-generated from MainWindow.xaml.
    /// It parses the XAML and creates all the UI controls.
    ///
    /// MUST be called:
    /// - In the constructor
    /// - Before accessing any controls defined in XAML
    /// </para>
    /// <para>
    /// After InitializeComponent():
    /// - All controls exist and have their XAML-defined properties
    /// - Data bindings are set up
    /// - DataContext is set (usually to a ViewModel)
    /// </para>
    /// </remarks>
    public MainWindow()
    {
        // Parse XAML and create UI controls
        // This must be called before accessing any XAML-defined elements
        InitializeComponent();

        // =====================================================================
        // In MVVM, that's typically all we need!
        // =====================================================================
        //
        // The XAML file sets DataContext to MainViewModel:
        //   <Window.DataContext>
        //       <vm:MainViewModel />
        //   </Window.DataContext>
        //
        // All UI elements bind to ViewModel properties:
        //   <TextBox Text="{Binding IsaAltitude}" />
        //   <Button Command="{Binding CalculateIsaCommand}" />
        //
        // No event handlers needed - Commands handle user actions.
        // No manual updates needed - PropertyChanged updates UI.
    }
}
