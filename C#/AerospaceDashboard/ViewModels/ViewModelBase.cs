// =============================================================================
// VIEWMODEL BASE CLASS AND COMMAND INFRASTRUCTURE
// =============================================================================
//
// C# / WPF CONCEPT: The MVVM Pattern
// ===================================
//
// MVVM (Model-View-ViewModel) is the standard architecture for WPF apps:
//
//     +-------------+       +---------------+       +-------------+
//     |    VIEW     |<----->|   VIEWMODEL   |<----->|    MODEL    |
//     | (XAML/UI)   |       | (C# Logic)    |       | (Data/Biz)  |
//     +-------------+       +---------------+       +-------------+
//
//     Bindings &           INotifyPropertyChanged    Pure data &
//     Commands             ICommand                  calculations
//
// WHY MVVM?
// ---------
// 1. SEPARATION OF CONCERNS: UI code separate from logic
// 2. TESTABILITY: ViewModels can be unit tested without UI
// 3. DESIGNER/DEVELOPER SPLIT: Designers work on XAML, devs on C#
// 4. BINDABILITY: WPF's data binding works naturally with ViewModels
//
// THE KEY INTERFACES:
// ------------------
// - INotifyPropertyChanged: ViewModel tells View when data changes
// - ICommand: View tells ViewModel when user takes action (button clicks)
//
// DATA FLOW EXAMPLE:
// -----------------
// 1. User types in TextBox (View)
// 2. Binding updates ViewModel property
// 3. Property setter calls SetProperty() -> PropertyChanged event
// 4. Other bound controls update automatically
//
// COMMAND FLOW EXAMPLE:
// --------------------
// 1. User clicks Button (View)
// 2. Button's Command binding invokes ICommand.Execute()
// 3. RelayCommand delegates to Action in ViewModel
// 4. ViewModel updates data, raises PropertyChanged
// 5. View updates automatically
//
// =============================================================================
// C# CONCEPTS IN THIS FILE
// =============================================================================
//
// 1. ABSTRACT CLASS
//    `public abstract class ViewModelBase`
//    Cannot be instantiated directly. Provides common functionality
//    for derived classes. Subclasses MUST be created to use it.
//
// 2. INTERFACE IMPLEMENTATION
//    `ViewModelBase : INotifyPropertyChanged`
//    The class promises to implement PropertyChanged event.
//
// 3. NULLABLE EVENT
//    `public event PropertyChangedEventHandler? PropertyChanged;`
//    The ? indicates the event may have no subscribers (null).
//
// 4. CALLER MEMBER NAME ATTRIBUTE
//    `[CallerMemberName] string? propertyName = null`
//    Compiler automatically fills in the calling property's name.
//    Reduces errors from hardcoded strings.
//
// 5. NULL-CONDITIONAL OPERATOR
//    `PropertyChanged?.Invoke(...)`
//    Only invokes if PropertyChanged is not null (has subscribers).
//
// 6. GENERIC METHOD
//    `SetProperty<T>(ref T field, T value, ...)`
//    Works with any type T. Compiler infers T from arguments.
//
// 7. REF PARAMETER
//    `ref T field`
//    Passes the variable by reference so we can modify it.
//
// 8. READONLY FIELDS
//    `private readonly Action<object?> _execute;`
//    Can only be set in constructor. Immutable after that.
//
// =============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AerospaceDashboard.ViewModels;

/// <summary>
/// Base class for all ViewModels, implementing INotifyPropertyChanged.
/// </summary>
/// <remarks>
/// <para>
/// C# CONCEPT: Abstract Base Class
/// Abstract classes cannot be instantiated directly - you must create
/// a subclass. They're used to share common functionality.
/// </para>
/// <para>
/// This class provides the "plumbing" for data binding:
/// - PropertyChanged event that WPF listens to
/// - SetProperty helper that updates fields and raises events
/// </para>
/// </remarks>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    // =========================================================================
    // INOTIFYPROPERTYCHANGED IMPLEMENTATION
    // =========================================================================

    /// <summary>
    /// Event raised when a property value changes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// C# CONCEPT: Nullable Event (PropertyChangedEventHandler?)
    /// The ? indicates this event may have no subscribers.
    /// We must check for null before invoking.
    /// </para>
    /// <para>
    /// WPF data binding subscribes to this event. When we raise it,
    /// WPF knows to update the UI for that property.
    /// </para>
    /// </remarks>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raise the PropertyChanged event for a property.
    /// </summary>
    /// <param name="propertyName">Name of the changed property</param>
    /// <remarks>
    /// <para>
    /// C# CONCEPT: CallerMemberName Attribute
    /// When you call OnPropertyChanged() from a property setter,
    /// the compiler automatically fills in the property name.
    /// No more error-prone hardcoded strings!
    ///
    /// Example:
    ///   public string Name
    ///   {
    ///       set { _name = value; OnPropertyChanged(); }  // "Name" auto-filled
    ///   }
    /// </para>
    /// <para>
    /// C# CONCEPT: Null-Conditional Operator (?.)
    /// `PropertyChanged?.Invoke(...)` only invokes if not null.
    /// Equivalent to: if (PropertyChanged != null) PropertyChanged.Invoke(...)
    /// </para>
    /// </remarks>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        // Only invoke if someone is listening (not null)
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Set a field value and raise PropertyChanged if the value changed.
    /// </summary>
    /// <typeparam name="T">The type of the property</typeparam>
    /// <param name="field">Reference to the backing field</param>
    /// <param name="value">The new value</param>
    /// <param name="propertyName">Property name (auto-filled by compiler)</param>
    /// <returns>True if the value changed, false if it was the same</returns>
    /// <remarks>
    /// <para>
    /// C# CONCEPT: Generic Method with Type Parameter
    /// The &lt;T&gt; makes this work with any type: int, string, complex objects.
    /// The compiler infers T from the arguments.
    /// </para>
    /// <para>
    /// C# CONCEPT: Ref Parameter
    /// `ref T field` passes the variable by reference.
    /// This lets us modify the caller's variable directly.
    /// Without 'ref', we'd only get a copy.
    /// </para>
    /// <para>
    /// This pattern is standard in WPF MVVM. It:
    /// 1. Checks if value actually changed (optimization)
    /// 2. Updates the backing field
    /// 3. Raises PropertyChanged event
    /// 4. Returns whether anything changed
    /// </para>
    /// </remarks>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        // Don't do anything if value hasn't changed
        // Uses EqualityComparer for proper null handling and value type comparison
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        // Update the backing field
        field = value;

        // Notify WPF that the property changed
        OnPropertyChanged(propertyName);

        // Return true to indicate value changed
        return true;
    }
}

/// <summary>
/// ICommand implementation that delegates to Action/Func delegates.
/// </summary>
/// <remarks>
/// <para>
/// C# / WPF CONCEPT: The Command Pattern
/// Commands represent user actions (button clicks, menu selections).
/// WPF buttons bind to ICommand properties instead of event handlers.
///
/// Benefits:
/// - Command logic lives in ViewModel, not code-behind
/// - Commands can be enabled/disabled via CanExecute
/// - Commands can be reused (same command on menu and toolbar)
/// </para>
/// <para>
/// This "RelayCommand" pattern delegates Execute/CanExecute to
/// delegates provided at construction time. Very flexible.
/// </para>
/// </remarks>
public class RelayCommand : System.Windows.Input.ICommand
{
    // =========================================================================
    // FIELDS
    // =========================================================================
    // C# CONCEPT: Readonly Fields
    // Fields marked 'readonly' can only be set in the constructor.
    // This makes the object safer and indicates design intent.

    /// <summary>The action to execute when command is invoked</summary>
    private readonly Action<object?> _execute;

    /// <summary>Optional predicate to determine if command can execute</summary>
    private readonly Func<object?, bool>? _canExecute;

    // =========================================================================
    // CONSTRUCTOR
    // =========================================================================

    /// <summary>
    /// Create a new RelayCommand.
    /// </summary>
    /// <param name="execute">Action to execute (required)</param>
    /// <param name="canExecute">Optional predicate for CanExecute</param>
    /// <remarks>
    /// <para>
    /// C# CONCEPT: Null-Coalescing Throw
    /// `execute ?? throw new ArgumentNullException(...)`
    /// If execute is null, throw an exception. Otherwise use the value.
    /// </para>
    /// <para>
    /// Usage examples:
    ///   new RelayCommand(_ => DoSomething())           // No parameter, always enabled
    ///   new RelayCommand(p => DoSomething(p))          // Uses parameter
    ///   new RelayCommand(_ => DoIt(), _ => CanDoIt())  // With CanExecute
    /// </para>
    /// </remarks>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    // =========================================================================
    // ICOMMAND IMPLEMENTATION
    // =========================================================================

    /// <summary>
    /// Event raised when CanExecute may have changed.
    /// </summary>
    /// <remarks>
    /// C# CONCEPT: Event Accessor Syntax (add/remove)
    /// Instead of storing our own subscribers, we forward to
    /// CommandManager.RequerySuggested. WPF automatically raises
    /// this event when it thinks command states may have changed.
    ///
    /// This is a common pattern that reduces boilerplate.
    /// </remarks>
    public event EventHandler? CanExecuteChanged
    {
        add => System.Windows.Input.CommandManager.RequerySuggested += value;
        remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    /// Determine if the command can be executed.
    /// </summary>
    /// <param name="parameter">Command parameter from XAML binding</param>
    /// <returns>True if command can execute</returns>
    /// <remarks>
    /// C# CONCEPT: Null-Conditional with Null-Coalescing
    /// `_canExecute?.Invoke(parameter) ?? true`
    ///
    /// If _canExecute is null, return true (always enabled).
    /// Otherwise, call _canExecute and return its result.
    /// </remarks>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <summary>
    /// Execute the command action.
    /// </summary>
    /// <param name="parameter">Command parameter from XAML binding</param>
    public void Execute(object? parameter) => _execute(parameter);
}
